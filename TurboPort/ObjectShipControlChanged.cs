using Microsoft.Xna.Framework;
using ProtoBuf;
using TurboPort.Event;

namespace TurboPort
{
    [GameEvent("ShCo")]
    [ProtoContract()]
    public class ObjectShipControlChanged : GameEvent
    {
        [ProtoMember(1)]
        public float Rotation;
        [ProtoMember(2)]
        public float Thrust;

        [ProtoMember(3)]
        public Vector3 Position;
        [ProtoMember(4)]
        public Vector3 Velocity;
    }

    [GameEvent("ShLa")]
    [ProtoContract()]
    public class ObjectShipHasLanded : GameEvent
    {
        [ProtoMember(1)]
        public Vector3 Position;
    }

    [GameEvent("ShCr")]
    [ProtoContract()]
    public class ObjectShipCreated : GameEvent, IGameObjectCreation
    {
        [ProtoMember(1)]
        public Vector3 Position;
    }

    [GameEvent("ShHB")]
    [ProtoContract()]
    public class ObjectShipHitBackground : GameEvent
    {
        [ProtoMember(1)]
        public Vector3 Velocity;
    }
}