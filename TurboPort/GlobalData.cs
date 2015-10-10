using System;
using Microsoft.Xna.Framework;

namespace TurboPort
{
    public class GlobalData
    {
        public Vector2 LevelDimensions;
        public float CameraDistance = 250f;

        public Matrix Projection;

        public Vector2 PixelsToCenter;
        public Vector2 ViewportResolution;

        public static GlobalData SetupMatrices(int width, int height)
        {
            GlobalData gd = new GlobalData();
            gd.LevelDimensions = new Vector2(21 * 16, 63 * 16);
            gd.ViewportResolution = new Vector2(width, height);

            gd.Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)(Math.PI / 4),
                gd.ViewportResolution.X / gd.ViewportResolution.Y,
                1.0f, 10000.0f);

            gd.PixelsToCenter = new Vector2(
                gd.CameraDistance / gd.Projection.M11,
                gd.CameraDistance / gd.Projection.M22);

            return gd;
        }
    }
}