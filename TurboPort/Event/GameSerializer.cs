using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using ProtoBuf;
using ProtoBuf.Meta;

namespace TurboPort.Event
{
    public class GameSerializer
    {
        private RuntimeTypeModel model;
        private readonly Dictionary<int, Type> typeForTypeId = new Dictionary<int, Type>();
        private readonly Dictionary<Type, int> typeIdForType = new Dictionary<Type, int>();

        public void Initialize()
        {
            model = TypeModel.Create();
            model.UseImplicitZeroDefaults = false;

            RegisterEventTypes();
            RegisterGameEvents();
        }

        private void RegisterEventTypes()
        {
            model.Add(typeof (Vector3), false).Add(1, "X").Add(2, "Y").Add(3, "Z");
            model.Add(typeof (ObjectInfo), true);
        }

        private void RegisterGameEvents()
        {
            var objectTypes = typeof (GameObject).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof (GameObject)));

            foreach (var derivedType in objectTypes)
            {
                var gameEventAttribute = derivedType.GetCustomAttribute<GameEventAttribute>();
                if (gameEventAttribute == null)
                    throw new Exception($"GameEventAttribute missing on type {derivedType.FullName}");

                if (derivedType.GetCustomAttribute<ProtoContractAttribute>() == null)
                    throw new Exception($"ProtoContractAttribute missing on type {derivedType.FullName}");

                typeForTypeId.Add(gameEventAttribute.IdFromFourLetters, derivedType);
                typeIdForType.Add(derivedType, gameEventAttribute.IdFromFourLetters);
            }
        }

        public Type GetTypeFromTypeId(int typeId)
        {
            return typeForTypeId[typeId];
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