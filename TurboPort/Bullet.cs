using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TurboPort
{
    public struct PointVertex
    {
        public Vector3 v;
        public int color;
        //public static readonly VertexFormats Format =  VertexFormats.Position | VertexFormats.Diffuse;
    };


    public class BulletBuffer
    {
        private ArrayList         bullets = new ArrayList();
        private Texture2D         texture;
        private VertexPositionColor[] vertices;


        public BulletBuffer(GraphicsDevice device, ContentManager content)
        {
            vertices = new VertexPositionColor[1024];

            texture = content.Load<Texture2D>(@"gfx/particle");
            //,
            //    D3DX.Default, D3DX.Default, 0, (Usage)0, Format.Unknown, Pool.Managed,
            //    Filter.None, (Filter)0, 0);
        }

        public void Render(GraphicsDevice device, BasicEffect basicEffect, ObjectShip ship, ILevelBackground levelBackground)
        {
            basicEffect.World = Matrix.Identity;

            basicEffect.LightingEnabled = false;
            //device.RenderState.DepthBufferEnable = false;
            //device.RenderState.CullMode = CullMode.None;

            //device.RenderState.PointSpriteEnable = true;
            ////device.RenderState.PointScaleEnable = true ;
            //device.RenderState.PointSize = 10.0f;
            //device.RenderState.PointSizeMin = 0.00f;
            //device.RenderState.PointSizeMax = 100.00f;
            ////device.RenderState.PointScaleA = 0.00f;
            ////device.RenderState.PointScaleB = 0.00f;
            ////device.RenderState.PointScaleC = 1.00f;

            //device.RenderState.AlphaBlendEnable = true;
            //device.RenderState.SourceBlend = Blend.One;
            //device.RenderState.DestinationBlend = Blend.One;

            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;


            int count = 0;

            bool hackColliding = false;
            for(int i = 0; i < 6; i++)
            {
                Vector3 v = ship.colisionPoints[i];
				//v = Vector3.TransformCoordinate(v, ship.renderMatrix);
                v = Vector3.Transform(v, ship.renderMatrix);
                

////                Vector3 v = ship.Position;
////                v.Y = v.Y + (float)(ship.boundingBoxMax.Y * ship.scale * Math.Cos(ship.Rotation.Z));
////                v.X = v.X - (float)(ship.boundingBoxMax.Y * ship.scale * Math.Sin(ship.Rotation.Z));
                bool collide = levelBackground.CheckCollision(v);

                VertexPositionColor pv;
                pv.Position = v;
                pv.Color = collide ? Color.Yellow : Color.White;

                if(collide)
                {
                    vertices[count] = pv;
                    count++;
                }

                hackColliding |= collide;
            }
            if(!ship.hackColliding && hackColliding)
            {
                ship.Position = ship.OldPosition;
                ship.Speed = -ship.Speed * 0.3f;
                SoundHandler.Checkpoint();
            }
            ship.hackColliding = hackColliding;

            //device.VertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);
        
            //// Unlock the vertex buffer
            //vertexBuffer.Unlock();
            //// Render any remaining particles
            if (count > 0)
            {
                foreach(EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    SpriteBatch sb = new SpriteBatch(device);
                    sb.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect);
                    for (int i = 0; i < count; i++)
                    {
                        sb.Draw(texture, new Vector2(vertices[i].Position.X, vertices[i].Position.Y), Color.White);
                    }
                    sb.End();
                }
            }




            //// Reset render states
            //device.RenderState.PointSpriteEnable = false;
            //device.RenderState.PointScaleEnable = false;

            //device.RenderState.PointSpriteEnable = true;
            //device.RenderState.PointScaleEnable = true ;
            //device.RenderState.AlphaBlendEnable = false;

            //device.RenderState.PointSpriteEnable = false;
            //device.RenderState.DepthBufferWriteEnable = true;
            //device.RenderState.SourceBlend = Blend.SourceAlpha;
            //device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

        }

    }

    /// <summary>
	/// Summary description for Bullet.
	/// </summary>
	public class Bullet
	{
		public Bullet()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
