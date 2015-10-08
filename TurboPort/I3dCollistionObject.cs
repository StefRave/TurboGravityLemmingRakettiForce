using Microsoft.Xna.Framework;

namespace TurboPort
{
    public interface I3DCollistionObject
    {
        float CollisionRadius();
        void DrawCollistion(Matrix view, Matrix projection, Vector3 position);
    }
}