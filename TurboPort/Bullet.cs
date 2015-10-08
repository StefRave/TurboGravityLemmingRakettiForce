using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TurboPort
{
    public class BulletBuffer
    {
        private readonly List<VertexPositionColor>   bullets = new List<VertexPositionColor>();
        private readonly Texture2D             texture;


        public BulletBuffer(ContentManager content)
        {
            texture = content.Load<Texture2D>(@"gfx/particle");
        }

        public void AddBulletToRender(VertexPositionColor vertex)
        {
            bullets.Add(vertex);
        }

        public void Clear()
        {
            bullets.Clear();
        }

        public void Render(GraphicsDevice device, BasicEffect basicEffect)
        {
            basicEffect.LightingEnabled = false;
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;

            if (bullets.Count > 0)
            {
                SpriteBatch sb = new SpriteBatch(device);
                sb.Begin(0, BlendState.Additive, null, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect);

                foreach (var bullet in bullets)
                {
                    var scaleFactor = 0.02f;
                    var position = new Vector2(
                        (float) (bullet.Position.X - (texture.Width*scaleFactor/2)),
                        (float) (bullet.Position.Y - (texture.Height*scaleFactor/2)));

                    sb.Draw(texture, position, null, bullet.Color, 0,
                        Vector2.Zero, new Vector2(scaleFactor, scaleFactor), SpriteEffects.None, 0);
                }
                sb.End();
            }
        }
    }
}
