#region Using Statements

using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace TurboPort
//namespace tglrf
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public static ILevelBackground levelBackground;
		private ObjectShip[] playerShips;
		private BulletBuffer bullerBuffer;
		private ShipBase[] shipBase;
		private GravitiForceLevel gfl;
		private RenderTarget2D renderTarget2D;
		private SpriteFont spriteFont;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
            
			Content.RootDirectory = FindContent();

#if !DEBUG
			graphicsManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			graphicsManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

			graphicsManager.IsFullScreen = true;
#else
			graphics.IsFullScreen = false;		
#endif
		}

	    private string FindContent()
	    {
	        string path = Path.GetFullPath(Environment.CurrentDirectory);
	        while (true)
	        {
	            string contentPath = Path.Combine(path, "Content");
	            if (Directory.Exists(contentPath))
	            {
	                string binPath = Path.Combine(contentPath, "bin");
                    if(Directory.Exists(binPath)) // in the dev environment the content is stored in Content/bin
                        return binPath;
	                return contentPath;
	            }
	            path = Path.GetDirectoryName(path);
                if(path == null)
                    throw new Exception("Content path not found");
	        }
	    }

	    /// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here
			base.Initialize ();
				
			InputHandler.Initialize();
			SoundHandler.Initialize(Content);

			base.Initialize();

			//renderTarget2D = new RenderTarget2D(GraphicsDevice, 20, 20, 1, SurfaceFormat.Bgr32);
			//renderTarget2D = new RenderTarget2D(GraphicsDevice, 20, 20, true, SurfaceFormat.Bgr32, DepthFormat.Depth24);
			//renderTarget2D = new RenderTarget2D(GraphicsDevice, 20, 20);
        }

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
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

			for(int i = 0; i < playerShips.Length; i++)
			{
				playerShips[i] = ObjectShip.CreateShip(GraphicsDevice, Content);

                playerShips[i].Position = shipBase[i].Position;
			}
			bullerBuffer = new BulletBuffer(GraphicsDevice, Content);
		}

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
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
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update (GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
			#if !__IOS__
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			    Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				Exit ();
			}
			#endif
			// TODO: Add your update logic here			


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
					//san
					//Console.WriteLine("[" + i + "] = " + playerShips[i].Position.ToString());
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



			base.Update (gameTime);
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
		protected override void Draw (GameTime gameTime)
		{
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

	
			//TODO: Add your drawing code here
            var defaultViewport = GraphicsDevice.Viewport;


			Viewport newView = defaultViewport;
			newView.Width = newView.Width / InputHandler.Player.Length;

			GlobalData gd = new GlobalData();
			SetupMatrices(gd, new Vector2(newView.Width, newView.Height));

            //DrawToRenderTarget(gd);


            //Clear the backbuffer to the cornflower blue 
            //GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target | ClearOptions.Stencil, Color.CornflowerBlue, 1.0f, 0);
            GraphicsDevice.Clear(Color.AntiqueWhite);
            try
			{
                GraphicsDevice.Viewport = newView;


                for (int player = 0; player < InputHandler.Player.Length; player++)
				{
					GraphicsDevice.Viewport = newView;

                    BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
					basicEffect.Projection = gd.Projection;
					basicEffect.View = CalculateView(gd, playerShips[player].Position);

                    //GraphicsDevice.RenderState.DepthBufferEnable = false;
                    levelBackground.Render(GraphicsDevice, basicEffect);
                    //san
                    //GraphicsDevice.RenderState.DepthBufferEnable = true;

                    SetUpLights(basicEffect);
					for(int i = 0; i < InputHandler.Player.Length; i++)
					{
                        playerShips[i].Render(GraphicsDevice, basicEffect, 0, 0);
                    }


                    DrawInfo("{0:00.00}x {1:00.00}y",
                        playerShips[player].Position.X,
					    playerShips[player].Position.Y);


                    for (int i = 0; i < InputHandler.Player.Length; i++)
                    {
                        bullerBuffer.Render(GraphicsDevice, basicEffect, playerShips[i], levelBackground);
                    }
                    //End the scene

                    newView.X += newView.Width + 0;
				}

			//san
			Texture2D texture = renderTarget2D;// .GetTexture();
			{
				//SpriteBatch sb = new SpriteBatch(GraphicsDevice);
				//sb.Begin();
				//sb.Draw(texture, new Vector2(220, 220), null, Color.Azure, 0, Vector2.Zero,
				//	gd.ViewportResolution.X/(gd.PixelsToCenter.X*2), SpriteEffects.None, 0);
				//sb.End();
			}


			base.Draw (gameTime);

            }
            finally
            {
                GraphicsDevice.Viewport = defaultViewport;
            }
        }

        private void DrawInfo(string format, params object[] args)
	    {
            var text = string.Format(format, args);
	        int lines = text.Count(c => c == '\n') + 1;

            SpriteBatch sb = new SpriteBatch(GraphicsDevice);
            sb.Begin();
	        sb.DrawString(spriteFont,
	            text,
	            new Vector2(50, GraphicsDevice.Viewport.Height - (30 * lines)), Color.Blue,
	            -0.025f, Vector2.Zero, 1, SpriteEffects.None, 0);
	        sb.End();
	    }


	    private void DrawToRenderTarget(GlobalData gd)
		{
			//GraphicsDevice.Viewport.Width GraphicsDevice.Viewport.Height

			//san
			BasicEffect basicEffect = new BasicEffect(GraphicsDevice);
			basicEffect.Projection = gd.Projection;
			basicEffect.View = Matrix.CreateLookAt(
				new Vector3(0f, 0f, gd.CameraDistance),
				Vector3.Zero,
				new Vector3(0f, 1f, 0f));

			float t = renderTarget2D.Width/2; // * gd.PixelsToCenter.X / gd.ViewportResolution.X;
			basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
				-t, t, -t, t,
				1.0f, 10000.0f);



			//san
			//GraphicsDevice.SetRenderTarget(0, renderTarget2D);
			GraphicsDevice.SetRenderTarget(renderTarget2D);
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
			//san
			//GraphicsDevice.SetRenderTarget(0, null);
			GraphicsDevice.SetRenderTarget(null);
		}
	
	}
}

