﻿#region Using Statements

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TurboPort.Event;
using TurboPort.Input;

#endregion

namespace TurboPort
{
    public class Game1 : Game
    {
        private readonly MasterControl masterControl;
        private readonly GameWorld gameWorld;
        private readonly GameObjectStore gameStore;
        private BulletBuffer bulletBuffer;
        private readonly GraphicsDeviceManager graphics;
        private SpriteFont spriteFont;
        private readonly UdpBroadcastEvents eventBroadCaster;
        private readonly UdpEventReceiver eventReceiver;
        private readonly MemoryStream gameEvents = new MemoryStream(10000000);

        private readonly GameInteraction gameInteraction;
        private readonly GameReplay replay;
        private readonly GameEventStore gameEventStore;

        public Game1(GameMode gameMode)
        {
            gameEventStore = new GameEventStore();
            gameStore = new GameObjectStore(gameEventStore);
            replay = new GameReplay(gameStore);
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
#if DEBUG
            Window.AllowUserResizing = true;
            graphics.IsFullScreen = false;
#else
            if (!Window.GetType().Name.Contains("Android"))
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                graphics.IsFullScreen = true;
                //Window.Position = Point.Zero;
                Window.IsBorderless = true;
            }
#endif
            eventReceiver = new UdpEventReceiver();
            eventBroadCaster = new UdpBroadcastEvents();

            gameWorld = new GameWorld(this, gameStore);
            gameInteraction = new GameInteraction(gameWorld);
            masterControl = new MasterControl(gameStore, gameWorld) { GameMode = gameMode };
        }


        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            gameInteraction.Initialize(GraphicsDevice);
            InputHandler.Initialize();
            SoundHandler.Initialize(Content);

            base.Initialize();

            masterControl.Initialize();
            Task.Run(() => masterControl.Start(CancellationToken.None));

            if ((masterControl.GameMode == GameMode.UdpReceive) || (masterControl.GameMode == GameMode.Multiplayer))
            {
                Task.Run(() => eventReceiver.StartReceivingAsync());
            }
            if (masterControl.GameMode == GameMode.ReadFromFile)
            {
                replay.Load(new MemoryStream(File.ReadAllBytes("eventstream.turboport")));
                replay.StartPlay(-0.5);
            }
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteFont = Content.Load<SpriteFont>("DejaVuSans");

            var gfl = GravitiForceLevel.ReadGravitiForceLevelFile("GRBomber's Delight.GFB");
            //gfl = GravitiForceLevel.ReadGravitiForceLevelFile("GRUp'n'Down (Race).GFB");
            //gfl = GravitiForceLevel.ReadGravitiForceLevelFile("MEModern Art.GFB");

            gameWorld.LevelBackground = LevelBackgroundGF.CreateLevelBackground(GraphicsDevice, gfl);

            gameWorld.AddShipBase(
                new ShipBase(new Vector3(gfl.playerBase[0].X - 104, 999 - gfl.playerBase[0].Y, 0f)));
            gameWorld.AddShipBase(
                new ShipBase(new Vector3(gfl.playerBase[1].X - 104, 999 - gfl.playerBase[1].Y, 0f)));

            bulletBuffer = new BulletBuffer(Content);

            ObjectShip.Initialize(Content, gameStore, gameWorld);

            base.LoadContent();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            SoundHandler.SetGameTime(gameTime);
            gameEventStore.SetGameTime(gameTime);

            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
            var keyboardState = Keyboard.GetState();
#if !__IOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                Dispose(true);
                return;
            }
