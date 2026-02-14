using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TurboPort.Event
{
    public class GameSerializer
    {
        private readonly RuntimeTypeModel model;
        private readonly Dictionary<Type, int> typeIdForType = new();

        public GameSerializer()
        {
            model = RuntimeTypeModel.Create();
            model.UseImplicitZeroDefaults = false;

            model.Add(typeof(Vector3), false).Add(1, "X").Add(2, "Y").Add(3, "Z");
            model.Add(typeof(ObjectInfo), true);
        }

        public int RegisterGameMessageType(Type gameObjectType)
        {
            var gameEventAttribute = gameObjectType.GetCustomAttribute<GameEventAttribute>();
            if (gameEventAttribute == null)
                throw new Exception($"GameEventAttribute missing on type {gameObjectType.FullName}");

            if (gameObjectType.GetCustomAttribute<ProtoContractAttribute>() == null)
                throw new Exception($"ProtoContractAttribute missing on type {gameObjectType.FullName}");

            typeIdForType.Add(gameObjectType, gameEventAttribute.IdFromFourLetters);

            return gameEventAttribute.IdFromFourLetters;
        }

        public int GetTypeId(Type type)
        {
            if (!typeIdForType.TryGetValue(type, out var result))
                throw new Exception($"Type {type.FullName} is not registered in GameSerializer");
            return result;
        }

        public bool TryGetTypeId(Type type, out int typeId)
        {
            return typeIdForType.TryGetValue(type, out typeId);
        }

        public void Serialize(Stream stream, object obj, ObjectInfo objectInfo)
        {
            model.SerializeWithLengthPrefix(stream, objectInfo, typeof(ObjectInfo), PrefixStyle.Fixed32BigEndian, 0);
            model.SerializeWithLengthPrefix(stream, obj, obj.GetType(), PrefixStyle.Fixed32BigEndian, 0);
        }

        public void DeserializeObjectInfo(Stream stream, ObjectInfo objectInfo)
        {
            model.DeserializeWithLengthPrefix(stream, objectInfo, typeof(ObjectInfo), PrefixStyle.Fixed32BigEndian, 0);
        }

        public void Deserialize(Stream stream, GameObject gameObject)
        {
            model.DeserializeWithLengthPrefix(stream, gameObject, gameObject.GetType(), PrefixStyle.Fixed32BigEndian, 0);
        }

        public void Deserialize(Stream stream, IGameMessage gameMessage)
        {
            model.DeserializeWithLengthPrefix(stream, gameMessage, gameMessage.GetType(), PrefixStyle.Fixed32BigEndian, 0);
        }

        [ProtoContract]
        public class ObjectInfo
        {
            [ProtoMember(1)] public int ObjectId;
            [ProtoMember(2)] public double GameTime;
            [ProtoMember(3)] public int CreateTypeId;
        }
    }
}