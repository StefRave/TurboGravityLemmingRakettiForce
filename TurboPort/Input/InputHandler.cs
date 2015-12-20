using System.Collections.Generic;

namespace TurboPort.Input
{
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

            var touchControl = TouchControl.Create();
            if(touchControl != null)
                playerInput[0].Add(touchControl);


            int playerIndex = 0;
            for (int controllerIndex = 0; controllerIndex < 4; controllerIndex++)
            {
                if (playerIndex >= nrOfPlayers)
                    break;

                var gamePadController = GamePadControl.CreateIfConnected(controllerIndex);
                if(gamePadController != null)
                    playerInput[playerIndex].Add(gamePadController);
            }

            // Default back to keyboard controls
            playerInput[0].Add(new KeyboardControl(KeyboardControl.KeyConfig.Player1));
            playerInput[1].Add(new KeyboardControl(KeyboardControl.KeyConfig.Player2));
        }
	}
}
