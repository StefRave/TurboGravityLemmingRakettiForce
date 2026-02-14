using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;

namespace TurboPort.Event
{
    public partial class GameObjectStore
    {
        internal readonly GameSerializer Serializer = new();
        private int idCounter;
        private readonly Dictionary<int, GameObject> gameObjects = new();
        private readonly Dictionary<int, Func<GameObject>> gameObjectCreators = new();
        private readonly Dictionary<int, Action<IGameMessage>> gameMessageActions = new();
        private readonly Dictionary<int, Func<IGameMessage>> gameMessageCreators = new();

        // Event accumulation (merged from GameEventStore)
        private readonly List<GameObject> modifiedGameObjects = new(10000);
        private readonly List<GameObject> newGameObjects = new(10000);
        private readonly List<IGameMessage> gameMessages = new(10000);
        private double totalGameTimeSeconds;

        public IReadOnlyCollection<GameObject> ModifiedGameObjects => modifiedGameObjects;
        public IReadOnlyCollection<GameObject> NewGameObjects => newGameObjects;
        public IReadOnlyCollection<IGameMessage> GameMessages => gameMessages;

        private int CreateUniqueId()
        {
            return Interlocked.Increment(ref idCounter);
        }

        public void RegisterCreation<TGameObject>(Func<TGameObject> creator)
            where TGameObject : GameObject
        {
            int typeId = Serializer.RegisterGameMessageType(typeof(TGameObject));
            gameObjectCreators.Add(typeId, creator);
        }

        public IDisposable SubscribeToGameMessage<TGameMessage>(Action<TGameMessage> messageAction)
            where TGameMessage : IGameMessage, new()
        {
            var type = typeof(TGameMessage);

            // Auto-register the message type if not already registered
            if (!Serializer.TryGetTypeId(type, out int typeId))
                typeId = Serializer.RegisterGameMessageType(type);

            if (!gameMessageCreators.ContainsKey(typeId))
                gameMessageCreators.Add(typeId, () => new TGameMessage());

            gameMessageActions[typeId] = msg => messageAction((TGameMessage)msg);

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
            AddNewObject(gameObject);

            return gameObject;
        }

        public GameObject CreateFromExternal(int typeId, int objectId, double gameObjectGameTime)
        {
            if (gameObjects.TryGetValue(objectId, out var gameObject))
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
            var creator = gameObjectCreators[typeId];

            GameObject gameObject = creator.Invoke();
            return gameObject;
        }

        public IGameMessage CreateMessageObject(int typeId)
        {
            if (!gameMessageCreators.TryGetValue(typeId, out var creator))
                return null;

            return creator.Invoke();
        }

        public GameObject GetGameObject(int objectId)
        {
            return gameObjects.GetValueOrDefault(objectId);
        }

        public void InvokeGameMessageAction(int typeId, IGameMessage gameMessage)
        {
            if (gameMessageActions.TryGetValue(typeId, out var messageAction))
                messageAction.Invoke(gameMessage);
        }

        public void InvokeGameMessageAction(IGameMessage gameMessage)
        {
            InvokeGameMessageAction(Serializer.GetTypeId(gameMessage.GetType()), gameMessage);
        }

        // --- Event accumulation methods (merged from GameEventStore) ---

        public void ClearRecordedObjects()
        {
            modifiedGameObjects.Clear();
            newGameObjects.Clear();
            gameMessages.Clear();
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

            foreach (var gameMessage in gameMessages)
            {
                objectInfo.GameTime = totalGameTimeSeconds;
                objectInfo.CreateTypeId = Serializer.GetTypeId(gameMessage.GetType());
                objectInfo.ObjectId = 0;

                Serializer.Serialize(eventStream, gameMessage, objectInfo);
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

        public void AddMessage(IGameMessage gameMessage)
        {
            // Auto-register the message type for serialization if not already registered
            var type = gameMessage.GetType();
            if (!Serializer.TryGetTypeId(type, out int typeId))
                typeId = Serializer.RegisterGameMessageType(type);

            if (!gameMessageCreators.ContainsKey(typeId))
                gameMessageCreators.Add(typeId, () => (IGameMessage)Activator.CreateInstance(type));

            gameMessages.Add(gameMessage);
        }
    }
}