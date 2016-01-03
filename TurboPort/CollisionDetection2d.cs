using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TurboPort
{
    public class CollisionDetection2D
    {
        public static ulong[] intersectionBuffer = new ulong[1000];

        public static unsafe bool IntersectPixels2(CollisionPositionInTexture collisionPositionInTexture,
            long[] backCollisionData, int collisionLongWidth, int widthBack, Vector3 shipPosition,
            long[] collisionTextureData, int[] textureData, Texture2D texture)
        {
            int pixelsHit = 0;
            int backStartX = (int) shipPosition.X - (collisionPositionInTexture.Rect.Width/2);
            int backStartY = (int) shipPosition.Y - (collisionPositionInTexture.Rect.Height/2) + 1;
            int backEndX = backStartX + collisionPositionInTexture.Rect.Width;
            int backEndY = backStartY + collisionPositionInTexture.Rect.Height;

            var shipX = collisionPositionInTexture.Rect.X;
            var shipY = collisionPositionInTexture.Rect.Y;
            if (backStartX < 0)
            {
                shipX -= backStartX;
                backStartX = 0;
            }
            int overlap = widthBack - backEndX;
            if (overlap < 0)
            {
                backEndX += overlap;
            }
            overlap = texture.Height - backEndY;
            if (overlap < 0)
            {
                backEndY += overlap;
            }
            if (backStartY < 0)
            {
                shipY -= backStartY;
                backStartY = 0;
            }
            ulong backMaskStart = unchecked((ulong)~0) >> backStartX;
            ulong backMaskEnd = unchecked((ulong)~0) << -backEndX;
            int shiftShip = (shipX & 63) - (backStartX & 63);
            var shipMask = (shiftShip > 0) ? unchecked((ulong)~0) << -shiftShip : unchecked((ulong)~0) << shiftShip;
            int longsToCompareMinus1 = ((backEndX + 63) / 64) - (backStartX /64) - 1;

            var offsetShipLineDelta = collisionPositionInTexture.Size.X + (backEndX - backStartX);
            int offsetShip = (collisionPositionInTexture.Rect.Height - 1 - shipY) * collisionPositionInTexture.Size.X + shipX;
            uint intersectionVerticalHitMask = 0;
            int firstHitY = -1;
            int lastHitY = -1;
            fixed (long* backgroundCollisionTexture = backCollisionData)
            fixed (long* collisionTexture = collisionTextureData)
            {
                long* backgroundCollisionY0 = backgroundCollisionTexture + (texture.Height - 1) * collisionLongWidth;
                for (int y = backStartY; y < backEndY; y++)
                {
                    int startOffsetShip = offsetShip;

                    offsetShip = startOffsetShip;

                    ulong* bt = (ulong*)(backgroundCollisionY0 + backStartX / 64 - collisionLongWidth*y);
                    ulong* st = (ulong*)(collisionTexture + (offsetShip/64));
                    ulong shipPrev = 0;
                    if (shiftShip > 0)
                        shipPrev = *st++;
                    for (int i = 0; i <= longsToCompareMinus1; i++)
                    {
                        ulong bv = *bt++;
                        if (i == 0) bv &= backMaskStart;
                        if (i == longsToCompareMinus1) bv &= backMaskEnd;
                        ulong sv;
                        ulong current = sv = *st++;
                        if (shiftShip > 0)
                            sv = (shipPrev << shiftShip) | ((current & shipMask) << shiftShip);
                        else if(shiftShip < 0)
                            sv = ((shipPrev & shipMask) << shiftShip) | (current >> -shiftShip);

                        ulong maskResult = bv & sv;
                        if (maskResult != 0)
                        {
                            pixelsHit += CountPixels(maskResult);
                            intersectionVerticalHitMask |= (uint)(1 << i);
                            if (firstHitY < 0)
                                firstHitY = y;
                            lastHitY = y;
                            intersectionBuffer[(y - firstHitY)*(longsToCompareMinus1 + 1) + i] = maskResult;
                        }
                        shipPrev = current;
                    }
                    for (int x = backStartX; x < backEndX; x++)
                    {
                        int offset = offsetShip++;

                        ulong l = (ulong)*(collisionTexture + (offset / 64));
                        long* pos = backgroundCollisionY0 + x / 64 - collisionLongWidth * y;
                        ulong mask = 0x8000000000000000 >> x;
                        if ((l & (0x8000000000000000 >> offset)) != 0)
                        {
                            if ((*pos & (long)mask) != 0)
                            {
                                pixelsHit++;
                                *pos &= (long)~mask;

                                //textureData[x + texture.Width*(texture.Height - y - 1)] = 0xffff;
                            }
                        }
                    }
                    offsetShip -= offsetShipLineDelta;
                }

                for (int y = firstHitY; y <= lastHitY; y++)
                {
                    int pos = texture.Width * (texture.Height - y - 1) + (backStartX & ~63);
                    for (int i = 0; i <= longsToCompareMinus1; i++)
                    {
                        ulong buf = intersectionBuffer[(y - firstHitY)*(longsToCompareMinus1 + 1) + i];
                        for (int j = 0; j < 64; j++)
                        {
                            if((buf & 0x8000000000000000) != 0)
                                textureData[pos] = -1;
                            buf <<= 1;
                            pos++;
                        }
                        intersectionBuffer[(y - firstHitY)*(longsToCompareMinus1 + 1) + i] = 0;
                    }
                }
            }
            return pixelsHit > 0;
        }

        private static int CountPixels(ulong maskResult)
        {
            ulong value = maskResult;
            value = (value & 0x5555555555555555) + ((value >> 1) & 0x5555555555555555);
            value = (value & 0x3333333333333333) + ((value >> 2) & 0x3333333333333333);
            value = (value & 0x0f0f0f0f0f0f0f0f) + ((value >> 4) & 0x0f0f0f0f0f0f0f0f);
            value = value + (value >> 8);
            value = value + (value >> 16);
            value = value + (value >> 32);
            return (int) value & 0xff;
        }
    }
}