#endif
            base.Update(gameTime);
            gameWorld.ProjectileFactory.UpdateProjectiles(gameTime);

            foreach (var playerShip in gameWorld.PlayerShips)
            {
                playerShip.Update(gameTime);
            }

            if (masterControl.GameMode == GameMode.ReadFromFile)
            {
                if (replay.PlayStatus == GameReplay.Status.Playing)
                {
                    replay.ProcessEventsUntilTime(gameTime.TotalGameTime.TotalSeconds);
                }
            }
            else
            {
                var enumerable = eventReceiver.GetEvents();
                foreach (NetworkData eventData in enumerable)
                {
                    bool myOwnData = eventData.PlayerId == masterControl.PlayerId;
                    if (myOwnData)
                    {
                        bool broadCast = eventData.PlayerId == 0;
                        if(!broadCast)
                            continue;
                    }

                    replay.Load(new MemoryStream(eventData.Data.Array, writable: false, index: eventData.Data.Offset, count: eventData.Data.Count, publiclyVisible: true));
                    replay.ReplayAll(gameTime.TotalGameTime.TotalSeconds);
                }

                if (keyboardState.IsKeyDown(Keys.F7))
                {
                    using (var fs = new FileStream("eventstream.turboport", FileMode.Create))
                    {
                        gameEvents.CopyTo(fs);
                    }
                }
                if (masterControl.GameMode != GameMode.UdpReceive)
                {
                    InputHandler.HandleInput();

                    for (var i = 0; i < gameWorld.PlayerShips.Count; i++)
                    {
                        var ship = gameWorld.PlayerShips[i];
                        if(ship.IsOwner)
                            ship.ProcessControllerInput(InputHandler.Player[i]);
                    }
                }
            }

            gameInteraction.DoInteraction();

            gameEventStore.SerializeModifiedObjects(gameEvents);
            if (eventBroadCaster != null)
            {
                if (gameEvents.Length > 0)
                {
                    eventBroadCaster.BroadCast(new NetworkData {PlayerId =  masterControl.PlayerId, Data = new ArraySegment<byte>(gameEvents.GetBuffer(), 0, (int) gameEvents.Length)});
                    gameEvents.Position = 0;
                    gameEvents.SetLength(0);
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
            //be.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
            be.AmbientLightColor = new Vector3(1f, 1f, 1f);

            be.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f);
            var direction = new Vector3(
                (float)(Math.Cos(Environment.TickCount / 350.0)),
                (float)(Math.Sin(Environment.TickCount / 350.0)),
                -2.0f);
            direction.Normalize();
            be.DirectionalLight0.Direction = direction;
            //be.DirectionalLight0.Direction = new Vector3(0,-1f,0);
            be.DirectionalLight0.Enabled = true;
            be.DirectionalLight0.SpecularColor = new Vector3(0f,0f,0f);
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
                GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.Viewport = newView;


                for (var player = 0; player < gameWorld.PlayerShips.Count; player++)
                {
                    GraphicsDevice.Viewport = newView;

                    var projection = gd.Projection;
                    var view = CalculateView(gd, gameWorld.PlayerShips[player].Position);

                    var basicEffect = new BasicEffect(GraphicsDevice)
                                      {
                                          Projection = projection,
                                          View = view
                                      };

                    //GraphicsDevice.RenderState.DepthBufferEnable = false;
                    gameWorld.LevelBackground.Render(GraphicsDevice, basicEffect);
                    //san
                    //GraphicsDevice.RenderState.DepthBufferEnable = true;

                    SetUpLights(basicEffect);
                    for (var i = 0; i < gameWorld.PlayerShips.Count; i++)
                    {
                        gameWorld.PlayerShips[i].Render(GraphicsDevice, basicEffect);
                    }

                    DrawInfo("{0:00.00}x {1:00.00}y\nSpeed {2}",
                        gameWorld.PlayerShips[player].Position.X,
                        gameWorld.PlayerShips[player].Position.Y,
                        gameWorld.PlayerShips[player].Velocity.Length());
                    //DrawInfo(TouchControl.Info);

                    bulletBuffer.Render(GraphicsDevice, (BasicEffect) basicEffect.Clone());
                    //End the scene

                    gameWorld.ProjectileFactory.Draw(view, projection);

                    base.Draw(gameTime);
                    GraphicsDevice.BlendState = BlendState.NonPremultiplied;

                    newView.X += newView.Width + 0;
                }
            }
            finally
            {
                GraphicsDevice.Viewport = defaultViewport;
            }
            gameInteraction.DrawCollisionTexture(gd);
        }

        private void DrawInfo(string format, params object[] args)
        {
            var text = format;
            if(args != null && args.Length > 0)
                text = string.Format(format, args);

            var lines = text.Count(c => c == '\n') + 1;

            var sb = new SpriteBatch(GraphicsDevice);
            sb.Begin();
            sb.DrawString(spriteFont,
                text,
                new Vector2(50, GraphicsDevice.Viewport.Height - (30*lines)), Color.Blue,
                -0.025f, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
            sb.End();
        }
    }
}