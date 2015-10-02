using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle=Microsoft.Xna.Framework.Rectangle;


namespace TurboPort
{
    public interface ILevelBackground
    {
        Rectangle Bounds { get; }

        void Render(GraphicsDevice device, BasicEffect be);
        bool CheckCollision(Vector3 vector3);
    }

    public class LevelBackgroundGF : ILevelBackground
    { 
        private Texture2D texture;
        public Rectangle bounds;
        private GravitiForceLevel gfl;
        private VertexDeclaration declaration;
        private VertexPositionTexture[] pt;

        private LevelBackgroundGF()
        {
        }


        public Rectangle Bounds
        {
            get { return bounds; }
        }

        public bool CheckCollision(Vector3 point)
        {
            if((point.X < bounds.Left) ||
                (point.X >= bounds.Right) ||
                (point.Y < bounds.Top) || 
                (point.Y >= bounds.Bottom))
            {
                return true;
            }
            return gfl.BitmapData[((int)point.X) + (texture.Width * ((int)point.Y))] != 0;
        }

        public void Render(GraphicsDevice device, BasicEffect be)
        {
            be.World = Matrix.Identity;
            be.LightingEnabled = false;

            be.Texture = texture;
            be.TextureEnabled = true;

            be.CurrentTechnique.Passes.First().Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleList, pt, 0, 2);
        }

        public static ILevelBackground CreateLevelBackground(GraphicsDevice device, GravitiForceLevel gfl)
        {
            LevelBackgroundGF result = new LevelBackgroundGF();
            result.gfl = gfl;
  
            result.texture = Texture2D.FromStream(device, gfl.GetBitmapStream());

            result.bounds = new Rectangle(0, 0, result.texture.Width, result.texture.Height);

            result.pt = new VertexPositionTexture[6];
            result.pt[0].Position = new Vector3(result.bounds.Left, result.bounds.Top, 0);
            result.pt[1].Position = new Vector3(result.bounds.Left, result.bounds.Bottom, 0);
            result.pt[2].Position = new Vector3(result.bounds.Right, result.bounds.Bottom, 0);
            result.pt[3].Position = new Vector3(result.bounds.Left, result.bounds.Top, 0);
            result.pt[4].Position = new Vector3(result.bounds.Right, result.bounds.Bottom, 0);
            result.pt[5].Position = new Vector3(result.bounds.Right, result.bounds.Top, 0);

            result.pt[0].TextureCoordinate = new Vector2(0, 1);
            result.pt[1].TextureCoordinate = new Vector2(0, 0);
            result.pt[2].TextureCoordinate = new Vector2(1, 0);
            result.pt[3].TextureCoordinate = new Vector2(0, 1);
            result.pt[4].TextureCoordinate = new Vector2(1, 0);
            result.pt[5].TextureCoordinate = new Vector2(1, 1);

            return result;
        }
    }
}

