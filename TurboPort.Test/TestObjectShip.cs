using System;
using FakeItEasy;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;
using TurboPort.Input;

namespace TurboPort.Test
{
    [TestFixture]
    public class TestObjectShip
    {
        private IMissileProjectileFactory fakeProjectileFactory;

        [SetUp]
        public void SetUp()
        {
            SoundHandler.Current = NullSoundHandler.Instance;
            fakeProjectileFactory = A.Fake<IMissileProjectileFactory>();
        }

        [Test]
        public void CreateInitialize_SetsPosition()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            var startPos = new Vector3(100, 200, 0);

            ship.CreateInitialize(startPos);

            ship.Position.ShouldBe(startPos);
            ship.HasLanded.ShouldBeTrue();
        }

        [Test]
        public void CreateInitialize_ShipIsLanded()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(50, 50, 0));

            ship.HasLanded.ShouldBeTrue();
            ship.Velocity.ShouldBe(Vector3.Zero);
        }

        [Test]
        public void Update_LandedShip_NoMovement()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            var gameTime = new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.016));
            ship.Update(gameTime);

            // A landed ship with no thrust should not move
            ship.Position.X.ShouldBe(100, tolerance: 0.01f);
            ship.Position.Y.ShouldBe(200, tolerance: 0.01f);
        }

        [Test]
        public void Update_WithThrust_ShipTakesOff()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Apply thrust input
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 0 });

            var gameTime = new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.016));
            ship.Update(gameTime);

            ship.HasLanded.ShouldBeFalse("Ship should take off when thrust is applied");
        }

        [Test]
        public void Update_WithThrust_ShipMovesUp()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 0 });

            float startY = ship.Position.Y;
            var gameTime = new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016));
            ship.Update(gameTime);

            // With full thrust and ship mass 35: thrust*100/Mass*100 = 285.7 > gravity(80)
            ship.Position.Y.ShouldBeGreaterThan(startY, "Ship should move up with full upward thrust");
        }

        [Test]
        public void Update_NoThrust_GravityPullsDown()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off first
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 0 });
            ship.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));

            float posAfterTakeoff = ship.Position.Y;

            // Now remove thrust and let gravity act
            ship.ProcessControllerInput(new PlayerControl { Thrust = 0, Rotation = 0 });
            for (int i = 0; i < 100; i++)
            {
                ship.Update(new GameTime(TimeSpan.FromSeconds(1 + i * 0.016), TimeSpan.FromSeconds(0.016)));
            }

            ship.Velocity.Y.ShouldBeLessThan(0, "Gravity should pull ship downward");
        }

        [Test]
        public void LandShip_StopsMovement()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 0 });
            ship.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));

            ship.HasLanded.ShouldBeFalse();

            // Land
            ship.LandShip(200);

            ship.HasLanded.ShouldBeTrue();
            ship.Velocity.ShouldBe(Vector3.Zero);
            ship.Rotation.Z.ShouldBe(0);
            ship.Position.Y.ShouldBe(200);
        }

        [Test]
        public void HitWithBackground_ReducesVelocity()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off and gain some speed
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 0 });
            for (int i = 0; i < 10; i++)
            {
                ship.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
            }

            var velocityBefore = ship.Velocity;
            ship.HitWithBackground();

            // Velocity should be reduced to 30%
            ship.Velocity.X.ShouldBe(velocityBefore.X * 0.3f, tolerance: 0.01f);
            ship.Velocity.Y.ShouldBe(velocityBefore.Y * 0.3f, tolerance: 0.01f);
        }

        [Test]
        public void ProcessControllerInput_Fire_CallsProjectileFactory()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off first so the ship is active
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 0, Fire = false });
            ship.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));

            // Now fire
            ship.ProcessControllerInput(new PlayerControl { Thrust = 0, Rotation = 0, Fire = true });

            A.CallTo(() => fakeProjectileFactory.Fire(
                A<Vector3>.Ignored, A<float>.Ignored, A<Vector3>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ProcessControllerInput_HoldFire_OnlyFiresOnce()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Fire = false });
            ship.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));

            // Fire first press
            ship.ProcessControllerInput(new PlayerControl { Fire = true });
            // Hold fire (same state)
            ship.ProcessControllerInput(new PlayerControl { Fire = true });
            ship.ProcessControllerInput(new PlayerControl { Fire = true });

            // Should only fire once on the transition from false to true
            A.CallTo(() => fakeProjectileFactory.Fire(
                A<Vector3>.Ignored, A<float>.Ignored, A<Vector3>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void ProcessControllerInput_ReleaseAndFireAgain_FiresTwice()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Fire = false });
            ship.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));

            // Fire, release, fire again
            ship.ProcessControllerInput(new PlayerControl { Fire = true });
            ship.ProcessControllerInput(new PlayerControl { Fire = false });
            ship.ProcessControllerInput(new PlayerControl { Fire = true });

            A.CallTo(() => fakeProjectileFactory.Fire(
                A<Vector3>.Ignored, A<float>.Ignored, A<Vector3>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void Bots_ShipsClose_CollisionOccurs()
        {
            var ship1 = new ObjectShip(fakeProjectileFactory);
            var ship2 = new ObjectShip(fakeProjectileFactory);

            // Position ships very close together (within bounding sphere radius)
            ship1.CreateInitialize(new Vector3(100, 200, 0));
            ship2.CreateInitialize(new Vector3(102, 200, 0)); // Within default radius of 5

            // Take off both ships so they're not landed (landed ships have 10x mass)
            ship1.ProcessControllerInput(new PlayerControl { Thrust = 1.0f });
            ship2.ProcessControllerInput(new PlayerControl { Thrust = 1.0f });
            var gt = new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016));
            ship1.Update(gt);
            ship2.Update(gt);

            // Give ship1 some horizontal velocity to make collision visible
            // Apply lateral thrust through rotation
            ship1.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 1.0f });
            ship1.Update(new GameTime(TimeSpan.FromSeconds(0.032), TimeSpan.FromSeconds(0.016)));

            var vel1Before = ship1.Velocity;

            ship1.Bots(ship2);

            // The ships are within bounding sphere radius (5), distance is 2
            // After elastic collision, velocities should exchange/change
            (ship1.Velocity != vel1Before).ShouldBeTrue("Ship-to-ship collision should change velocities");
        }

        [Test]
        public void Bots_ShipsFarApart_NoCollision()
        {
            var ship1 = new ObjectShip(fakeProjectileFactory);
            var ship2 = new ObjectShip(fakeProjectileFactory);

            ship1.CreateInitialize(new Vector3(100, 200, 0));
            ship2.CreateInitialize(new Vector3(200, 200, 0)); // Far apart

            // Take off both
            ship1.ProcessControllerInput(new PlayerControl { Thrust = 1.0f });
            ship2.ProcessControllerInput(new PlayerControl { Thrust = 1.0f });
            ship1.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));
            ship2.Update(new GameTime(TimeSpan.FromSeconds(0.016), TimeSpan.FromSeconds(0.016)));

            var vel1Before = ship1.Velocity;
            var vel2Before = ship2.Velocity;

            ship1.Bots(ship2);

            // Velocities should be unchanged
            ship1.Velocity.ShouldBe(vel1Before);
            ship2.Velocity.ShouldBe(vel2Before);
        }

        [Test]
        public void Rotation_AccumulatesOverTime()
        {
            var ship = new ObjectShip(fakeProjectileFactory);
            ship.CreateInitialize(new Vector3(100, 200, 0));

            // Take off with rotation
            ship.ProcessControllerInput(new PlayerControl { Thrust = 1.0f, Rotation = 1.0f });

            for (int i = 0; i < 10; i++)
            {
                ship.Update(new GameTime(TimeSpan.FromSeconds(i * 0.016), TimeSpan.FromSeconds(0.016)));
            }

            ship.Rotation.Z.ShouldNotBe(0, "Ship should have rotated");
        }
    }
}
