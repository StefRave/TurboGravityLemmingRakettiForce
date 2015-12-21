using System.IO;
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
            var s = new GameSerializer();
            s.Initialize();

            var file = new MemoryStream();
            var x = new ObjectShipHasLanded { GameTime = 1.2, ObjectId = 2, Position = new Vector3(1, 2, 3) };
            s.Serialize(file, x);
            s.Serialize(file, x);

            file.Position = 0;
            var y = (ObjectShipHasLanded)s.Deserialize(file);
            Assert.AreEqual(x.Position, y.Position);
            Assert.AreEqual(x.GameTime, y.GameTime);
            Assert.AreEqual(x.ObjectId, y.ObjectId);
        }

        [Test]
        public void TestObjectShipControlChanged()
        {
            var s = new GameSerializer();
            s.Initialize();

            var file = new MemoryStream();
            var x = new ObjectShipControlChanged { GameTime = 1.2, ObjectId = 2, Rotation = 3, Thrust = 5 };
            s.Serialize(file, x);
            s.Serialize(file, x);

            file.Position = 0;
            var y = (ObjectShipControlChanged)s.Deserialize(file);
            Assert.AreEqual(x.Rotation, y.Rotation);
            Assert.AreEqual(x.Thrust, y.Thrust);
            Assert.AreEqual(x.GameTime, y.GameTime);
            Assert.AreEqual(x.ObjectId, y.ObjectId);
        }
    }
}
