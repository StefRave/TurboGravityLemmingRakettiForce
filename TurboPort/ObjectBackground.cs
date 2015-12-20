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
        bool Interact(ObjectShip ship, CollisionPositionInTexture collisionPositionInTexture);
    }

    public class LevelBackgroundGF : ILevelBackground
    { 
        private Texture2D texture;
        private int[] textureData;
        public Rectangle bounds;
        private GravitiForceLevel gfl;
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
            bool hit = gfl.BitmapData[((int)point.X) + (texture.Width * ((int)point.Y))] != 0;
            if (hit)
            {
                gfl.BitmapData[((int) point.X) + (texture.Width*((int) point.Y - 1))] = 0;
                gfl.BitmapData[((int) point.X) + (texture.Width*((int) point.Y - 0))] = 0;
                gfl.BitmapData[((int) point.X) + (texture.Width*((int) point.Y + 1))] = 0;
                gfl.BitmapData[((int) point.X - 1) + (texture.Width*((int) point.Y))] = 0;
                gfl.BitmapData[((int) point.X + 1) + (texture.Width*((int) point.Y))] = 0;
                textureData[(int)point.X + texture.Width * (texture.Height - ((int)point.Y) - 2)] = unchecked((int)0x00000000);
                textureData[(int)point.X + texture.Width * (texture.Height - ((int)point.Y) - 1)] = unchecked((int)0x00000000);
                textureData[(int)point.X + texture.Width * (texture.Height - ((int)point.Y) - 0)] = unchecked((int)0x00000000);
                textureData[(int)point.X - 1 + texture.Width * (texture.Height - ((int)point.Y) - 1)] = unchecked((int)0x00000000);
                textureData[(int)point.X + 1 + texture.Width * (texture.Height - ((int)point.Y) - 1)] = unchecked((int)0x00000000);
                texture.SetData(textureData);
            }


            return hit;
        }

        public bool Interact(ObjectShip ship, CollisionPositionInTexture collisionPositionInTexture)
        {

            var point = new Point(
                (int)ship.Position.X + (collisionPositionInTexture.Size.X /2),
                (int)ship.Position.Y + (collisionPositionInTexture.Size.Y / 2));

            ship.hit = CollisionDetection2D.IntersectPixels(
                new Rectangle(collisionPositionInTexture.Rect.Location + point, collisionPositionInTexture.Rect.Size),
                collisionPositionInTexture.ByteData,
                collisionPositionInTexture.Size.X,
                bounds,
                gfl.BitmapData,
                bounds.Width);

            ship.hit = false;
            int xStart = (int)ship.Position.X - (collisionPositionInTexture.Rect.Width/2) + 1;
            int yStart = (int)ship.Position.Y - (collisionPositionInTexture.Rect.Height/2) + 1;
            int xEnd = xStart + collisionPositionInTexture.Rect.Width;
            int yEnd = yStart + collisionPositionInTexture.Rect.Height;
            int offsetShip = (collisionPositionInTexture.Rect.Height - 1) * collisionPositionInTexture.Size.Y + collisionPositionInTexture.Rect.X + 1;
            int offsetShipLineDelta = collisionPositionInTexture.Size.Y + collisionPositionInTexture.Rect.Width;
            var byteData = collisionPositionInTexture.ByteData;
            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    if ((byteData[offsetShip++] & 0xffffff) != 0)
                    {
                        if (gfl.BitmapData[x + texture.Width*y] != 0)
                        {
                            ship.hit = true;
                            gfl.BitmapData[x + texture.Width*y] = 0;
                            textureData[x + texture.Width*(texture.Height - y - 1)] = unchecked((int)0);
                        }
                    }
                }
                offsetShip -= offsetShipLineDelta;
            }
            if (ship.hit)
            texture.SetData(textureData);

            return ship.hit;
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
            result.textureData = new int[result.texture.Height*result.texture.Width];
            result.texture.GetData(result.textureData);


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

