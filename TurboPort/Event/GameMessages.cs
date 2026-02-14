using ProtoBuf;

namespace TurboPort.Event
{
    public interface IGameMessage
    {
    }

    [ProtoContract]
    [GameEvent("helo")]
    public class AnybodyThereGameMessage : IGameMessage
    {
    }

    [ProtoContract]
    [GameEvent("Helo")]
    public class MasterIsHere : IGameMessage
    {
    }

    [ProtoContract]
    [GameEvent("gast")]
    public class GameStateRequest : IGameMessage
    {
    }
}
