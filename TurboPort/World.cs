using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TurboPort
{
    public class GameWorld
    {
        public MissileProjectileFactory ProjectileFactory { get; private set; }
        private readonly List<ShipBase> playerShipBases = new List<ShipBase>();
        private readonly List<ObjectShip> playerShips = new List<ObjectShip>();
        public ILevelBackground LevelBackground { get; set; }
        

        public IReadOnlyList<ShipBase> PlayerShipBases { get { return playerShipBases; } }
        public IReadOnlyList<ObjectShip> PlayerShips { get { return playerShips; } }

        public GameWorld(Game game)
        {
            ProjectileFactory = new MissileProjectileFactory(game, this);
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
