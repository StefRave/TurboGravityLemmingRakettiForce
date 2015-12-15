using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TurboPort
{

	public class PlayerControl
    {
        public float    Thrust;
        public float    Rotation;
        public bool     Fire;
        public bool     FireSpecial;
    }

    public interface IInputControl
    {
        void HandleInput(PlayerControl playerControl);
    }

    public class GamePadController : IInputControl 
    {
        private readonly PlayerIndex playerIndex;

        public GamePadController(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        /// <param name="playerIndex">0 - 4</param>
        /// <returns></returns>
        static public GamePadController CreateIfConnected(int playerIndex)
        {
            PlayerIndex index;
            switch (playerIndex)
            {
                case 0: index = PlayerIndex.One; break;
                case 1: index = PlayerIndex.One; break;
                case 2: index = PlayerIndex.One; break;
                case 3: index = PlayerIndex.One; break;
                default:
                    return null;
            }

            GamePadState padState = GamePad.GetState(index);
            if (!padState.IsConnected) // Stef: returns true even after controller is disconnected
                return null;

            return new GamePadController(index);
        }



        public void HandleInput(PlayerControl playerControl)
        {
            GamePadState padState = GamePad.GetState(playerIndex);
            if(!padState.IsConnected)
                return;

            playerControl.Rotation = padState.ThumbSticks.Left.X;
            playerControl.Thrust = Math.Max(0, padState.ThumbSticks.Right.Y);

            if(padState.Buttons.LeftShoulder == ButtonState.Pressed)
                playerControl.Rotation = -1;
            if(padState.Buttons.RightShoulder == ButtonState.Pressed)
                playerControl.Rotation = 1;
            if(padState.Buttons.X == ButtonState.Pressed)
                playerControl.Thrust = 1;

            playerControl.Fire        = padState.Buttons.A == ButtonState.Pressed;
            playerControl.FireSpecial = padState.Buttons.B == ButtonState.Pressed;

        }
    }

    public class InputControlKeyboard : IInputControl 
    {
        public class KeyConfig
        {
            public Keys ThrustKey;
            public Keys RotateLeft;
            public Keys RotateRight;
            public Keys Fire;
            public Keys FireSpecial;

            public static KeyConfig Player1
            {
                get
                {
                    KeyConfig c = new KeyConfig();
					c.FireSpecial = Keys.LeftControl;
                    c.Fire = Keys.LeftShift;
                    c.RotateRight = Keys.D;
                    c.RotateLeft = Keys.A;
                    c.ThrustKey = Keys.W;
                    return c;
                }
            }

            public static KeyConfig Player2
            {
                get
                {
                    KeyConfig c = new KeyConfig();
                    c.FireSpecial = Keys.N;
                    c.Fire = Keys.M;
                    c.RotateRight = Keys.Right;
                    c.RotateLeft = Keys.Left;
                    c.ThrustKey = Keys.Up;
                    return c;
                }
            }
        }


        readonly KeyConfig keyConfig;

        public InputControlKeyboard(KeyConfig keyConfig)
        {
            this.keyConfig = keyConfig;
        }

        public void HandleInput(PlayerControl playerControl)
        {
            KeyboardState state = Keyboard.GetState();

            if(state.IsKeyDown(keyConfig.ThrustKey))
                playerControl.Thrust = 1;
            if(state.IsKeyDown(keyConfig.RotateLeft))
            playerControl.Rotation = -1;
            if(state.IsKeyDown(keyConfig.RotateRight))
                playerControl.Rotation = 1;

            if(state.IsKeyDown(keyConfig.Fire))
                playerControl.Fire = true;
            if(state.IsKeyDown(keyConfig.FireSpecial))
                playerControl.FireSpecial = true;
        }
    }

    public class InputHandler
    {
        private static List<IInputControl>[] playerInput;

        public static PlayerControl[] Player { get; private set; }

        static public bool HandleInput()
        {
            for(int j = 0; j < playerInput.Length; j++)
            {
                var playerControl = new PlayerControl();
                foreach (var inputControl in playerInput[j])
                    inputControl.HandleInput(playerControl);

                Player[j] = playerControl;
            }
            return true;
        }

        static public void Initialize()
        {
            int nrOfPlayers = Settings.Current.Players.Length;
            Player = new PlayerControl[nrOfPlayers];

            playerInput = new List<IInputControl>[nrOfPlayers];
            for (int i = 0; i < playerInput.Length; i++)
                playerInput[i] = new List<IInputControl>();

        
            int playerIndex = 0;
            for (int controllerIndex = 0; controllerIndex < 4; controllerIndex++)
            {
                if (playerIndex >= nrOfPlayers)
                    break;

                var gamePadController = GamePadController.CreateIfConnected(controllerIndex);
                if(gamePadController != null)
                    playerInput[playerIndex].Add(gamePadController);
            }

            // Default back to keyboard controls
            playerInput[0].Add(new InputControlKeyboard(InputControlKeyboard.KeyConfig.Player1));
            playerInput[1].Add(new InputControlKeyboard(InputControlKeyboard.KeyConfig.Player2));
        }
	}
}
