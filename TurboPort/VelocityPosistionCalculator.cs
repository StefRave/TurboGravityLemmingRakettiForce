using System;
using Microsoft.Xna.Framework;

namespace TurboPort
{
    public class VelocityPosistionCalculator
    {
        public double Mass;
        public double Drag = 0.7;
        public double Gravity = 80;

        public void CalcVelocityAndPosition(ref Vector3 position, ref Vector3 velocity, double elapsedTime, float thrustAngle, float thrust)
        {
            var gc = Math.Sin(-thrustAngle) * thrust / Mass * 100;
            var vzg = velocity.X*Drag - gc;
            position.X += (float) ((gc*elapsedTime + (vzg - vzg*Math.Exp(-Drag*elapsedTime))/Drag)/Drag);
            velocity.X = (float) (gc/Drag + Math.Exp(-Drag*elapsedTime)*vzg/Drag);

            gc = -Gravity + Math.Cos(thrustAngle)*thrust / Mass * 100;
            vzg = velocity.Y*Drag - gc;
            position.Y += (float) ((gc*elapsedTime + (vzg - vzg*Math.Exp(-Drag*elapsedTime))/Drag)/Drag);
            velocity.Y = (float) (gc/Drag + Math.Exp(-Drag*elapsedTime)*vzg/Drag);
        }
    }
}