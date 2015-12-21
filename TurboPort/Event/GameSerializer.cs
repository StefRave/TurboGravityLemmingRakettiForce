using System;
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
        private MetaType gameEventMetaType;

        public void Initialize()
        {
            model = TypeModel.Create();
            RegisterEventTypes();
            RegisterGameEvents();
        }

        private void RegisterEventTypes()
        {
            model.Add(typeof (Vector3), false).Add(1, "X").Add(2, "Y").Add(3, "Z");
            gameEventMetaType = model.Add(typeof (GameEvent), true);
        }

        private void RegisterGameEvents()
        {
            var eventTypes = typeof (GameEvent).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof (GameEvent)));

            foreach (var eventType in eventTypes)
            {
                var gameEventAttribute = eventType.GetCustomAttribute<GameEventAttribute>();
                if (gameEventAttribute == null)
                    throw new Exception($"GameEventAttribute missing on type {eventType.FullName}");

                if (eventType.GetCustomAttribute<ProtoContractAttribute>() == null)
                    throw new Exception($"ProtoContractAttribute missing on type {eventType.FullName}");

                gameEventMetaType.AddSubType(gameEventAttribute.IdFromFourLetters, eventType);
            }
        }

        public void Serialize(Stream stream, GameEvent gameEvent)
        {
            model.SerializeWithLengthPrefix(stream, gameEvent, null, PrefixStyle.Fixed32BigEndian, 0);
        }

        public GameEvent Deserialize(Stream stream)
        {
            var result = (GameEvent)model.DeserializeWithLengthPrefix(stream, null, typeof(GameEvent), PrefixStyle.Fixed32BigEndian, 0);
            return result;
        }
    }
}