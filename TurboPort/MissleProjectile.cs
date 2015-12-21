#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using TurboPort.ParticleSystems;

#endregion

namespace TurboPort
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    internal class MissleProjectile
    {
        #region Constants

        const float trailParticlesPerSecond = 200;
        const float projectileLifespan = 1.5f;
        private static readonly VelocityPositionCalculator VelocityPositionCalculator = new VelocityPositionCalculator { Mass = 5 };

        #endregion

        #region Fields

        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        MissleParticleEmitter trailEmitter;

        Vector3 position;
        Vector3 velocity;
        float age;
        private float shootingAngleInDegrees;

        #endregion

        public Vector3 Position { get { return position; } }

        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public MissleProjectile(ParticleSystem explosionParticles, ParticleSystem explosionSmokeParticles, ParticleSystem projectileTrailParticles, Vector3 position, float shootingAngleInDegrees, Vector3 velocity)
        {
            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;

            // Start at the origin, firing in a random (but roughly upward) direction.
            this.position = position;
            this.shootingAngleInDegrees = shootingAngleInDegrees;

            this.velocity = velocity;
            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new MissleParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, this.position);
        }

        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float thrust = age == 0 ? 430 : 1;
            // Simple projectile physics.
            VelocityPositionCalculator.CalcVelocityAndPosition(ref position, ref velocity, elapsedTime, shootingAngleInDegrees, thrust);
            age += elapsedTime;

            // Update the particle emitter, which will create our particle trail.
            trailEmitter.Update(gameTime, position);

            // If enough time has passed, explode! Note how we pass our velocity
            // in to the AddParticle method: this lets the explosion be influenced
            // by the speed and direction of the projectile which created it.
            if (age > projectileLifespan)
            {
                return false;
            }
                
            return true;
        }

        public void Explode()
        {
            const int numExplosionParticles = 30;
            const int numExplosionSmokeParticles = 50;

            for (int i = 0; i < numExplosionParticles; i++)
                explosionParticles.AddParticle(position, Vector3.Zero);

            for (int i = 0; i < numExplosionSmokeParticles; i++)
                explosionSmokeParticles.AddParticle(position, Vector3.Zero);
        }
    }
}
