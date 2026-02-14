using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace TurboPort.Test
{
    [TestFixture]
    public class TestVelocityPositionCalculator
    {
        private VelocityPositionCalculator sut;

        [SetUp]
        public void SetUp()
        {
            sut = new VelocityPositionCalculator
            {
                Mass = 35,
                Drag = 0.7f,
                Gravity = 80
            };
        }

        [Test]
        public void ZeroThrust_ObjectFallsDown()
        {
            var position = Vector3.Zero;
            var velocity = Vector3.Zero;

            sut.CalcVelocityAndPosition(ref position, ref velocity, 0.016, thrustAngle: 0, thrust: 0);

            // With zero thrust and gravity=80, the object should start falling (negative Y velocity)
            velocity.Y.ShouldBeLessThan(0, "Object should gain downward velocity due to gravity");
            position.Y.ShouldBeLessThan(0, "Object should move downward due to gravity");
            // No horizontal force, so X should stay at zero
            velocity.X.ShouldBe(0, tolerance: 0.001f);
            position.X.ShouldBe(0, tolerance: 0.001f);
        }

        [Test]
        public void FullThrustUpward_CounteractsGravity()
        {
            var position = Vector3.Zero;
            var velocity = Vector3.Zero;

            // Thrust angle 0 means thrust is pointing straight up (cos(0)=1)
            // With enough thrust, the object should move upward
            sut.CalcVelocityAndPosition(ref position, ref velocity, 0.016, thrustAngle: 0, thrust: 100);

            // thrust/Mass*100 = 100/35*100 ≈ 285.7 which is greater than gravity=80
            velocity.Y.ShouldBeGreaterThan(0, "Upward thrust should exceed gravity");
        }

        [Test]
        public void ThrustAtAngle_MovesHorizontally()
        {
            var position = Vector3.Zero;
            var velocity = Vector3.Zero;

            // Thrust angle = pi/2 (90 degrees) means thrust pointing right
            float angle = (float)(System.Math.PI / 2);
            sut.CalcVelocityAndPosition(ref position, ref velocity, 0.016, thrustAngle: angle, thrust: 100);

            // sin(-pi/2) = -1, so horizontal component should push in X direction
            velocity.X.ShouldNotBe(0, "Should have horizontal velocity when thrusting sideways");
        }

        [Test]
        public void DragSlowsDownMovingObject()
        {
            var position = Vector3.Zero;
            var velocity = new Vector3(100, 0, 0);

            // No thrust, just drag on a moving object
            sut.CalcVelocityAndPosition(ref position, ref velocity, 0.016, thrustAngle: 0, thrust: 0);

            // Velocity should decrease due to drag
            velocity.X.ShouldBeLessThan(100, "Drag should reduce horizontal velocity");
            velocity.X.ShouldBeGreaterThan(0, "Velocity should still be positive");
        }

        [Test]
        public void SymmetricThrustAngles_ProduceOppositeHorizontalMotion()
        {
            var pos1 = Vector3.Zero;
            var vel1 = Vector3.Zero;
            var pos2 = Vector3.Zero;
            var vel2 = Vector3.Zero;

            float angle = 0.5f;
            sut.CalcVelocityAndPosition(ref pos1, ref vel1, 0.016, thrustAngle: angle, thrust: 50);
            sut.CalcVelocityAndPosition(ref pos2, ref vel2, 0.016, thrustAngle: -angle, thrust: 50);

            // Horizontal components should be opposite
            vel1.X.ShouldBe(-vel2.X, tolerance: 0.001f);
            // Vertical components should be equal (same cos)
            vel1.Y.ShouldBe(vel2.Y, tolerance: 0.001f);
        }

        [Test]
        public void MultipleTimeSteps_PositionAccumulates()
        {
            var position = Vector3.Zero;
            var velocity = Vector3.Zero;

            // Apply multiple steps with constant thrust
            for (int i = 0; i < 10; i++)
            {
                sut.CalcVelocityAndPosition(ref position, ref velocity, 0.016, thrustAngle: 0, thrust: 50);
            }

            // After 10 steps of upward thrust exceeding gravity, position.Y should be positive
            // thrust/Mass*100 = 50/35*100 ≈ 142.8 > gravity=80
            position.Y.ShouldBeGreaterThan(0, "Accumulated upward thrust should move object up");
        }

        [Test]
        public void ZeroElapsedTime_NoChange()
        {
            var position = new Vector3(10, 20, 0);
            var velocity = new Vector3(5, 5, 0);
            var origPos = position;
            var origVel = velocity;

            sut.CalcVelocityAndPosition(ref position, ref velocity, 0, thrustAngle: 0, thrust: 100);

            position.ShouldBe(origPos);
            velocity.X.ShouldBe(origVel.X, tolerance: 0.001f);
            velocity.Y.ShouldBe(origVel.Y, tolerance: 0.001f);
        }

        [Test]
        public void HighMass_ReducesThrustEffect()
        {
            var pos1 = Vector3.Zero;
            var vel1 = Vector3.Zero;
            var pos2 = Vector3.Zero;
            var vel2 = Vector3.Zero;

            sut.Mass = 10;
            sut.CalcVelocityAndPosition(ref pos1, ref vel1, 0.016, thrustAngle: 0, thrust: 50);

            sut.Mass = 100;
            sut.CalcVelocityAndPosition(ref pos2, ref vel2, 0.016, thrustAngle: 0, thrust: 50);

            // Higher mass should result in less upward velocity
            // Both are affected equally by gravity, but thrust effect differs
            vel1.Y.ShouldBeGreaterThan(vel2.Y, "Lower mass should have higher upward velocity from thrust");
        }

        [Test]
        public void ConsistencyCheck_SmallVsLargeTimestep()
        {
            // Compare one large step vs many small steps — the ODE solver should give similar results
            var posLarge = Vector3.Zero;
            var velLarge = Vector3.Zero;
            sut.CalcVelocityAndPosition(ref posLarge, ref velLarge, 0.1, thrustAngle: 0.3f, thrust: 50);

            var posSmall = Vector3.Zero;
            var velSmall = Vector3.Zero;
            int steps = 100;
            for (int i = 0; i < steps; i++)
            {
                sut.CalcVelocityAndPosition(ref posSmall, ref velSmall, 0.001, thrustAngle: 0.3f, thrust: 50);
            }

            // The analytical ODE solver should produce consistent results regardless of timestep
            posLarge.X.ShouldBe(posSmall.X, tolerance: 0.5f);
            posLarge.Y.ShouldBe(posSmall.Y, tolerance: 0.5f);
            velLarge.X.ShouldBe(velSmall.X, tolerance: 0.5f);
            velLarge.Y.ShouldBe(velSmall.Y, tolerance: 0.5f);
        }
    }
}
