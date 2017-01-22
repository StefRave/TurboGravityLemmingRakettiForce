using System;
using System.Threading;
using System.Threading.Tasks;
using TurboPort.Event;
using TurboPort.Test;

namespace TurboPort
{
    public class MasterControl
    {
        private readonly GameObjectStore objectStore;
        private readonly GameWorld gameWorld;
        private readonly IDelayService delayService;
        private CancellationToken cancellationToken;
        public int PlayerId { get; } = 0;
        public Guid PlayerFullId { get; } = Guid.NewGuid();

        public MasterControl(GameObjectStore objectStore, GameWorld gameWorld) : this(objectStore, gameWorld, new DelayService())
        {
        }

        public MasterControl(GameObjectStore objectStore, GameWorld gameWorld, IDelayService delayService)
        {
            this.objectStore = objectStore;
            this.gameWorld = gameWorld;
            this.delayService = delayService;
        }

        public GameMode GameMode { get; set; }

        public void Initialize()
        {
            objectStore.RegisterGameMessage(typeof(AnybodyThereGameMessage));
            objectStore.RegisterGameMessage(typeof(MasterIsHere));
            objectStore.RegisterGameMessage(typeof(GameStateRequest));
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            if (GameMode == GameMode.UdpBroadCast)
            {
                foreach (ShipBase shipBase in gameWorld.PlayerShipBases)
                {
                    objectStore.CreateAsOwner<ObjectShip>()
                        .CreateInitialize(shipBase.Position);
                }
            }
            else if (GameMode == GameMode.Multiplayer)
            {
                //gameObjectStore.RegisterGameMessage<AnybodyThereGameMessage>(msg => actionPerformed = true);

                SendAnybodyThere();
                var gameOpen = await Wait<MasterIsHere>(TimeSpan.FromSeconds(1));
                if (gameOpen == null)
                {
                    SetupGameAsMaster();
                    await RunAsMaster();
                }
                else
                {
                    objectStore.EventStore.AddMessage(new GameStateRequest());
                }

                // Anybody there?
                // no -> Create player
                // yes wanna join? -> Request state + create player
                // yes we are bussy -> Request state + listen
            }
        }

        private void SetupGameAsMaster()
        {
            objectStore.CreateAsOwner<ObjectShip>()
                .CreateInitialize(gameWorld.PlayerShipBases[0].Position);
        }

        private async Task RunAsMaster()
        {
            try
            {
                objectStore.EventStore.AddMessage(new MasterIsHere());

                using (objectStore.SubscribeToGameMessage<AnybodyThereGameMessage>(HandleAsMaster))
                using (objectStore.SubscribeToGameMessage<GameStateRequest>(HandleAsMaster))
                {
                    await delayService.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                await PromoteNewMaster();
                throw;
            }
        }

        private void HandleAsMaster(GameStateRequest obj)
        {
            
        }

        private void HandleAsMaster(AnybodyThereGameMessage obj)
        {
            objectStore.EventStore.AddMessage(new MasterIsHere());
        }


        private Task PromoteNewMaster()
        {
            throw new NotImplementedException();
        }

        private async Task<T> Wait<T>(TimeSpan fromSeconds) where T : class, IGameMessage, new()
        {
            var completionSource = new TaskCompletionSource<T>();
            using (objectStore.SubscribeToGameMessage<T>(msg => completionSource.SetResult(msg)))
            {
                await Task.WhenAny(completionSource.Task, delayService.Delay(fromSeconds, cancellationToken));

                if (completionSource.Task.IsCompleted)
                    return completionSource.Task.Result;

                return null;
            }
        }

        private void SendAnybodyThere()
        {
            objectStore.EventStore.AddMessage(new AnybodyThereGameMessage());
        }
    }
}