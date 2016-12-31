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
        private static GameSerializer instance;
        private RuntimeTypeModel model;
        private readonly Dictionary<Type, int> typeIdForType = new Dictionary<Type, int>();

        private GameSerializer()
        {
        }

        public static GameSerializer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(GameSerializer))
                    {
                        if (instance == null)
                        {
                            var gameSerializer = new GameSerializer();
                            gameSerializer.Initialize();

                            // ensures that the instance is well initialized,
                            // and only then, it assigns the static variable.
                            // http://stackoverflow.com/a/12945510/3714267
                            System.Threading.Thread.MemoryBarrier();
                            instance = gameSerializer;
                        }
                    }
                }
                return instance;
            }
        }

        public void Initialize()
        {
            model = TypeModel.Create();
            model.UseImplicitZeroDefaults = false;

            RegisterEventTypes();
        }

        private void RegisterEventTypes()
        {
            model.Add(typeof (Vector3), false).Add(1, "X").Add(2, "Y").Add(3, "Z");
            model.Add(typeof (ObjectInfo), true);
        }

        public int RegisterGameEvents(Type gameObjectType)
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
            return typeIdForType[type];
        }

        public void Serialize(Stream stream, GameObject gameObject, ObjectInfo objectInfo)
        {
            model.SerializeWithLengthPrefix(stream, objectInfo, typeof(ObjectInfo), PrefixStyle.Fixed32BigEndian, 0);
            model.SerializeWithLengthPrefix(stream, gameObject, gameObject.GetType(), PrefixStyle.Fixed32BigEndian, 0);
        }

        public void DeserializeObjectInfo(Stream stream, ObjectInfo objectInfo)
        {
            model.DeserializeWithLengthPrefix(stream, objectInfo, typeof(ObjectInfo), PrefixStyle.Fixed32BigEndian, 0);
        }

        public void Deserialize(Stream stream, GameObject gameObject)
        {
            model.DeserializeWithLengthPrefix(stream, gameObject, gameObject.GetType(), PrefixStyle.Fixed32BigEndian, 0);
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