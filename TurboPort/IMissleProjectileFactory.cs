using Microsoft.Xna.Framework;

namespace TurboPort
{
    public interface IMissleProjectileFactory
    {
        void Fire(Vector3 position, float angleInDegrees, Vector3 velocity);
    }
}