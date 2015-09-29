using System;
using System.Collections.Generic;
using System.Reflection;
//using DxVBLibA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tglrf
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

	/*

#if JoyLib
    public class InputControlJoystick : IInputControl
    {
        private readonly int controlerIndex;
        static public readonly float DeadZone = 0.15f;

        public InputControlJoystick(int controlerIndex)
        {
            this.controlerIndex = controlerIndex;
        }

        private static float GetAnalogValue(int intValue)
        {
            float value = (intValue - 32768)/32768f;
            if(value > DeadZone)
                return (value - DeadZone) / (1 - DeadZone);
            else if(value < -DeadZone)
                return (value + DeadZone) / (1 - DeadZone);

            return 0;
        }

        public void HandleInput(PlayerControl playerControl)
        {
            JoystickState state = Joystick.GetState(controlerIndex);

            playerControl.Thrust = Math.Max(0, -GetAnalogValue(state.GetAxis(JoystickState.Axis.R)));
            playerControl.Rotation = GetAnalogValue(state.GetAxis(JoystickState.Axis.X));
            playerControl.Fire = state.GetButton(3);
            playerControl.FireSpecial = state.GetButton(4);
        }
    }
#else
    public class InputControlJoystick : IInputControl
    {
        private readonly DirectInputDeviceInstance8 inputDevice;
        private readonly DirectInputDevice8 device;

        static public readonly float DeadZone = 0.15f;

        public InputControlJoystick(DirectInput8 input, DirectInputDeviceInstance8 inputDevice)
        {
            this.inputDevice = inputDevice;
            //Console.Out.WriteLine("gameController.GetProductName() = {0}", gameController.GetProductName());
            //Console.Out.WriteLine("gameController.GetInstanceName() = {0}", gameController.GetInstanceName());
            device = input.CreateDevice(inputDevice.GetGuidInstance());
            device.SetCommonDataFormat(CONST_DICOMMONDATAFORMATS.DIFORMAT_JOYSTICK);
            device.SetCooperativeLevel(0, CONST_DISCLFLAGS.DISCL_BACKGROUND | CONST_DISCLFLAGS.DISCL_NONEXCLUSIVE);
            DIDEVCAPS caps = new DIDEVCAPS();
            device.GetCapabilities(ref caps);
            device.Acquire();
        }

        private static float GetAnalogValue(int intValue)
        {
            float value = (intValue - 32768) / 32768f;
            if(value > DeadZone)
                return (value - DeadZone) / (1 - DeadZone);
            else if(value < -DeadZone)
                return (value + DeadZone) / (1 - DeadZone);

            return 0;
        }

        public void HandleInput(PlayerControl playerControl)
        {
            DIJOYSTATE joyState = new DIJOYSTATE();
            device.GetDeviceStateJoystick(ref joyState);

            playerControl.Thrust = Math.Max(0, -GetAnalogValue(joyState.rz));
            playerControl.Rotation = GetAnalogValue(joyState.x);
            playerControl.Fire = joyState.Buttons[3] > 0;
            playerControl.FireSpecial = joyState.Buttons[4] > 0;
        }
    }

#endif
    public class InputControlXboxController : IInputControl 
    {
        public PropertyInfo Thrust;
        public PropertyInfo RotateLeftRight;
        public int ThrustKey;
        public int RotateLeft;
        public int RotateRight;
        public int Fire;
        public int FireSpecial;


        public void HandleInput(PlayerControl playerControl)
        {
            GamePadState padState = GamePad.GetState(PlayerIndex.One);
            if(!padState.IsConnected)
                return;

            playerControl.Rotation = padState.ThumbSticks.Left.X;
            playerControl.Thrust = Math.Max(0, padState.ThumbSticks.Right.Y);

            if(padState.Buttons.LeftShoulder == ButtonState.Pressed)
                playerControl.Rotation = -1;
            if(padState.Buttons.RightShoulder == ButtonState.Pressed)
                playerControl.Rotation = 1;
            if(padState.Buttons.Back == ButtonState.Pressed)
                playerControl.Thrust = 1;

            playerControl.Fire        = padState.Buttons.A == ButtonState.Pressed;
			playerControl.FireSpecial = padState.Buttons.B == ButtonState.Pressed;

        }
    }
*/
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
                    c.FireSpecial = Keys.Space;
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
                    c.FireSpecial = Keys.Enter;
                    c.Fire = Keys.NumPad0;
                    c.RotateRight = Keys.NumPad6;
                    c.RotateLeft = Keys.NumPad4;
                    c.ThrustKey = Keys.NumPad5;
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

            playerControl.Thrust = state.IsKeyDown(keyConfig.ThrustKey) ? 1 : 0;
            playerControl.Rotation = state.IsKeyDown(keyConfig.RotateLeft) ? -1 : 0;
            playerControl.Rotation += state.IsKeyDown(keyConfig.RotateRight) ? 1 : 0;

            playerControl.Fire = state.IsKeyDown(keyConfig.Fire);
            playerControl.FireSpecial = state.IsKeyDown(keyConfig.FireSpecial);
        }


    }

	/// <summary>
	/// Summary description for InputHandler.
	/// </summary>
    public class InputHandler
    {
        static private IInputControl[] playerInput;

        static private PlayerControl[] player;

        static public PlayerControl[] Player      { get { return player; } }
        static public bool           KeyAction;

        

        static public bool HandleInput()
        {
            for(int j = 0; j < playerInput.Length; j++)
            {
                playerInput[j].HandleInput(player[j]);
            }
            return true;
        }

        static public void Initialize()
        {
            int nrOfPlayers = Settings.Current.Players.Length;
            player = new PlayerControl[nrOfPlayers];
            for(int i = 0; i < player.Length; i++)
                player[i] = new PlayerControl();

            playerInput = new IInputControl[nrOfPlayers];
        

	     GamePadState state = GamePad.GetState(PlayerIndex.Two);

            List<IInputControl> inputHandlers = new List<IInputControl>();
            //int count = Joystick.GetJoystickCount();
            //if(count > 0)
            //    inputHandlers.Add(new InputControlJoystick(0));

/** san
            DirectX8Class directx = new DirectX8Class();
            DirectInput8 input = directx.DirectInputCreate();
            DirectInputEnumDevices8 gameControllers = input.GetDIDevices(CONST_DI8DEVICETYPE.DI8DEVCLASS_GAMECTRL, CONST_DIENUMDEVICESFLAGS.DIEDFL_ATTACHEDONLY);
            for(int i = 0; i < gameControllers.GetCount(); i++)
            {
                if(inputHandlers.Count >= nrOfPlayers)
                    break;

                DirectInputDeviceInstance8 gameController = gameControllers.GetItem(i + 1);
                inputHandlers.Add(new InputControlJoystick(input, gameController));
            }

*/

            if(inputHandlers.Count < nrOfPlayers)
                inputHandlers.Add(new InputControlKeyboard(InputControlKeyboard.KeyConfig.Player1));
            if(inputHandlers.Count < nrOfPlayers)
                inputHandlers.Add(new InputControlKeyboard(InputControlKeyboard.KeyConfig.Player2));

            playerInput = inputHandlers.ToArray();
        }
	}
}
