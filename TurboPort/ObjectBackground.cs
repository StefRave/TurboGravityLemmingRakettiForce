using System.Diagnostics;
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
        void PerformBulletHitInTexture(Vector3 position);
    }

    public class LevelBackgroundGF : ILevelBackground
    { 
        private Texture2D texture;
        private int[] textureData;
        private long[] collisionData;
        private int collistionDataWidth;
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
            }
            return hit;
        }

        public void PerformBulletHitInTexture(Vector3 point)
        {
            int x = (int)point.X;
            int y = (int) point.Y;
            textureData[x + texture.Width*(texture.Height - y - 2)] = 0;
            textureData[x + texture.Width*(texture.Height - y - 1)] = 0;
            textureData[x + texture.Width*(texture.Height - y - 0)] = 0;
            textureData[x - 1 + texture.Width*(texture.Height - y - 1)] = 0;
            textureData[x + 1 + texture.Width*(texture.Height - y - 1)] = 0;
            texture.SetData(textureData);
        }

        public bool Interact(ObjectShip ship, CollisionPositionInTexture collisionPositionInTexture)
        {

            var isHit = CollisionDetection2D.DetectCollistionAndUpdateTexture(collisionPositionInTexture,
                collisionData, collistionDataWidth,
                textureData, texture.Width, texture.Height);

            ship.Hit = isHit;

            //Debug.Assert(isHit == ship.Hit);
            if (ship.Hit)
                texture.SetData(textureData);
            return ship.Hit;
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
            result.collistionDataWidth = (result.texture.Width + 0x3f) & ~0x3f;
            result.collisionData = new long[result.collistionDataWidth / 0x40 * result.texture.Height];

            int offsetCollision = 0;
            int offsetTexture = 0;
            for (int y = 0; y < result.texture.Height; y++)
            {
                int i;
                long mask64Bits = 0;
                for (i = 0; i < result.texture.Width; i++)
                {
                    mask64Bits *= 2;
                    if ((result.textureData[offsetTexture++] & 0xffffff) != 0)
                        mask64Bits++;

                    if ((i & 0x3f) == 0x3f)
                        result.collisionData[offsetCollision++] = mask64Bits;
                }
                if(i != result.collistionDataWidth)
                {
                    mask64Bits <<= result.collistionDataWidth - i;
                    result.collisionData[offsetCollision++] = mask64Bits;
                }
            }

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

