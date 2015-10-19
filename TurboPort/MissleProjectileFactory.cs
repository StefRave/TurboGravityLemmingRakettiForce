﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TurboPort.ParticleSystems;

namespace TurboPort
{
    public class MissleProjectileFactory : GameComponent, IMissleProjectileFactory
    {
        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        private readonly List<MissleProjectile> projectiles = new List<MissleProjectile>();

        private readonly ExplosionParticleSystem explosionParticles;
        private readonly ExplosionSmokeParticleSystem explosionSmokeParticles;
        private readonly ProjectileTrailParticleSystem projectileTrailParticles;
        private readonly FireParticleSystem fireParticles;
        private readonly SmokePlumeParticleSystem smokePlumeParticles;

        public MissleProjectileFactory(Game game) : base(game)
        {
            var contentLoader = game.Content.FromPath("particle3d");
            explosionParticles = new ExplosionParticleSystem(game, contentLoader);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(game, contentLoader);
            projectileTrailParticles = new ProjectileTrailParticleSystem(game, contentLoader);
            smokePlumeParticles = new SmokePlumeParticleSystem(game, contentLoader);
            fireParticles = new FireParticleSystem(game, contentLoader);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            // Register the particle system components.
            Game.Components.Add(explosionParticles);
            Game.Components.Add(explosionSmokeParticles);
            Game.Components.Add(projectileTrailParticles);
            Game.Components.Add(smokePlumeParticles);
            Game.Components.Add(fireParticles);
        }

        public void Fire(Vector3 position, float angleInDegrees, Vector3 velocity)
        {
            SoundHandler.Fire();

            projectiles.Add(new MissleProjectile(explosionParticles,
                               explosionSmokeParticles,
                               projectileTrailParticles,
                               position, angleInDegrees, velocity));
        }

        /// <summary>
        ///     Helper for updating the list of active projectiles.
        /// </summary>
        public void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                    continue;
                }
                i++;
            }
        }

        public void Interact(ILevelBackground background)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (background.CheckCollision(projectiles[i].Position))
                {
                    projectiles[i].Explode();
                    projectiles.RemoveAt(i);
                    SoundHandler.Bullethit();
                    continue;
                }
                i++;
            }
        }

        public void Interact(ObjectShip ship, CollisionPositionInTexture collisionPositionInTexture)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (ship.CheckCollision(projectiles[i].Position, collisionPositionInTexture))
                {
                    projectiles[i].Explode();
                    projectiles.RemoveAt(i);
                    continue;
                }
                i++;
            }

        }

        public void Draw(Matrix view, Matrix projection)
        {
            explosionParticles.SetCamera(view, projection);
            explosionSmokeParticles.SetCamera(view, projection);
            projectileTrailParticles.SetCamera(view, projection);
            smokePlumeParticles.SetCamera(view, projection);
            fireParticles.SetCamera(view, projection);
        }
    }
}