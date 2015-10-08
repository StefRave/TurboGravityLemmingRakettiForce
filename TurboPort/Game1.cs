#region Using Statements

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TurboPort.ParticleSystems;

#endregion

namespace TurboPort
//namespace tglrf
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static ILevelBackground levelBackground;


        private static readonly Random random = new Random();
        private readonly ExplosionParticleSystem explosionParticles;
        private readonly ExplosionSmokeParticleSystem explosionSmokeParticles;
        private readonly FireParticleSystem fireParticles;
        private readonly ProjectileTrailParticleSystem projectileTrailParticles;
        private readonly SmokePlumeParticleSystem smokePlumeParticles;
        private BulletBuffer bulletBuffer;
        private RenderTarget2D collisionRenderTarget;
        private GravitiForceLevel gfl;
        private readonly GraphicsDeviceManager graphics;
        private ObjectShip[] playerShips;
        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        private readonly List<MissleProjectile> projectiles = new List<MissleProjectile>();
        private ShipBase[] shipBase;
        private SpriteFont spriteFont;

        private TimeSpan timeToNextProjectile = TimeSpan.Zero;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = FindContent();

#if !DEBUG
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            graphics.IsFullScreen = true;
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
#else
			graphics.IsFullScreen = false;
#endif

            var contentLoader = Content.FromPath("particle3d");
            explosionParticles = new ExplosionParticleSystem(this, contentLoader);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(this, contentLoader);
            projectileTrailParticles = new ProjectileTrailParticleSystem(this, contentLoader);
            smokePlumeParticles = new SmokePlumeParticleSystem(this, contentLoader);
            fireParticles = new FireParticleSystem(this, contentLoader);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            // Register the particle system components.
            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);
            Components.Add(projectileTrailParticles);
            Components.Add(smokePlumeParticles);
            Components.Add(fireParticles);
        }

        private string FindContent()
        {
            var path = Path.GetFullPath(Environment.CurrentDirectory);
            while (true)
            {
                var contentPath = Path.Combine(path, "Content");
                if (Directory.Exists(contentPath))
                {
                    var binPath = Path.Combine(contentPath, "bin");
                    if (Directory.Exists(binPath)) // in the dev environment the content is stored in Content/bin
                        return binPath;
                    return contentPath;
                }
                path = Path.GetDirectoryName(path);
                if (path == null)
                    throw new Exception("Content path not found");
            }
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            InputHandler.Initialize();
            SoundHandler.Initialize(Content);

            base.Initialize();

            //renderTarget2D = new RenderTarget2D(GraphicsDevice, 20, 20, 1, SurfaceFormat.Bgr32);
            //renderTarget2D = new RenderTarget2D(GraphicsDevice, 20, 20, true, SurfaceFormat.Bgr32, DepthFormat.Depth24);
            collisionRenderTarget = new RenderTarget2D(GraphicsDevice, 40, 40, false, SurfaceFormat.Alpha8,
                DepthFormat.None);
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            //         spriteBatch = new SpriteBatch (GraphicsDevice);

            ////TODO: use this.Content to load your game content here 

            spriteFont = Content.Load<SpriteFont>("DejaVuSans");
            //spriteFont = Content.Load<SpriteFont>("Contnt/bin/Windows/gfx/DejaVuSans");
            gfl = null;

            playerShips = new ObjectShip[Settings.Current.Players.Length];
            gfl = GravitiForceLevel.ReadGravitiForceLevelFile("GRBomber's Delight.GFB");

            levelBackground = LevelBackgroundGF.CreateLevelBackground(GraphicsDevice, gfl);

            shipBase = new ShipBase[playerShips.Length];
            shipBase[0] =
                new ShipBase(new Vector3(gfl.playerBase[0].X - 104, 999 - gfl.playerBase[0].Y, 0f));
            shipBase[1] =
                new ShipBase(new Vector3(gfl.playerBase[1].X - 104, 999 - gfl.playerBase[1].Y, 0f));


            var missleProjectileFactory = new MissleProjectileFactory(projectiles, explosionParticles,
                explosionSmokeParticles, projectileTrailParticles);

            bulletBuffer = new BulletBuffer(Content);
            for (var i = 0; i < playerShips.Length; i++)
            {
                playerShips[i] = new ObjectShip(missleProjectileFactory, bulletBuffer, levelBackground);
                playerShips[i].Initialize(Content);

                playerShips[i].Position = shipBase[i].Position;
            }
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
#endif
            bulletBuffer.Clear();

            var inputOk = InputHandler.HandleInput();


            //TestSettings.Value1 = 0; (double)numericUpDown1.Value;
            //TestSettings.Value2 = 0; (double)numericUpDown2.Value;
            //TestSettings.Value3 = 0; (double)numericUpDown3.Value;

            var timeElapsed = gameTime.ElapsedGameTime.TotalSeconds;
            if (inputOk)
            {
                for (var i = 0; i < InputHandler.Player.Length; i++)
                {
                    playerShips[i].HandleController(InputHandler.Player[i], timeElapsed);
                    //san
                    //Console.WriteLine("[" + i + "] = " + playerShips[i].Position.ToString());
                }
            }
            foreach (var playerShip in playerShips)
            {
                playerShip.Update(gameTime);
            }

            for (var i = 0; i < shipBase.Length; i++)
            {
                for (var j = 0; j < playerShips.Length; j++)
                {
                    shipBase[i].Interact(playerShips[j]);
                }
            }
            for (var i = 0; i < playerShips.Length - 1; i++)
            {
                for (var j = 1; j < playerShips.Length; j++)
                {
                    playerShips[i].Bots(playerShips[j]);
                }
            }


            //UpdateExplosions(gameTime);
            UpdateProjectiles(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Helper for updating the explosions effect.
        /// </summary>
        private void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                Vector3 velocity;
                const float sidewaysVelocityRange = 60;
                const float verticalVelocityRange = 40;
                velocity.X = (float) (random.NextDouble() - 0.5)*sidewaysVelocityRange;
                velocity.Y = (float) (random.NextDouble() + 0.5)*verticalVelocityRange;
                velocity.Z = (float) (random.NextDouble() - 0.5)*sidewaysVelocityRange;

                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectiles.Add(new MissleProjectile(explosionParticles,
                    explosionSmokeParticles,
                    projectileTrailParticles,
                    playerShips[0].GunPosition, playerShips[0].ShootingVelocity));

                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }

        /// <summary>
        ///     Helper for updating the list of active projectiles.
        /// </summary>
        private void UpdateProjectiles(GameTime gameTime)
        {
            var i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }


        private static Matrix CalculateView(GlobalData gd, Vector3 target)
        {
            var cameraPosition = new Vector3(
                Math.Min(Math.Max(target.X, gd.PixelsToCenter.X), gd.LevelDimensions.X - gd.PixelsToCenter.X),
                Math.Min(Math.Max(target.Y, gd.PixelsToCenter.Y), gd.LevelDimensions.Y - gd.PixelsToCenter.Y),
                gd.CameraDistance);

            return Matrix.CreateLookAt(
                cameraPosition,
                new Vector3(cameraPosition.X, cameraPosition.Y, 0),
                new Vector3(0f, 1f, 0f));
        }

        private static void SetUpLights(BasicEffect be)
        {
            be.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
            be.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);

            be.DirectionalLight0.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
            be.DirectionalLight0.Direction = new Vector3(
                (float) (Math.Cos(Environment.TickCount/350.0)),
                -1.0f,
                (float) (Math.Sin(Environment.TickCount/350.0)));
            be.DirectionalLight0.Enabled = true;
            be.DirectionalLight0.SpecularColor = Vector3.Zero;
            be.LightingEnabled = true;
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Clear the backbuffer to the cornflower blue 
            //GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target | ClearOptions.Stencil, Color.CornflowerBlue, 1.0f, 0);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var defaultViewport = GraphicsDevice.Viewport;

            GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            var newView = defaultViewport;
            newView.Width = newView.Width/InputHandler.Player.Length;

            var gd = GlobalData.SetupMatrices(newView.Width, newView.Height);

            try
            {
                DrawToColisionDetectionTexture(gd);


                GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.Viewport = newView;


                for (var player = 0; player < InputHandler.Player.Length; player++)
                {
                    GraphicsDevice.Viewport = newView;


                    var projection = gd.Projection;
                    var view = CalculateView(gd, playerShips[player].Position);

                    var basicEffect = new BasicEffect(GraphicsDevice)
                                      {
                                          Projection = projection,
                                          View = view
                                      };

                    //GraphicsDevice.RenderState.DepthBufferEnable = false;
                    levelBackground.Render(GraphicsDevice, basicEffect);
                    //san
                    //GraphicsDevice.RenderState.DepthBufferEnable = true;

                    SetUpLights(basicEffect);
                    for (var i = 0; i < InputHandler.Player.Length; i++)
                    {
                        playerShips[i].Render(basicEffect);
                    }


                    DrawInfo("{0:00.00}x {1:00.00}y\nSpeed {2}",
                        playerShips[player].Position.X,
                        playerShips[player].Position.Y,
                        playerShips[player].Speed.Length());



                    bulletBuffer.Render(GraphicsDevice, (BasicEffect) basicEffect.Clone());
                    //End the scene


                    explosionParticles.SetCamera(view, projection);
                    explosionSmokeParticles.SetCamera(view, projection);
                    projectileTrailParticles.SetCamera(view, projection);
                    smokePlumeParticles.SetCamera(view, projection);
                    fireParticles.SetCamera(view, projection);

                    base.Draw(gameTime);
                    GraphicsDevice.BlendState = BlendState.NonPremultiplied;

                    newView.X += newView.Width + 0;
                }
            }
            finally
            {
                GraphicsDevice.Viewport = defaultViewport;
            }
            Texture2D texture = collisionRenderTarget;
            {
                var sb = new SpriteBatch(GraphicsDevice);
                sb.Begin();
                sb.Draw(texture, new Vector2(220, 220), null, Color.Azure, 0, Vector2.Zero,
                    gd.ViewportResolution.X/(gd.PixelsToCenter.X*2), SpriteEffects.None, 0);
                sb.End();
            }
        }

        private void DrawToColisionDetectionTexture(GlobalData gd)
        {
            GraphicsDevice.SetRenderTarget(collisionRenderTarget);
            GraphicsDevice.Viewport = new Viewport
                                      {
                                          Width = collisionRenderTarget.Width,
                                          Height = collisionRenderTarget.Height
                                      };

            var view = Matrix.CreateLookAt(
                new Vector3(0f, 0f, gd.CameraDistance),
                Vector3.Zero,
                new Vector3(0f, 1f, 0f));

            var t = collisionRenderTarget.Width/2f;
            var projection = Matrix.CreateOrthographicOffCenter(
                -t, t, -t, t,
                1.0f, 10000.0f);

            var maxRadius = playerShips.Max(obj => obj.CollisionRadius());

            var position = new Vector3(
                -(collisionRenderTarget.Width/2),
                -(collisionRenderTarget.Height/2),
                0);
            position.Y += maxRadius;

            foreach (I3DCollistionObject collistionObject in playerShips)
            {
                position.X += maxRadius;
                collistionObject.DrawCollistion(view, projection, position);
                position.X += maxRadius;
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawInfo(string format, params object[] args)
        {
            var text = string.Format(format, args);
            var lines = text.Count(c => c == '\n') + 1;

            var sb = new SpriteBatch(GraphicsDevice);
            sb.Begin();
            sb.DrawString(spriteFont,
                text,
                new Vector2(50, GraphicsDevice.Viewport.Height - (30*lines)), Color.Blue,
                -0.025f, Vector2.Zero, 1, SpriteEffects.None, 0);
            sb.End();
        }
    }
}