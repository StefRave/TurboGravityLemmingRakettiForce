using TurboPort.Input;

namespace TurboPort
{
    public interface IControllerInputProcessor
    {
        void ProcessControllerInput(PlayerControl control);
    }
}