using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;

namespace TurboPort.Event
{
    public class GameObjectStore
    {
        private static int idCounter;
        private static MemoryStream gameEvents = new MemoryStream(10000000);
        private static double currentGameTime;
        private static GameSerializer s;
        private static readonly Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();
        private static readonly Dictionary<Type, Func<GameEvent, GameObject>> objectCreators = new Dictionary<Type, Func<GameEvent, GameObject>>();

        static GameObjectStore()
        {
            s = new GameSerializer();
            s.Initialize();
        }

        private static int CreateUniqueId()
        {
            return Interlocked.Increment(ref idCounter);
        }

        public static void SetGameTime(GameTime gameTime)
        {
            currentGameTime = gameTime.TotalGameTime.TotalSeconds;
        }

        public static void AddEvent(GameObject gameObject, GameEvent gameEvent)
        {
            gameEvent.ObjectId = gameObject.ObjectId;
            gameEvent.GameTime = currentGameTime;

            s.Serialize(gameEvents, gameEvent);

            gameObject.ApplyGameEvent(gameEvent);
        }

        public static GameObject AddCreationEvent(GameEvent gameEvent)
        {
            gameEvent.ObjectId = CreateUniqueId();
            gameEvent.GameTime = currentGameTime;

            s.Serialize(gameEvents, gameEvent);

            Func<GameEvent, GameObject> creator;
            if(!objectCreators.TryGetValue(gameEvent.GetType(), out creator))
                throw new Exception($"No creator registered to create GameObject from {gameEvent.GetType().FullName}");

            var gameObject = creator.Invoke(gameEvent);
            gameObject.ObjectId = gameEvent.ObjectId;
            gameObject.IsOwner = true;

            gameObjects.Add(gameObject.ObjectId, gameObject);
            return gameObject;
        }

        public static void Store()
        {
            File.WriteAllBytes("eventstream.turboport", gameEvents.ToArray());
        }

        public static void ProcessEvent(GameEvent gameEvent)
        {
            if (gameEvent is IGameObjectCreation)
            {
                ProcessCreationEvent(gameEvent);
            }
            else
            {
                GameObject gameObject;
                if (!gameObjects.TryGetValue(gameEvent.ObjectId, out gameObject))
                    return;

                gameObject.ApplyGameEvent(gameEvent);
            }
        }

        private static void ProcessCreationEvent(GameEvent gameEvent)
        {
            Func<GameEvent, GameObject> creator;
            if (!objectCreators.TryGetValue(gameEvent.GetType(), out creator))
                return;//throw new Exception($"No creator registered to create GameObject from {gameEvent.GetType().FullName}");

            var gameObject = creator.Invoke(gameEvent);
            gameObject.ObjectId = gameEvent.ObjectId;
            gameObjects.Add(gameObject.ObjectId, gameObject);
        }

        public static void RegisterCreation<TGameObject, TGameEvent>(Func<TGameEvent, TGameObject> creator) 
            where TGameObject : GameObject 
            where TGameEvent : GameEvent, IGameObjectCreation
        {
            objectCreators.Add(typeof(TGameEvent), ge => creator.Invoke((TGameEvent)ge));
        }
    }
}