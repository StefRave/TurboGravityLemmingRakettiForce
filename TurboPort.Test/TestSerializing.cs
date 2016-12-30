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

            var sut = CreateSutWithRegistrations(missileProjectileFactory);

            var objectShip = sut.CreateAsOwner<ObjectShip>();

            objectShip.LandShip(10);
            objectShip.HitWithBackground();
            MemoryStream gameEvents = new MemoryStream();

            sut.StoreModifiedObjects(gameEvents);

            gameEvents.Position = 0;
            sut = CreateSutWithRegistrations(missileProjectileFactory);
            var replay = new GameReplay(sut);
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

        private static GameObjectStore CreateSutWithRegistrations(IMissileProjectileFactory missileProjectileFactory)
        {
            GameObjectStore sut = new GameObjectStore();
            sut.RegisterCreation(() => new ObjectShip(missileProjectileFactory));
            return sut;
        }
    }
}
