using System;
using Microsoft.Xna.Framework;

namespace TurboPort
{
    public class ShipBase
    {
        public Vector3 Position;
        
        public void Interact(ObjectShip other)
        {
            if(other.Velocity.Y <= 0)
            {
                if(Math.Abs(other.Position.X - Position.X) < 16)
                {
                    if(other.Position.Y <= Position.Y)
                    {
                        if(other.OldPosition.Y >= Position.Y)
                        {
                            double pop = Math.Cos(other.Rotation.Z);
                            if(pop >= 0.93)
                            {
                                if(other.OldPosition.Y != Position.Y)
                                {
                                    SoundHandler.TochDown();
                                }
                                other.Velocity = Vector3.Zero;
                                other.Position.Y = Position.Y;
                                other.Rotation.Z = 0;
                            }
                            else
                            {
                                SoundHandler.Bigexp();
                            }
                        }
                    }
                }
            }
        }

        public ShipBase(Vector3 position)
        {
            Position = position;
        }
    }
}