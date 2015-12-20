using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TurboPort.Input
{
    public class GamePadControl : IInputControl 
    {
        private readonly PlayerIndex playerIndex;

        public GamePadControl(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        /// <param name="playerIndex">0 - 4</param>
        /// <returns></returns>
        static public GamePadControl CreateIfConnected(int playerIndex)
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

            return new GamePadControl(index);
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
}