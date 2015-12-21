using System;
using Microsoft.Xna.Framework;

namespace TurboPort
{
    public class ShipBase
    {
        public Vector3 Position;
        
        public void Interact(ObjectShip ship)
        {
            if (ship.HasLanded)
                return;

            if (ship.Velocity.Y >= 0)
                return;

            if (Math.Abs(ship.Position.X - Position.X) >= 16)
                return;

            if (ship.Position.Y > (Position.Y + 1))
                return;
            if (ship.Position.Y < (Position.Y - 1))
                return;

            double pop = Math.Cos(ship.Rotation.Z);
            if (pop >= 0.90)
            {
                ship.LandShip(Position.Y);
            }
        }

        public ShipBase(Vector3 position)
        {
            Position = position;
        }
    }
}