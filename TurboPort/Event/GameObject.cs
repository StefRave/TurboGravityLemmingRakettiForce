using ProtoBuf;

namespace TurboPort.Event
{
    public abstract class GameObject
    {
        protected internal GameObjectStore gameStore;
        public int ObjectId { get; internal set; }

        public double LastUpdatedGameTime { get; internal set; }
        internal bool WillBeSerialized { get; set; }

        /// <summary>
        /// false if from replay or other client
        /// </summary>
        public bool IsOwner { get; internal set; }

        protected void PublishEvent()
        {
            gameStore.EventStore.AddEvent(this);
        }

        protected internal abstract void ProcessGameEvents();
        protected internal abstract void ObjectStored();
    }
}