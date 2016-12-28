namespace TurboPort
{
    public static class CollisionDetection2D
    {
        public static unsafe bool DetectCollistionAndUpdateTexture(
            CollisionPositionInTexture collisionPositionInTexture,
            long[] targetCollisionBitmap, int targetCollisionBitmapWidth,
            int[] targetTextureData, int targetTextureWidth, int targetTextureHeight)
        {
            int pixelsHit = 0;
            int backStartX = (int)collisionPositionInTexture.Position.X - (collisionPositionInTexture.Rect.Width/2);
            int backStartY = (int)collisionPositionInTexture.Position.Y - (collisionPositionInTexture.Rect.Height/2) + 1;
            int backEndX = backStartX + collisionPositionInTexture.Rect.Width;
            int backEndY = backStartY + collisionPositionInTexture.Rect.Height;

            var shipX = collisionPositionInTexture.Rect.X;
            var shipY = collisionPositionInTexture.Rect.Y;
            if (backStartX < 0)
            {
                shipX -= backStartX;
                backStartX = 0;
            }
            int overlap = targetTextureWidth - backEndX;
            if (overlap < 0)
            {
                backEndX += overlap;
            }
            overlap = targetTextureHeight - backEndY;
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

            fixed (long* backgroundCollisionTexture = targetCollisionBitmap)
            fixed (long* collisionTexture = collisionPositionInTexture.CollisionData)
            {
                long* textureCollisionPtrStart = collisionTexture + ((collisionPositionInTexture.Rect.Height - 1 - shipY) * collisionPositionInTexture.Size.X + shipX) / 64;
                long* backgroundCollistionPtrStart = backgroundCollisionTexture + (targetTextureHeight - 1 - backStartY) * targetCollisionBitmapWidth / 64 + backStartX / 64;

                for (int y = backStartY; y < backEndY; y++)
                {
                    ulong* backgroundCollisionPtr = (ulong*)backgroundCollistionPtrStart;
                    ulong* textureCollistionPtr = (ulong*)textureCollisionPtrStart;
                    ulong shipPrev = 0;
                    if (shiftShip > 0)
                        shipPrev = *textureCollistionPtr++;
                    for (int i = 0; i <= longsToCompareMinus1; i++)
                    {
                        ulong sv;
                        ulong current = *textureCollistionPtr++;
                        if (shiftShip > 0)
                            sv = (shipPrev << shiftShip) | ((current & shipMask) << shiftShip);
                        else if (shiftShip < 0)
                            sv = ((shipPrev & shipMask) << shiftShip) | (current >> -shiftShip);
                        else
                            sv = current;

                        ulong maskResult = *backgroundCollisionPtr & sv;
                        if (i == 0) maskResult &= backMaskStart;
                        if (i == longsToCompareMinus1) maskResult &= backMaskEnd;

                        if (maskResult != 0)
                        {
                            *backgroundCollisionPtr &= ~maskResult;

                            pixelsHit += CountPixels(maskResult);

                            int positionInTexture = i * 64 + (targetTextureWidth * (targetTextureHeight - y - 1) + (backStartX & ~63));
                            for (int j = 0; j < 64; j++)
                            {
                                if ((maskResult & 0x8000000000000000) != 0)
                                    targetTextureData[positionInTexture] = -1;
                                maskResult <<= 1;
                                positionInTexture++;
                            }
                        }
                        backgroundCollisionPtr++;
                        shipPrev = current;
                    }
                    textureCollisionPtrStart -= collisionPositionInTexture.Size.X / 64;
                    backgroundCollistionPtrStart -= targetCollisionBitmapWidth / 64;
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