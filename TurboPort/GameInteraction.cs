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
        private long[] collisionTexture;

        public GameInteraction(GameWorld gameWorld)
        {
            this.gameWorld = gameWorld;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            const int width = 64; // must be multiple of 64
            const int height = 64;
            collisionRenderTarget = new RenderTarget2D(graphicsDevice, width, height);
            collisionRenderTargetBytes = new int[width * height];
            collisionTexture = new long[width / 64 * height + 1]; // +1 for collision detection algorithm
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
                    if (ship.IsOwner)
                        ship.HitWithBackground();
                }

                foreach (ShipBase shipBase in gameWorld.PlayerShipBases)
                {
                    if(ship.IsOwner)
                        shipBase.Interact(ship);
                }
            }

            for (var i = 0; i < gameWorld.PlayerShips.Count - 1; i++)
            {
                for (var j = 1; j < gameWorld.PlayerShips.Count; j++)
                {
                    if (gameWorld.PlayerShips[i].IsOwner || gameWorld.PlayerShips[j].IsOwner)
                        gameWorld.PlayerShips[i].Bots(gameWorld.PlayerShips[j]);
                }
            }
        }

        public unsafe void DrawToColisionDetectionTexture()
        {
            if (gameWorld.PlayerShips.Count == 0)
                return;

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
                        CollisionData = collisionTexture,
                        Size = new Point(collisionRenderTarget.Width, collisionRenderTarget.Height),
                        Position = collistionObject.Position,
                    });

                position.X += radiusInt;
                collistionObject.DrawToCollistionTexture(view, projection, new Vector3(position.X, position.Y, 0));
                position.X += radiusInt;
            }
            collisionRenderTarget.GetData(collisionRenderTargetBytes);
            graphicsDevice.SetRenderTarget(null);
            fixed (long* collisionptrFix = collisionTexture)
            fixed (int* textureptrFix = collisionRenderTargetBytes)
            {
                long* collisionptr = collisionptrFix;
                int* textureptr = textureptrFix;

                int width = collisionRenderTarget.Width;
                long collistion = 0;
                for (int y = 0; y < collisionRenderTarget.Height; y++)
                {
                    for (int x = 0; x < width; x+=64)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            collistion <<= 1;
                            if ((*textureptr++ & 0xffffff) != 0)
                                collistion++;
                        }
                        *collisionptr++ = collistion;
                    }
                }
            }
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
        public long[] CollisionData { get; set; }
        public Point Size { get; set; }
        public Vector3 Position { get; set; }
    }
}