using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tglrf.xna
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        readonly GraphicsDeviceManager graphicsManager;
        SpriteBatch spriteBatch;

        public static LevelBackgroundGF levelBackground;
        private ObjectShip[] playerShips;
        private BulletBuffer bullerBuffer;
        private ShipBase[] shipBase;
        private GravitiForceLevel gfl;
        private RenderTarget2D renderTarget2D;
        private SpriteFont spriteFont;


        public Game1()
        {
            graphicsManager = new GraphicsDeviceManager(this);

            //graphicsManager.PreferredBackBufferWidth = 1280;
            //graphicsManager.PreferredBackBufferHeight = 1000;
#if !DEBUG
            graphicsManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphicsManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            
            graphicsManager.IsFullScreen = true;
#endif
#if false
            graphicsManager.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
#endif
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here


            InputHandler.Initialize();
            SoundHandler.Initialize();

            base.Initialize();

            renderTarget2D = new RenderTarget2D(GraphicsDevice, 20, 20, 1, SurfaceFormat.Bgr32);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here

            spriteFont = Content.Load<SpriteFont>("Courier");
            gfl = null;

            playerShips = new ObjectShip[Settings.Current.Players.Length];
            gfl = GravitiForceLevel.ReadGravitiForceLevelFile("GRBomber's Delight.GFB");

            levelBackground = LevelBackgroundGF.CreateLevelBackground(GraphicsDevice, gfl, Content);

            shipBase = new ShipBase[playerShips.Length];
            shipBase[0] =
                new ShipBase(new Vector3(gfl.playerBase[0].X - 104, 999 - gfl.playerBase[0].Y, 0f));
            shipBase[1] =
                new ShipBase(new Vector3(gfl.playerBase[1].X - 104, 999 - gfl.playerBase[1].Y, 0f));

            for(int i = 0; i < playerShips.Length; i++)
            {
                playerShips[i] = ObjectShip.CreateShip(GraphicsDevice, Content);

                playerShips[i].Position = shipBase[i].Position;
            }
            bullerBuffer = new BulletBuffer(GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            bool inputOk = InputHandler.HandleInput();

            //TestSettings.Value1 = 0; (double)numericUpDown1.Value;
            //TestSettings.Value2 = 0; (double)numericUpDown2.Value;
            //TestSettings.Value3 = 0; (double)numericUpDown3.Value;

            double timeElapsed = gameTime.ElapsedGameTime.TotalSeconds;
            if(inputOk)
            {
                for(int i = 0; i < InputHandler.Player.Length; i++)
                {
                    playerShips[i].HandleController(InputHandler.Player[i], timeElapsed);
                }
            }
            for(int i = 0; i < shipBase.Length; i++)
            {
                for(int j = 0; j < playerShips.Length; j++)
                {
                    shipBase[i].Interact(playerShips[j]);
                }
            }
            for(int i = 0; i < playerShips.Length - 1; i++)
            {
                for(int j = 1; j < playerShips.Length; j++)
                {
                    playerShips[i].Bots(playerShips[j]);
                }
            }


            base.Update(gameTime);
        }


        private static void SetupMatrices(GlobalData gd, Vector2 screenResolution)
        {
            gd.LevelDimensions = new Vector2(21 * 16, 63 * 16);
            gd.CameraDistance = 250f;
            gd.ViewportResolution = screenResolution;

            gd.Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)(Math.PI / 4),
                gd.ViewportResolution.X / gd.ViewportResolution.Y,
                1.0f, 10000.0f);

            gd.PixelsToCenter = new Vector2(
                gd.CameraDistance / gd.Projection.M11,
                gd.CameraDistance / gd.Projection.M22);
        }


        private static Matrix CalculateView(GlobalData gd, Vector3 target)
        {
            Vector3 cameraPosition = new Vector3(
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
                (float)(Math.Cos(Environment.TickCount/350.0)),
                -1.0f,
                (float)(Math.Sin(Environment.TickCount / 350.0)));
            be.DirectionalLight0.Enabled = true;
            be.DirectionalLight0.SpecularColor = Vector3.Zero;
            be.LightingEnabled = true;
        }




        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Viewport defaultViewport = GraphicsDevice.Viewport;


            Viewport newView = defaultViewport;
            newView.Width = newView.Width / InputHandler.Player.Length;

            GlobalData gd = new GlobalData();
            SetupMatrices(gd, new Vector2(newView.Width, newView.Height));


            DrawToRenderTarget(gd);


            //Clear the backbuffer to a blue color 
            GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Blue, 1.0f, 0);
            try
            {

                for(int player = 0; player < InputHandler.Player.Length; player++)
                {
                    GraphicsDevice.Viewport = newView;

                    BasicEffect basicEffect = new BasicEffect(GraphicsDevice, null);
                    basicEffect.Projection = gd.Projection;
                    basicEffect.View = CalculateView(gd, playerShips[player].Position);

                    GraphicsDevice.RenderState.DepthBufferEnable = false;
                    levelBackground.Render(GraphicsDevice, basicEffect);
                    GraphicsDevice.RenderState.DepthBufferEnable = true;

                    SetUpLights(basicEffect);
                    for(int i = 0; i < InputHandler.Player.Length; i++)
                    {
                        //playerShips[i].Render(device, vScrollBar1.Value, vScrollBar2.Value);
                        playerShips[i].Render(GraphicsDevice, basicEffect, 0, 0);
                    }

                    SpriteBatch sb = new SpriteBatch(GraphicsDevice);
                    sb.Begin();
                    sb.DrawString(spriteFont,
                        string.Format("{0:00.00}x {1:00.00}y", playerShips[player].Position.X, playerShips[player].Position.Y),
                        new Vector2(50, newView.Height - 30), Color.Blue,
                        -0.025f, Vector2.Zero, 1, SpriteEffects.None, 0);
                    sb.End();
                    //Direct3D.Font font = new Direct3D.Font(GraphicsDevice, new System.Drawing.Font("Arial", 10));
                    //font.DrawString(null,
                    //                string.Format("{0:00.00} {1:00.00}", playerShips[player].Position.X,
                    //                              playerShips[player].Position.Y),
                    //                new Rectangle(newView.X + 20, 20, 200, 30), DrawStringFormat.None, Color.Yellow);
                    //font.Dispose();



                    for(int i = 0; i < InputHandler.Player.Length; i++)
                    {
                        bullerBuffer.Render(GraphicsDevice, basicEffect, playerShips[i], levelBackground);
                    }
                    //End the scene

                    newView.X += newView.Width + 0;
                }
            }
            finally
            {
                GraphicsDevice.Viewport = defaultViewport;
            }

            Texture2D texture = renderTarget2D.GetTexture();
            {
                SpriteBatch sb = new SpriteBatch(GraphicsDevice);
                sb.Begin();
                sb.Draw(texture, new Vector2(220, 220), null, Color.Azure, 0, Vector2.Zero,
                        gd.ViewportResolution.X/(gd.PixelsToCenter.X*2), SpriteEffects.None, 0);
                sb.End();
            }
            base.Draw(gameTime);
        }

        private void DrawToRenderTarget(GlobalData gd)
        {
            //GraphicsDevice.Viewport.Width GraphicsDevice.Viewport.Height

            BasicEffect basicEffect = new BasicEffect(GraphicsDevice, null);
            basicEffect.Projection = gd.Projection;
            basicEffect.View = Matrix.CreateLookAt(
                new Vector3(0f, 0f, gd.CameraDistance),
                Vector3.Zero,
                new Vector3(0f, 1f, 0f));

            float t = renderTarget2D.Width/2; // * gd.PixelsToCenter.X / gd.ViewportResolution.X;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                -t, t, -t, t,
                1.0f, 10000.0f);

            



            GraphicsDevice.SetRenderTarget(0, renderTarget2D);
            Viewport renderTargetViewPort = new Viewport();
            renderTargetViewPort.Width = renderTarget2D.Width;
            renderTargetViewPort.Height = renderTarget2D.Height;
            GraphicsDevice.Viewport = renderTargetViewPort;
            GraphicsDevice.Clear(ClearOptions.Target, Color.Black,1.0f, 0);

            {
                ObjectShip obj = playerShips[0];
                Matrix renderMatrix =
                    Matrix.CreateTranslation(new Vector3(obj.centerOffset.X, obj.centerOffset.Y, obj.centerOffset.Z))*
                    Matrix.CreateRotationY(obj.Rotation.Y)*
                    Matrix.CreateRotationZ(obj.Rotation.Z)*
                    Matrix.CreateScale(10);
                    //Matrix.CreateTranslation(obj.Position);

                foreach(ModelMesh mesh in obj.Model.Meshes)
                {
                    foreach(BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.Projection = basicEffect.Projection;
                        effect.View = basicEffect.View;
                        effect.World = renderMatrix;
                    }
                    mesh.Draw();
                }

                obj.Render(GraphicsDevice, basicEffect, 0, 0);
            }
            GraphicsDevice.SetRenderTarget(0, null);
        }
    }
}
