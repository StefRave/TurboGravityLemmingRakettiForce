using System.Collections.Generic;
using System.IO;

namespace TurboPort.Event
{
    public class GameReplay
    {
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

        public GameReplay()
        {
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

        public void ProcessEventsUntilTime(double currentGameTime)
        {
            if(PlayStatus != Status.Playing)
                return;

            while (PlayStatus == Status.Playing)
            {
                if (currentGameTime < nextObjectInfo.GameTime - GameTimeDelta)
                    return;

                GameObject gameObject;
                if (nextObjectInfo.CreateTypeId != 0)
                {
                    gameObject = GameObjectStore.CreateFromExternal(nextObjectInfo.CreateTypeId, nextObjectInfo.ObjectId);
                }
                else
                {
                    gameObject = GameObjectStore.GetGameObject(nextObjectInfo.ObjectId);
                    if(gameObject == null)
                        continue;
                }
                gameObject.LastUpdatedGameTime = nextObjectInfo.GameTime;

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