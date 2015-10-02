using System.IO;
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
            //return gfl.BitmapData[((int)point.X) + (levelWidth * ((int)point.Y))] != 0;

			//san 
			return false;
        }


        public void Render(GraphicsDevice device, BasicEffect be)
		{
//san
#if false
            be.World = Matrix.Identity;
            
            //device.RenderState.DepthBufferEnable = false;   // dx: device.RenderState.ZBufferEnable = false;
            be.LightingEnabled = false;                     //dx: device.RenderState.Lighting = false;
            device.RenderState.CullMode = CullMode.None;

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
            be.Begin();
            device.VertexDeclaration = declaration;
            //foreach(EffectPass pass in be.CurrentTechnique.Passes)
            {
                //pass.Begin();
                be.CurrentTechnique.Passes[0].Begin();
                device.DrawUserPrimitives(PrimitiveType.TriangleFan, vertexes, 0, 2);
                be.CurrentTechnique.Passes[0].End();
                //pass.End();
            }

            be.End();
#else
            SpriteBatch sb = new SpriteBatch(device);
            sb.Begin();
            sb.Draw(texture, new Vector2(0,0), Color.White);
            sb.End();
#endif

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

            VertexPositionTexture[] pt = new VertexPositionTexture[4];
            pt[0].Position = new Vector3(result.bounds.Left, result.bounds.Top, 0);
            pt[1].Position = new Vector3(result.bounds.Right, result.bounds.Top, 0);
            pt[2].Position = new Vector3(result.bounds.Right, result.bounds.Bottom, 0);
            pt[3].Position = new Vector3(result.bounds.Left, result.bounds.Bottom, 0);

            pt[0].TextureCoordinate = new Vector2(0, 1);
            pt[1].TextureCoordinate = new Vector2(1, 1);
            pt[2].TextureCoordinate = new Vector2(1, 0);
            pt[3].TextureCoordinate = new Vector2(0, 0);



            return result;
        }
    }

    public class LevelBackgroundTest : ILevelBackground
    {
        private Texture2D texture;
        private VertexPositionTexture[] vertexes;
        public Rectangle bounds;

        private LevelBackgroundTest()
        {
        }


        public Rectangle Bounds
        {
            get { return bounds; }
        }

        public void Render(GraphicsDevice device, BasicEffect be)
        {
            be.World = Matrix.Identity;

			be.LightingEnabled = false;                     //dx: device.RenderState.Lighting = false;
			be.Texture = texture;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertexes, 0, 2);

/** san
            device.RenderState.DepthBufferEnable = false;   // dx: device.RenderState.ZBufferEnable = false;
            be.LightingEnabled = false;                     //dx: device.RenderState.Lighting = false;
            device.RenderState.CullMode = CullMode.None;

            be.Texture = texture;
            
            device.DrawUserPrimitives(PrimitiveType.TriangleFan, vertexes, 0, 2);
*/
            //device.Transform.World = Matrix.Identity;

            //device.RenderState.ZBufferEnable = false;
            //device.RenderState.Lighting = false;
            //device.RenderState.CullMode = Cull.None;

            //device.SetTexture(0, texture);
            //device.TextureState[0].ColorOperation = TextureOperation.Modulate;
            //device.TextureState[0].ColorArgument1 = TextureArgument.Texture;
            //device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
            //device.TextureState[0].AlphaOperation = TextureOperation.Disable;
	
            //device.SetStreamSource(0, vertexes, 0);
            //device.VertexFormat = Direct3D.CustomVertex.PositionTextured.Format;
            //device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
        }

        public static LevelBackgroundTest CreateLevelBackground(GraphicsDevice device)
        {
            LevelBackgroundTest result = new LevelBackgroundTest();

            //result.texture = Texture2D.FromFile(device, Path.Combine(DirectorySettings.MediaDir, @"gfx\tx108.jpg"));

			string path = Path.Combine(DirectorySettings.MediaDir, @"gfx/tx108.jpg");
			FileStream fs = File.Create(path);

			result.texture = Texture2D.FromStream (device, fs);
            result.bounds = new Rectangle(-50, -50, 100, 100);


            VertexPositionTexture[] pt = new VertexPositionTexture[4];
            result.vertexes = pt;
            pt[0].Position = new Vector3(result.bounds.Left, result.bounds.Top, 0);
            pt[1].Position = new Vector3(result.bounds.Right, result.bounds.Top, 0);
            pt[2].Position = new Vector3(result.bounds.Right, result.bounds.Bottom, 0);
            pt[3].Position = new Vector3(result.bounds.Left, result.bounds.Bottom, 0);

            pt[0].TextureCoordinate = new Vector2(0, 0);
            pt[1].TextureCoordinate = new Vector2(result.bounds.Width, 0);
            pt[2].TextureCoordinate = new Vector2(result.bounds.Width, result.bounds.Height);
            pt[3].TextureCoordinate = new Vector2(0, result.bounds.Height);

            return result;
        }
    }
}
