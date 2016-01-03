using System.IO;
using FakeItEasy;
using Microsoft.Xna.Framework;
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

            GameObjectStore.RegisterCreation(() => new ObjectShip(missileProjectileFactory));

            var objectShip = GameObjectStore.CreateAsOwner<ObjectShip>();

            objectShip.LandShip(10);
            objectShip.HitWithBackground();
            GameObjectStore.StoreModifiedObjects();

            var ms = new MemoryStream();
            GameObjectStore.Store(ms);
            ms.Position = 0;

            GameObjectStore.Clear();
            var replay = new GameReplay();
            replay.Load(ms);
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


    }
}
