using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TurboPort.Event;

namespace TurboPort
{
    public class GameWorld
    {
        public MissileProjectileFactory ProjectileFactory { get; private set; }
        private readonly List<ShipBase> playerShipBases = new List<ShipBase>();
        private readonly List<ObjectShip> playerShips = new List<ObjectShip>();
        public ILevelBackground LevelBackground { get; set; }
        

        public IReadOnlyList<ShipBase> PlayerShipBases => playerShipBases;
        public IReadOnlyList<ObjectShip> PlayerShips => playerShips;

        public GameWorld(Game game, GameObjectStore gameStore)
        {
            ProjectileFactory = new MissileProjectileFactory(game, this, gameStore);
        }

        public void AddPlayerShip(ObjectShip ship)
        {
            playerShips.Add(ship);
        }

        public void AddShipBase(ShipBase shipBase)
        {
            playerShipBases.Add(shipBase);
        }
    }
}
