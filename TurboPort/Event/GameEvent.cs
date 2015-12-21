using ProtoBuf;

namespace TurboPort.Event
{
    [ProtoContract]
    public abstract class GameEvent
    {
        [ProtoMember(1)]
        public int ObjectId;
        [ProtoMember(2)]
        public double GameTime;
    }
}