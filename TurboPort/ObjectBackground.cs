using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Rectangle=Microsoft.Xna.Framework.Rectangle;


namespace TurboPort
{
    public interface ILevelBackground
    {
        Rectangle Bounds { get; }

        void Render(GraphicsDevice device, BasicEffect be);
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
#if true
            be.World = Matrix.Identity;
            
            //device.RenderState.DepthBufferEnable = false;   // dx: device.RenderState.ZBufferEnable = false;
            be.LightingEnabled = false;                     //dx: device.RenderState.Lighting = false;
            //device.RenderState.CullMode = CullMode.None;

            be.Texture = texture;
            be.TextureEnabled = true;
            // dx: device.SetTexture(0, texture);

            //device.SamplerState[0].MagFilter = TextureFilter.Linear;
            //device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            //device.TextureState[0].ColorArgument1 = TextureArgument.Texture;
            //device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            //device.TextureState[0].AlphaOperation = TextureOperation.Disable;

            //device.SetStreamSource(0, vertexes, 0);
            //device.VertexFormat = Direct3D.CustomVertex.PositionTextured.Format;
            //device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);

            //VertexBuffer vb = new VertexBuffer(device, typeof(VertexPositionTexture), 4, BufferUsage.None);
            //vb.SetData(vertexes);
            //device.Vertices[0].SetSource(vb, 0, VertexPositionTexture.SizeInBytes);

            be.CurrentTechnique.Passes.First().Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, pt, 0, 2);

#else
            SpriteBatch sb = new SpriteBatch(device);
            sb.Begin();
            sb.Draw(texture, new Vector2(0,0), Color.White);
            sb.End();
#endif
            //SpriteBatch sb = new SpriteBatch(device);
            //sb.Begin();
            //sb.Draw(texture, new Vector2(0, 0), Color.White);
            //sb.End();
        }

        public static LevelBackgroundGF CreateLevelBackground(GraphicsDevice device, GravitiForceLevel gfl, ContentManager content)
        {
            LevelBackgroundGF result = new LevelBackgroundGF();
            result.gfl = gfl;
  

            result.texture = Texture2D.FromStream(device, gfl.GetBitmapStream());

            //result.texture = content.Load<Texture2D>(@"test");
            //result.vertexes = VertexBuffer.CreateGeneric<Direct3D.CustomVertex.PositionTextured>(device,
            //    4, Usage.WriteOnly, Direct3D.CustomVertex.PositionTextured.Format, Pool.Managed, null);

            result.bounds = new Rectangle(0, 0, result.texture.Width, result.texture.Height);

            //result.vertexes.

            result.pt = new VertexPositionTexture[4];
            result.pt[0].Position = new Vector3(result.bounds.Left, result.bounds.Top, 0);
            result.pt[2].Position = new Vector3(result.bounds.Right, result.bounds.Top, 0);
            result.pt[1].Position = new Vector3(result.bounds.Right, result.bounds.Bottom, 0);
            result.pt[3].Position = new Vector3(result.bounds.Left, result.bounds.Bottom, 0);
            
            result.pt[0].TextureCoordinate = new Vector2(0, 1);
            result.pt[2].TextureCoordinate = new Vector2(1, 1);
            result.pt[1].TextureCoordinate = new Vector2(1, 0);
            result.pt[3].TextureCoordinate = new Vector2(0, 0);



            return result;
        }
    }
}

