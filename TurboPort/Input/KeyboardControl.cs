using Microsoft.Xna.Framework.Input;

namespace TurboPort.Input
{
    public class KeyboardControl : IInputControl 
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

        public KeyboardControl(KeyConfig keyConfig)
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
}