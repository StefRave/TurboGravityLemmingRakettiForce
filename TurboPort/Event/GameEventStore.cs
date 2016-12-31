using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace TurboPort.Event
{
    public class GameEventStore
    {
        private static readonly GameSerializer Serializer = GameSerializer.Instance;
        private readonly List<GameObject> modifiedGameObjects = new List<GameObject>(10000);
        private readonly List<GameObject> newGameObjects = new List<GameObject>(10000);
        private double totalGameTimeSeconds;

        public void ClearRecordedObjects()
        {
            modifiedGameObjects.Clear();
            newGameObjects.Clear();
        }

        public void SetGameTime(GameTime gameTime)
        {
            totalGameTimeSeconds = gameTime.TotalGameTime.TotalSeconds;
        }

        public void SerializeModifiedObjects(Stream eventStream)
        {
            var objectInfo = new GameSerializer.ObjectInfo();
            foreach (var gameObject in newGameObjects)
            {
                objectInfo.GameTime = gameObject.LastUpdatedGameTime;
                objectInfo.CreateTypeId = Serializer.GetTypeId(gameObject.GetType());
                objectInfo.ObjectId = gameObject.ObjectId;

                Serializer.Serialize(eventStream, gameObject, objectInfo);
                gameObject.WillBeSerialized = false;
                gameObject.ObjectStored();
            }

            foreach (var gameObject in modifiedGameObjects)
            {
                objectInfo.GameTime = gameObject.LastUpdatedGameTime;
                objectInfo.CreateTypeId = 0;
                objectInfo.ObjectId = gameObject.ObjectId;

                Serializer.Serialize(eventStream, gameObject, objectInfo);
                gameObject.WillBeSerialized = false;
                gameObject.ObjectStored();
            }
            ClearRecordedObjects();
        }

        public void AddNewObject(GameObject gameObject)
        {
            gameObject.WillBeSerialized = true;
            gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
            newGameObjects.Add(gameObject);
        }

        public void AddEvent(GameObject gameObject)
        {
            if (gameObject.WillBeSerialized)
                return;

            gameObject.WillBeSerialized = true;
            gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
            modifiedGameObjects.Add(gameObject);
        }
    }
}