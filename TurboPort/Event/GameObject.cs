namespace TurboPort.Event
{
    public abstract class GameObject
    {
        public int ObjectId { get; internal set; }
        /// <summary>
        /// false if from replay or other client
        /// </summary>
        public bool IsOwner { get; internal set; }

        public void PublishEvent(GameEvent e)
        {
            GameObjectStore.AddEvent(this, e);
        }

        public abstract void ApplyGameEvent(GameEvent e);
    }
}