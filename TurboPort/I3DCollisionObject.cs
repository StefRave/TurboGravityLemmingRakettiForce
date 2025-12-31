using Microsoft.Xna.Framework;

namespace TurboPort
{
    public interface I3DCollisionObject
    {
        float CollisionRadius();
        void DrawToCollisionTexture(Matrix view, Matrix projection, Vector3 position);
        Vector3 Position { get; }
    }
}