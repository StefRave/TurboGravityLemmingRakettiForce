using System.Collections.Generic;

namespace TurboPort
{
    public class GameWorld
    {
        public MissleProjectileFactory ProjectileFactory { get; private set; }
        private readonly List<ShipBase> playerShipBases = new List<ShipBase>();
        private readonly List<ObjectShip> playerShips = new List<ObjectShip>();
        public ILevelBackground LevelBackground { get; set; }
        

        public IReadOnlyList<ShipBase> PlayerShipBases { get { return playerShipBases; } }
        public IReadOnlyList<ObjectShip> PlayerShips { get { return playerShips; } }

        public GameWorld(MissleProjectileFactory missleProjectileFactory)
        {
            ProjectileFactory = missleProjectileFactory;
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
