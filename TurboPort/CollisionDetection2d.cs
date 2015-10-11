using System;
using Microsoft.Xna.Framework;

namespace TurboPort
{
    public class CollisionDetection2D
    {
        static public bool IntersectPixels(Rectangle rectangleA, byte[] dataA, int widthA,
                            Rectangle rectangleB, byte[] dataB, int widthB)
        {
            // Find the bounds of the rectangle intersection
            int top = Math.Max(rectangleA.Top, rectangleB.Top);
            int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
            int left = Math.Max(rectangleA.Left, rectangleB.Left);
            int right = Math.Min(rectangleA.Right, rectangleB.Right);

            // Check every point within the intersection bounds
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    // Get the color of both pixels at this point
                    byte colorA = dataA[(x - rectangleA.Left) +
                                         (y - rectangleA.Top) * widthA];
                    byte colorB = dataB[(x - rectangleB.Left) +
                                         (rectangleB.Height - (y - rectangleB.Top)) * widthB];

                    // If both pixels are not completely transparent,
                    if (colorA != 0 && colorB != 0)
                    {
                        // then an intersection has been found
                        return true;
                    }
                }
            }

            // No intersection found
            return false;
        }
    }
}