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

            if (ship.Velocity.Y > 0)
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
                SoundHandler.TochDown();
                ship.HasLanded = true;
                ship.Velocity = Vector3.Zero;
                ship.Position.Y = Position.Y;
                ship.Rotation.Z = 0;
            }
        }

        public ShipBase(Vector3 position)
        {
            Position = position;
        }
    }
}