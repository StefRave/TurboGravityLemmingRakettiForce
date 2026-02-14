using System.IO;
using FakeItEasy;
using NUnit.Framework;
using TurboPort.Event;

namespace TurboPort.Test
{
    [TestFixture]
    public class TestSerializing
    {
        [Test]
        public void TestObjectShipHasLanded()
        {
            var missileProjectileFactory = A.Fake<IMissileProjectileFactory>();
            var gameObjectStore = new GameObjectStore();
            gameObjectStore.RegisterCreation(() => new ObjectShip(missileProjectileFactory));


            var objectShip = gameObjectStore.CreateAsOwner<ObjectShip>();

            objectShip.LandShip(10);
            objectShip.HitWithBackground();

            MemoryStream gameEvents = new MemoryStream();
            gameObjectStore.SerializeModifiedObjects(gameEvents);
            gameObjectStore.ClearRecordedObjects();

            gameEvents.Position = 0;
            var replay = new GameReplay(gameObjectStore);
            replay.Load(gameEvents);
            replay.StartPlay(0);
            replay.ProcessEventsUntilTime(1000);


            //var x = new ObjectShipState { GameTime = 1.2, ObjectId = 2, Position = new Vector3(1, 2, 3), Events = ObjectShipState.Event.HitBackground | ObjectShipState.Event.Create };
            //s.Serialize(file, x);
            //s.Serialize(file, x);

            //file.Position = 0;
            //var y = (ObjectShipState)s.Deserialize(file);
            //Assert.AreEqual(x.Position, y.Position);
            //Assert.AreEqual(x.GameTime, y.GameTime);
            //Assert.AreEqual(x.ObjectId, y.ObjectId);
            //Assert.AreEqual(x.Events, y.Events);
        }

        [Test]
        public void TestSerializingActions()
        {
            bool actionPerformed = false;
            var gameObjectStore = new GameObjectStore();
            gameObjectStore.SubscribeToGameMessage<AnybodyThereGameMessage>(msg => actionPerformed = true);

            gameObjectStore.AddMessage(new AnybodyThereGameMessage());

            MemoryStream gameEvents = new MemoryStream();
            gameObjectStore.SerializeModifiedObjects(gameEvents);
            gameObjectStore.ClearRecordedObjects();
            gameEvents.Position = 0;

            var replay = new GameReplay(gameObjectStore);
            replay.Load(gameEvents);
            replay.StartPlay(0);
            replay.ProcessEventsUntilTime(1000);

            Assert.That(actionPerformed, Is.True);
        }

    }
}
