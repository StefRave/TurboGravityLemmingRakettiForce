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
        private GameEvent nextEvent;
        

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
            nextEvent = s.Deserialize(inputStream);
            PlayStatus = Status.Paused;
        }

        public void StartPlay(double currentGameTime)
        {
            if (PlayStatus != Status.Paused)
                return;

            if (nextEvent != null)
                GameTimeDelta = nextEvent.GameTime - currentGameTime;

            PlayStatus = Status.Playing;
        }

        public IEnumerable<GameEvent> GetEventsUntilTime(double currentGameTime)
        {
            if(PlayStatus != Status.Playing)
                yield break;

            while (nextEvent != null)
            {
                if (currentGameTime < nextEvent.GameTime - GameTimeDelta)
                    yield break;

                nextEvent.GameTime += GameTimeDelta;
                yield return nextEvent;
                nextEvent = s.Deserialize(inputStream);
            }
            PlayStatus = Status.Finnished;
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