using Microsoft.Xna.Framework;

namespace TurboPort
{
    public interface I3DCollistionObject
    {
        float CollisionRadius();
        void DrawToCollistionTexture(Matrix view, Matrix projection, Vector3 position);
        Vector3 Position { get; }
    }
}