using System;
using System.IO;

namespace TurboPort.Event
{
    public class GameReplay
    {
        private readonly GameObjectStore gameStore;
        private static readonly GameSerializer s = GameSerializer.Instance;
        private Stream inputStream;

        public double GameTimeDelta { get; private set; }
        public Status PlayStatus { get; private set; }
        private readonly GameSerializer.ObjectInfo nextObjectInfo = new GameSerializer.ObjectInfo();

        public GameReplay(GameObjectStore gameStore)
        {
            this.gameStore = gameStore;
            PlayStatus = Status.Inactive;
        }

        public void Load(Stream inputStream)
        {
            this.inputStream = inputStream;
            s.DeserializeObjectInfo(inputStream, nextObjectInfo);
            PlayStatus = Status.Paused;
        }

        public void StartPlay(double currentGameTime)
        {
            if (PlayStatus != Status.Paused)
                return;

            GameTimeDelta = nextObjectInfo.GameTime - currentGameTime;

            PlayStatus = Status.Playing;
        }

        public void ReplayAll(double currentGameTime)
        {
            PlayStatus = Status.Playing;
            GameTimeDelta = nextObjectInfo.GameTime - currentGameTime;
            ProcessEventsUntilTime(10000000);
        }

        public void ProcessEventsUntilTime(double currentGameTime)
        {
            if(PlayStatus != Status.Playing)
                return;

            while (PlayStatus == Status.Playing)
            {
                double gameObjectGameTime = nextObjectInfo.GameTime - GameTimeDelta;
                if (currentGameTime < gameObjectGameTime)
                    break;

                ProcessEvent(gameObjectGameTime);

                if (inputStream.Position == inputStream.Length)
                    PlayStatus = Status.Finnished;
            }
        }

        private void ProcessEvent(double gameObjectGameTime)
        {
            if (nextObjectInfo.ObjectId != 0)
                ProcessGameObject(gameObjectGameTime);
            else
                ProcessGameMessage();
        }

        private void ProcessGameObject(double gameObjectGameTime)
        {
            GameObject gameObject;
            if (nextObjectInfo.CreateTypeId != 0)
            {
                gameObject = gameStore.CreateFromExternal(nextObjectInfo.CreateTypeId, nextObjectInfo.ObjectId,
                    gameObjectGameTime);
            }
            else
            {
                gameObject = gameStore.GetGameObject(nextObjectInfo.ObjectId);
                if (gameObject == null)
                    throw new Exception("Unknown object id");
            }

            s.Deserialize(inputStream, gameObject);
            if(!gameObject.IsOwner) // If we own the object the events would already have been processed (explosions etc..)
                gameObject.ProcessGameEvents();

            s.DeserializeObjectInfo(inputStream, nextObjectInfo);
        }

        private void ProcessGameMessage()
        {
            var gameMessage = gameStore.CreateMessageObject(nextObjectInfo.CreateTypeId);
            s.Deserialize(inputStream, gameMessage);
            gameStore.InvokeGameMessageAction(nextObjectInfo.CreateTypeId, gameMessage);
        }

        public enum Status 
        {
            Inactive,
            Paused,
            Playing,
            Finnished,
        }
    }
}