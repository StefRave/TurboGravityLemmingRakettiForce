using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TurboPort.ParticleSystems;

namespace TurboPort
{
    class MissleProjectileFactory : IMissleProjectileFactory
    {
        private readonly List<MissleProjectile> projectiles;
        private readonly ExplosionParticleSystem explosionParticles;
        private readonly ExplosionSmokeParticleSystem explosionSmokeParticles;
        private readonly ProjectileTrailParticleSystem projectileTrailParticles;

        public MissleProjectileFactory(List<MissleProjectile> projectiles, ExplosionParticleSystem explosionParticles, ExplosionSmokeParticleSystem explosionSmokeParticles, ProjectileTrailParticleSystem projectileTrailParticles)
        {
            this.projectiles = projectiles;
            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            this.projectileTrailParticles = projectileTrailParticles;
        }

        public void Fire(Vector3 position, Vector3 direction)
        {
            SoundHandler.Fire();

            projectiles.Add(new MissleProjectile(explosionParticles,
                               explosionSmokeParticles,
                               projectileTrailParticles,
                               position, direction));

        }
    }
}