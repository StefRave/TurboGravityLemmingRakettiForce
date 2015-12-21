using Microsoft.Xna.Framework;

namespace TurboPort
{
    public interface IMissileProjectileFactory
    {
        void Fire(Vector3 position, float angleInDegrees, Vector3 velocity);
    }
}