using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;
using TurboPort.Event;

namespace TurboPort.Test
{
    [TestFixture]
    public class TestMasterControl
    {
        private GameObjectStore gameStore;
        private GameWorld gameWorld;
        private DelayServiceMock delayServiceMock;
        private MasterControl sut;

        [SetUp]
        public void SetUp()
        {
            gameStore = new GameObjectStore();
            gameWorld = new GameWorld(new Game(), gameStore);
            delayServiceMock = new DelayServiceMock();
            sut = new MasterControl(gameStore, gameWorld, delayServiceMock) {GameMode = GameMode.Multiplayer};


            gameWorld.AddShipBase(new ShipBase(Vector3.Left));
            gameWorld.AddShipBase(new ShipBase(Vector3.Right));

            gameStore.RegisterCreation(
                () =>
                {
                    var objectShip = new ObjectShip(gameWorld.ProjectileFactory);
                    gameWorld.AddPlayerShip(objectShip);
                    return objectShip;
                });

            var synchronizationContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        }

        [Test]
        public async Task Test()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task task = sut.Start(cancellationTokenSource.Token);

            await delayServiceMock.WaitForDelayCall();
            gameStore.GameMessages.Count.ShouldBe(1);
            var anybodyThereGameMessage = gameStore.GameMessages.OfType<AnybodyThereGameMessage>().FirstOrDefault();
            anybodyThereGameMessage.ShouldNotBeNull();
            gameStore.ClearRecordedObjects();

            gameStore.InvokeGameMessageAction(new MasterIsHere());
            delayServiceMock.Continue();

            await task;
            gameStore.GameMessages.Count.ShouldBe(1);
            var gameStateRequest = gameStore.GameMessages.OfType<GameStateRequest>().FirstOrDefault();
            gameStateRequest.ShouldNotBeNull();
            gameStore.ClearRecordedObjects();

            //await delayServiceMock.WaitForDelayCall();

            cancellationTokenSource.Cancel();
        }
    }
}
