using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TurboPort
{
    public class GameInteraction
    {
        private readonly GameWorld gameWorld;
        private GraphicsDevice graphicsDevice;
        private RenderTarget2D collisionRenderTarget;
        private int[] collisionRenderTargetBytes;
        private readonly Dictionary<object, CollisionPositionInTexture> dictje = new Dictionary<object, CollisionPositionInTexture>();

        public GameInteraction(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            const int width = 40;
            const int height = 40;
            collisionRenderTarget = new RenderTarget2D(graphicsDevice, width, height);
            collisionRenderTargetBytes = new int[width * height * 4];
        }

        public void DoInteraction()
        {
            DrawToColisionDetectionTexture();

            gameWorld.ProjectileFactory.Interact(gameWorld.LevelBackground);

            foreach (ObjectShip ship in gameWorld.PlayerShips)
            {
                var collisionPositionInTexture = dictje[ship];
                gameWorld.ProjectileFactory.Interact(ship, collisionPositionInTexture);
                if (gameWorld.LevelBackground.Interact(ship, collisionPositionInTexture))
                {
                    ship.Velocity = -ship.Velocity * 0.3f;
                }

                foreach (ShipBase shipBase in gameWorld.PlayerShipBases)
                    shipBase.Interact(ship);
            }

            for (var i = 0; i < gameWorld.PlayerShips.Count - 1; i++)
            {
                for (var j = 1; j < gameWorld.PlayerShips.Count; j++)
                {
                    gameWorld.PlayerShips[i].Bots(gameWorld.PlayerShips[j]);
                }
            }

        }

        public void DrawToColisionDetectionTexture()
        {
            graphicsDevice.SetRenderTarget(collisionRenderTarget);
            graphicsDevice.Viewport = new Viewport
                                            {
                                                Width = collisionRenderTarget.Width,
                                                Height = collisionRenderTarget.Height
                                            };

            var view = Matrix.CreateLookAt(
                new Vector3(0f, 0f, 100),
                Vector3.Zero,
                new Vector3(0f, 1f, 0f));

            var t = collisionRenderTarget.Width/2f;
            var projection = Matrix.CreateOrthographicOffCenter(
                -t, t, -t, t,
                1.0f, 10000.0f);

            var maxRadius = gameWorld.PlayerShips.Max(obj => obj.CollisionRadius());

            var position = new Point(
                -(collisionRenderTarget.Width/2),
                (collisionRenderTarget.Height/2));
            int radiusInt = (int)Math.Ceiling(maxRadius) + 1;
            position.Y -= radiusInt;

            dictje.Clear();

            foreach (I3DCollistionObject collistionObject in gameWorld.PlayerShips)
            {
                int x = position.X + collisionRenderTarget.Width / 2;
                var y = position.Y + radiusInt - collisionRenderTarget.Height / 2;
                dictje.Add(collistionObject,
                    new CollisionPositionInTexture
                    {
                        Rect = new Rectangle(x, y, radiusInt * 2, radiusInt * 2),
                        ByteData = collisionRenderTargetBytes,
                        Size = new Point(collisionRenderTarget.Width, collisionRenderTarget.Height)
                    });

                position.X += radiusInt;
                collistionObject.DrawToCollistionTexture(view, projection, new Vector3(position.X, position.Y, 0));
                position.X += radiusInt;
            }
            collisionRenderTarget.GetData(collisionRenderTargetBytes);
            graphicsDevice.SetRenderTarget(null);
        }


        public void DrawCollisionTexture(GlobalData gd)
        {
            Texture2D texture = collisionRenderTarget;
            {
                var sb = new SpriteBatch(graphicsDevice);
                sb.Begin(0, BlendState.Additive);
                sb.Draw(texture, new Vector2(20, 20), null, Color.Azure, 0, Vector2.Zero,
                    gd.ViewportResolution.X / (gd.PixelsToCenter.X * 2), SpriteEffects.None, 0);
                sb.End();
            }
        }
    }

    public class CollisionPositionInTexture
    {
        public Rectangle Rect { get; set; }
        public int[] ByteData { get; set; }
        public Point Size { get; set; }
    }
}