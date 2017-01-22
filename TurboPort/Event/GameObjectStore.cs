using System;
using System.Collections.Generic;
using System.Threading;

namespace TurboPort.Event
{
    public partial class GameObjectStore
    {
        private static readonly GameSerializer Serializer = GameSerializer.Instance;
        private static int idCounter;
        private readonly Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, Func<GameObject>> gameObjectCreators = new Dictionary<int, Func<GameObject>>();
        private readonly Dictionary<int, Action<IGameMessage>> gameMessageActions = new Dictionary<int, Action<IGameMessage>>();
        private readonly Dictionary<int, Func<IGameMessage>> gameMessageCreators = new Dictionary<int, Func<IGameMessage>>();
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
            int typeId = Serializer.RegisterGameMessageType(typeof(TGameObject));
            gameObjectCreators.Add(typeId, creator);
        }

        public void RegisterGameMessage(params Type[] types)
        {
            foreach (var type in types)
            {
                int typeId = Serializer.RegisterGameMessageType(type);
                gameMessageCreators.Add(typeId, () => (IGameMessage)Activator.CreateInstance(type));
            }
        }

        public IDisposable SubscribeToGameMessage<TGameMessage>(Action<TGameMessage> messageAction)
            where TGameMessage : IGameMessage, new()
        {
            int typeId = Serializer.GetTypeId(typeof(TGameMessage));

            gameMessageActions.Add(typeId, msg => messageAction((TGameMessage)msg));

            return new DisposeAction(() =>
                                     {
                                         gameMessageActions.Remove(typeId);
                                         gameMessageCreators.Remove(typeId);
                                     });
        }

        public TObject CreateAsOwner<TObject>() where TObject : GameObject
        {
            TObject gameObject = (TObject)CreateGameObject(Serializer.GetTypeId(typeof(TObject)));
            gameObject.SetObjectStore(this);

            gameObject.IsOwner = true;
            gameObject.ObjectId = CreateUniqueId();
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

            gameObject = CreateGameObject(typeId);
            gameObject.SetObjectStore(this);

            gameObject.IsOwner = false;
            gameObject.ObjectId = objectId;
            gameObject.LastUpdatedGameTime = gameObjectGameTime;
            gameObjects.Add(gameObject.ObjectId, gameObject);

            return gameObject;
        }

        private GameObject CreateGameObject(int typeId)
        {
            Func<GameObject> creator = gameObjectCreators[typeId];

            GameObject gameObject = creator.Invoke();
            return gameObject;
        }

        public IGameMessage CreateMessageObject(int typeId)
        {
            Func<IGameMessage> creator;
            if(!gameMessageCreators.TryGetValue(typeId, out creator))
                return null;

            IGameMessage gameMessage = creator.Invoke();
            return gameMessage;
        }

        public GameObject GetGameObject(int objectId)
        {
            GameObject gameObject;
            if (!gameObjects.TryGetValue(objectId, out gameObject))
                return null;

            return gameObject;
        }

        public void InvokeGameMessageAction(int typeId, IGameMessage gameMessage)
        {
            Action<IGameMessage> messageAction;
            if(gameMessageActions.TryGetValue(typeId, out messageAction))
                messageAction.Invoke(gameMessage);
        }

        public void InvokeGameMessageAction(IGameMessage gameMessage)
        {
            InvokeGameMessageAction(Serializer.GetTypeId(gameMessage.GetType()), gameMessage);
        }
    }
}