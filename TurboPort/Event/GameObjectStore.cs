using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TurboPort.Event
{
    public class GameObjectStore
    {
        private static int idCounter;
        private static MemoryStream gameEvents = new MemoryStream(10000000);
        private static GameSerializer s;
        private static readonly Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();
        private static readonly List<GameObject> modifiedGameObjects = new List<GameObject>(10000);
        private static readonly List<GameObject> newGameObjects = new List<GameObject>(10000);
        private static readonly Dictionary<Type, Func<GameObject>> objectCreators = new Dictionary<Type, Func<GameObject>>();
        private static readonly GameSerializer.ObjectInfo objectInfo = new GameSerializer.ObjectInfo();

        static GameObjectStore()
        {
            s = new GameSerializer();
            s.Initialize();
        }

        public static void Clear()
        {
            gameObjects.Clear();
            gameEvents.Position = 0;
        }

        private static int CreateUniqueId()
        {
            return Interlocked.Increment(ref idCounter);
        }

        public static void StoreModifiedObjects(double totalGameTimeSeconds)
        {
            foreach (var gameObject in newGameObjects)
            {
                gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
                objectInfo.GameTime = totalGameTimeSeconds;
                objectInfo.CreateTypeId = s.GetTypeId(gameObject.GetType());
                objectInfo.ObjectId = gameObject.ObjectId;

                s.Serialize(gameEvents, gameObject, objectInfo);
                gameObject.WillBeSerialized = false;
                gameObject.ObjectStored();
            }
            newGameObjects.Clear();

            foreach (var gameObject in modifiedGameObjects)
            {
                gameObject.LastUpdatedGameTime = totalGameTimeSeconds;
                objectInfo.GameTime = totalGameTimeSeconds;
                objectInfo.CreateTypeId = 0;
                objectInfo.ObjectId = gameObject.ObjectId;

                s.Serialize(gameEvents, gameObject, objectInfo);
                gameObject.WillBeSerialized = false;
                gameObject.ObjectStored();
            }
            modifiedGameObjects.Clear();
        }

        public static void AddEvent(GameObject gameObject)
        {
            if (gameObject.WillBeSerialized)
                return;

            gameObject.WillBeSerialized = true;
            modifiedGameObjects.Add(gameObject);
        }

        public static void RegisterCreation<TGameObject>(Func<TGameObject> creator) 
            where TGameObject : GameObject
        {
            objectCreators.Add(typeof(TGameObject), creator);
        }

        public static TObject CreateAsOwner<TObject>() where TObject : GameObject
        {
            TObject gameObject = (TObject)Create(typeof(TObject));

            gameObject.IsOwner = true;
            gameObject.ObjectId = CreateUniqueId();
            gameObjects.Add(gameObject.ObjectId, gameObject);

            gameObject.WillBeSerialized = true;
            newGameObjects.Add(gameObject);

            return gameObject;
        }

        public static GameObject CreateFromExternal(int typeId, int objectId)
        {
            var gameObject = Create(s.GetTypeFromTypeId(typeId));

            gameObject.IsOwner = false;
            gameObject.ObjectId = objectId;
            gameObjects.Add(gameObject.ObjectId, gameObject);

            return gameObject;
        }

        private static GameObject Create(Type type)
        {
            Func<GameObject> creator;
            if (!objectCreators.TryGetValue(type, out creator))
                throw new Exception($"No creator registered to create GameObject from {type.FullName}");

            GameObject gameObject = creator.Invoke();

            return gameObject;
        }

        public static GameObject GetGameObject(int objectId)
        {
            GameObject gameObject;
            if (!gameObjects.TryGetValue(objectId, out gameObject))
                return null;

            return gameObject;
        }

        public static void Store(Stream stream)
        {
            stream.Write(gameEvents.GetBuffer(), 0, (int)gameEvents.Length);
        }
    }
}