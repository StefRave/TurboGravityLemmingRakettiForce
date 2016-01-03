#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using ProtoBuf;
using TurboPort.Event;
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
    [ProtoContract()]
    [GameEvent("Mis1")]
    internal class MissileProjectile : GameObject
    {
        #region Constants

        const float trailParticlesPerSecond = 200;
        const float projectileLifespan = 1.5f;
        private static readonly VelocityPositionCalculator VelocityPositionCalculator = new VelocityPositionCalculator { Mass = 5 };

        #endregion

        #region Fields

        private readonly ILevelBackground levelBackground;
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        private readonly ParticleSystem projectileTrailParticles;
        private readonly Action<MissileProjectile> removeAction;
        MissileParticleEmitter trailEmitter;

        [ProtoMember(1)] Vector3 position;
        [ProtoMember(2)] Vector3 velocity;
        [ProtoMember(3)] private float shootingAngleInDegrees;
        private double createdTime;
        [ProtoMember(4)] private bool explode;
        #endregion

        public Vector3 Position { get { return position; } }

        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public MissileProjectile(ILevelBackground levelBackground, ParticleSystem explosionParticles, ParticleSystem explosionSmokeParticles, ParticleSystem projectileTrailParticles, Action<MissileProjectile> removeAction)
        {
            this.levelBackground = levelBackground;
            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            this.projectileTrailParticles = projectileTrailParticles;
            this.removeAction = removeAction;
        }

        public void CreateInitialize(Vector3 position, float shootingAngleInDegrees, Vector3 velocity)
        {
            // Start at the origin, firing in a random (but roughly upward) direction.
            this.position = position;
            this.shootingAngleInDegrees = shootingAngleInDegrees;
            this.velocity = velocity;
            ApplyCreate();
        }

        private void ApplyCreate()
        {
            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new MissileParticleEmitter(projectileTrailParticles, trailParticlesPerSecond, position);
            createdTime = LastUpdatedGameTime;

            SoundHandler.Fire();
        }

        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(GameTime gameTime)
        {
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            // Simple projectile physics.
            VelocityPositionCalculator.CalcVelocityAndPosition(ref position, ref velocity, elapsedTime, shootingAngleInDegrees, thrust: 1);

            // Update the particle emitter, which will create our particle trail.
            trailEmitter.Update(gameTime, position);

            // If enough time has passed, explode! Note how we pass our velocity
            // in to the AddParticle method: this lets the explosion be influenced
            // by the speed and direction of the projectile which created it.
            if ((gameTime.TotalGameTime.TotalSeconds - createdTime) > projectileLifespan)
            {
                return false;
            }
                
            return true;
        }

        public void Explode()
        {
            explode = true;
            PublishEvent();
            ApplyExplode();
        }

        private void ApplyExplode()
        {
            removeAction.Invoke(this);

            levelBackground.PerformBulletHitInTexture(position);

            SoundHandler.Bullethit();

            const int numExplosionParticles = 30;
            const int numExplosionSmokeParticles = 50;

            for (int i = 0; i < numExplosionParticles; i++)
                explosionParticles.AddParticle(position, Vector3.Zero);

            for (int i = 0; i < numExplosionSmokeParticles; i++)
                explosionSmokeParticles.AddParticle(position, Vector3.Zero);
        }

        protected internal override void ProcessGameEvents()
        {
            if (explode)
                ApplyExplode();
            else
                ApplyCreate();
        }

        protected internal override void ObjectStored()
        {
        }
    }
}
