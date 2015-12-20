using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace TurboPort.Input
{
    public class TouchControl : IInputControl
    {
        public static string Info = "";
        private Vector2 startPos;
        private int startPosId;

        public TouchControl()
        {
        }

        public static TouchControl Create()
        {
            var capabilities = TouchPanel.GetCapabilities();
            if(!capabilities.IsConnected)
                return null;

            TouchPanel.EnabledGestures =
                GestureType.DoubleTap |
                GestureType.Hold |
                GestureType.FreeDrag |
                GestureType.Tap |
                GestureType.Flick;

            return new TouchControl();
        }

        public void HandleInput(PlayerControl playerControl)
        {
            var touchCollection = TouchPanel.GetState();

            StringBuilder sb = new StringBuilder();
            sb.Append($"{touchCollection.Count}: ");
            Info = "";
            foreach (TouchLocation touch in touchCollection)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    if (touch.Position.X < (TouchPanel.DisplayWidth / 2f))
                    {
                        playerControl.Fire = true;
                    }
                    else
                    {
                        startPos = touch.Position;
                        startPosId = touch.Id;
                    }
                    continue;
                }

                if ((touch.State == TouchLocationState.Moved) && (touch.Id == startPosId))
                {
                    var relPos = (touch.Position - startPos)/150;

                    sb.Append($"{touch.State} {touch.Id} {touch.Position} {touch.Pressure}\n");

                    playerControl.Rotation = Math.Max(Math.Min(1f, relPos.X), -1f);
                    if (relPos.Y < 0)
                        playerControl.Thrust = Math.Min(-relPos.Y, 1f);
                    Info = $"{playerControl.Thrust} {playerControl.Rotation}";
                }
            }
        }
    }
}