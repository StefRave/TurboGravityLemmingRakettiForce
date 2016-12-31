using System;
using System.Collections.Generic;
using System.Threading;

namespace TurboPort.Event
{
    public class GameObjectStore
    {
        private static readonly GameSerializer Serializer = GameSerializer.Instance;
        private static int idCounter;
        private readonly Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, Func<GameObject>> objectCreators = new Dictionary<int, Func<GameObject>>();
        public GameEventStore EventStore { get; }

        public GameObjectStore(GameEventStore gameEventStore)
        {
            EventStore = gameEventStore;
        }

        private static int CreateUniqueId()
        {
            return Interlocked.Increment(ref idCounter);
        }

        public void RegisterCreation<TGameObject>(Func<TGameObject> creator) 
            where TGameObject : GameObject
        {
            int typeId = Serializer.RegisterGameEvents(typeof(TGameObject));
            objectCreators.Add(typeId, creator);
        }

        public TObject CreateAsOwner<TObject>() where TObject : GameObject
        {
            TObject gameObject = (TObject)Create(Serializer.GetTypeId(typeof(TObject)));

            gameObject.IsOwner = true;
            gameObject.ObjectId = CreateUniqueId();
            gameObject.gameStore = this;
            gameObjects.Add(gameObject.ObjectId, gameObject);

            gameObject.WillBeSerialized = true;
            EventStore.AddNewObject(gameObject);

            return gameObject;
        }

        public GameObject CreateFromExternal(int typeId, int objectId, double gameObjectGameTime)
        {
            GameObject gameObject;
            if (gameObjects.TryGetValue(objectId, out gameObject))
                return gameObject; // probably caused by duplicate package on udp

            gameObject = Create(typeId);

            gameObject.IsOwner = false;
            gameObject.ObjectId = objectId;
            gameObject.gameStore = this;
            gameObject.LastUpdatedGameTime = gameObjectGameTime;
            gameObjects.Add(gameObject.ObjectId, gameObject);

            return gameObject;
        }

        private GameObject Create(int typeId)
        {
            Func<GameObject> creator = objectCreators[typeId];

            GameObject gameObject = creator.Invoke();
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