using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;

namespace TurboPort.Event
{
    public class GameObjectStore
    {
        private static int idCounter;
        private static readonly GameSerializer Serializer;
        private readonly Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();
        private readonly List<GameObject> modifiedGameObjects = new List<GameObject>(10000);
        private readonly List<GameObject> newGameObjects = new List<GameObject>(10000);
        private readonly Dictionary<Type, Func<GameObject>> objectCreators = new Dictionary<Type, Func<GameObject>>();
        private readonly GameSerializer.ObjectInfo objectInfo = new GameSerializer.ObjectInfo();
        private double totalGameTimeSeconds;

        static GameObjectStore()
        {
            Serializer = new GameSerializer();
            Serializer.Initialize();
        }

        public void ClearRecordedObjects()
        {
            modifiedGameObjects.Clear();
            newGameObjects.Clear();
        }

        private static int CreateUniqueId()
        {
            return Interlocked.Increment(ref idCounter);
        }

        public void SetGameTime(GameTime gameTime)
        {
            totalGameTimeSeconds = gameTime.TotalGameTime.TotalSeconds;
        }

        public void StoreModifiedObjects(Stream eventStream)
        {
            foreach (var gameObject in newGameObjects)
            {
                gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
                objectInfo.GameTime = totalGameTimeSeconds;
                objectInfo.CreateTypeId = Serializer.GetTypeId(gameObject.GetType());
                objectInfo.ObjectId = gameObject.ObjectId;

                Serializer.Serialize(eventStream, gameObject, objectInfo);
                gameObject.WillBeSerialized = false;
                gameObject.ObjectStored();
            }

            foreach (var gameObject in modifiedGameObjects)
            {
                gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
                objectInfo.GameTime = totalGameTimeSeconds;
                objectInfo.CreateTypeId = 0;
                objectInfo.ObjectId = gameObject.ObjectId;

                Serializer.Serialize(eventStream, gameObject, objectInfo);
                gameObject.WillBeSerialized = false;
                gameObject.ObjectStored();
            }
            ClearRecordedObjects();
        }

        public void AddEvent(GameObject gameObject)
        {
            if (gameObject.WillBeSerialized)
                return;

            gameObject.WillBeSerialized = true;
            modifiedGameObjects.Add(gameObject);
        }

        public void RegisterCreation<TGameObject>(Func<TGameObject> creator) 
            where TGameObject : GameObject
        {
            objectCreators.Add(typeof(TGameObject), creator);
        }

        public TObject CreateAsOwner<TObject>() where TObject : GameObject
        {
            TObject gameObject = (TObject)Create(typeof(TObject));

            gameObject.IsOwner = true;
            gameObject.ObjectId = CreateUniqueId();
            gameObject.gameStore = this;
            gameObjects.Add(gameObject.ObjectId, gameObject);

            gameObject.WillBeSerialized = true;
            newGameObjects.Add(gameObject);

            return gameObject;
        }

        public GameObject CreateFromExternal(int typeId, int objectId)
        {
            GameObject gameObject;
            if (gameObjects.TryGetValue(objectId, out gameObject))
                return gameObject; // probably caused by duplicate package on udp

            gameObject = Create(Serializer.GetTypeFromTypeId(typeId));

            gameObject.IsOwner = false;
            gameObject.ObjectId = objectId;
            gameObject.gameStore = this;
            gameObjects.Add(gameObject.ObjectId, gameObject);

            return gameObject;
        }

        private GameObject Create(Type type)
        {
            Func<GameObject> creator;
            if (!objectCreators.TryGetValue(type, out creator))
                throw new Exception($"No creator registered to create GameObject from {type.FullName}");

            GameObject gameObject = creator.Invoke();
            gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
            return gameObject;
        }

        public GameObject GetGameObject(int objectId)
        {
            GameObject gameObject;
            if (!gameObjects.TryGetValue(objectId, out gameObject))
                return null;

            return gameObject;
        }
    }
}