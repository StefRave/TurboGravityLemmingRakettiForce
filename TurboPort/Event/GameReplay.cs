using System.IO;

namespace TurboPort.Event
{
    public class GameReplay
    {
        private readonly GameObjectStore gameStore;
        private static readonly GameSerializer s;
        private Stream inputStream;
        public double GameTimeDelta { get; private set; }
        public Status PlayStatus { get; private set; }
        private readonly GameSerializer.ObjectInfo nextObjectInfo = new GameSerializer.ObjectInfo();


        static GameReplay()
        {
            s = new GameSerializer();
            s.Initialize();
        }

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

                GameObject gameObject;
                if (nextObjectInfo.CreateTypeId != 0)
                {
                    gameObject = gameStore.CreateFromExternal(nextObjectInfo.CreateTypeId, nextObjectInfo.ObjectId);
                }
                else
                {
                    gameObject = gameStore.GetGameObject(nextObjectInfo.ObjectId);
                    if(gameObject == null)
                        continue;
                }
                gameObject.LastUpdatedGameTime = gameObjectGameTime;

                s.Deserialize(inputStream, gameObject);
                gameObject.ProcessGameEvents();

                s.DeserializeObjectInfo(inputStream, nextObjectInfo);

                if (inputStream.Position == inputStream.Length)
                    PlayStatus = Status.Finnished;
            }
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