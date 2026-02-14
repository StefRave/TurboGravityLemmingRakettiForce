using FakeItEasy;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace TurboPort.Test
{
    [TestFixture]
    public class TestShipBase
    {
        private IMissileProjectileFactory fakeProjectileFactory;
        private ShipBase shipBase;

        [SetUp]
        public void SetUp()
        {
            // Ensure sound doesn't cause issues in tests
            SoundHandler.Current = NullSoundHandler.Instance;
            fakeProjectileFactory = A.Fake<IMissileProjectileFactory>();
            shipBase = new ShipBase(new Vector3(100, 500, 0));
        }

        [Test]
        public void Ship_AlreadyLanded_NoInteraction()
        {
            var ship = CreateShipAtPosition(100, 500);
            // CreateInitialize sets HasLanded = true

            ship.HasLanded.ShouldBeTrue();
            shipBase.Interact(ship);
            // Ship should still be landed (no change)
            ship.HasLanded.ShouldBeTrue();
        }

        [Test]
        public void Ship_MovingUp_NoLanding()
        {
            var ship = CreateFlyingShip(100, 500);
            // After takeoff, ship gains upward velocity from thrust
            // Velocity.Y should be positive (upward), so Interact won't land

            // Verify preconditions
            ship.HasLanded.ShouldBeFalse();
            ship.Velocity.Y.ShouldBeGreaterThan(0, "Ship should be moving upward after thrust");

            shipBase.Interact(ship);
            // Ship should not have landed because velocity.Y >= 0
            ship.HasLanded.ShouldBeFalse();
        }

        [Test]
        public void Ship_TooFarHorizontally_NoLanding()
        {
            // Ship is 20 pixels away from base (threshold is 16)
            var ship = CreateDescendingShip(120, 500);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeFalse();
        }

        [Test]
        public void Ship_AboveBase_NoLanding()
        {
            // Ship is too high above the base
            var ship = CreateDescendingShip(100, 505);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeFalse();
        }

        [Test]
        public void Ship_BelowBase_NoLanding()
        {
            // Ship is too far below the base
            var ship = CreateDescendingShip(100, 495);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeFalse();
        }

        [Test]
        public void Ship_CorrectPosition_DescendingStraight_Lands()
        {
            // Ship at the right position, descending, oriented upright
            var ship = CreateDescendingShip(100, 500);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeTrue();
        }

        [Test]
        public void Ship_CorrectPosition_TooTilted_NoLanding()
        {
            // Ship in position but tilted too much
            var ship = CreateDescendingShipWithTilt(100, 500, tiltAngle: 0.5f);
            // cos(0.5) ≈ 0.877 < 0.90 threshold

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeFalse();
        }

        [Test]
        public void Ship_CorrectPosition_SlightlyTilted_Lands()
        {
            // cos(0.3) ≈ 0.955 > 0.90 threshold
            var ship = CreateDescendingShipWithTilt(100, 500, tiltAngle: 0.3f);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeTrue();
        }

        [Test]
        public void Ship_WithinHorizontalThreshold_Lands()
        {
            // Ship is 15 pixels away (threshold is abs < 16)
            var ship = CreateDescendingShip(115, 500);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeTrue();
        }

        [Test]
        public void Ship_ExactlyAtHorizontalThreshold_NoLanding()
        {
            // Ship is exactly 16 pixels away (threshold is abs >= 16)
            var ship = CreateDescendingShip(116, 500);

            shipBase.Interact(ship);
            ship.HasLanded.ShouldBeFalse();
        }

        private ObjectShip CreateShipAtPosition(float x, float y)
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(x, y, 0));
            return ship;
        }

        /// <summary>
        /// Creates a ship that is not landed and has upward velocity.
        /// </summary>
        private ObjectShip CreateFlyingShip(float x, float y)
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(x, y, 0));
            ship.HasLanded = false;
            ship.SetVelocity(new Vector3(0, 5, 0));
            return ship;
        }

        /// <summary>
        /// Creates a ship that is flying and descending (negative Y velocity)
        /// at the given position.
        /// </summary>
        private ObjectShip CreateDescendingShip(float x, float y)
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(x, y, 0));
            ship.HasLanded = false;
            ship.SetVelocity(new Vector3(0, -5, 0));
            return ship;
        }

        /// <summary>
        /// Creates a descending ship with a specific rotation tilt.
        /// </summary>
        private ObjectShip CreateDescendingShipWithTilt(float x, float y, float tiltAngle)
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(x, y, 0));
            ship.HasLanded = false;
            ship.SetVelocity(new Vector3(0, -5, 0));
            ship.SetRotation(new Vector3(0, 0, tiltAngle));
            return ship;
        }
    }
}
