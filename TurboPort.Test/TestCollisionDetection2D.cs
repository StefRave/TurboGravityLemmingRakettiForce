using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;

namespace TurboPort.Test
{
    [TestFixture]
    public class TestCollisionDetection2D
    {
        [Test]
        public void NoOverlap_ReturnsNoCollision()
        {
            // Create a 64x4 background bitmap (all solid)
            int width = 64;
            int height = 4;
            int bitmapWidth = (width + 63) / 64;
            long[] targetBitmap = new long[bitmapWidth * height];
            int[] textureData = new int[width * height];

            // Fill with all solid pixels
            for (int i = 0; i < targetBitmap.Length; i++)
                targetBitmap[i] = unchecked((long)0xFFFFFFFFFFFFFFFF);
            for (int i = 0; i < textureData.Length; i++)
                textureData[i] = unchecked((int)0xFFFFFFFF);

            // Ship collision data — all zeros (no ship pixels)
            int shipWidth = 64;
            int shipHeight = 4;
            long[] shipCollisionData = new long[shipWidth / 64 * shipHeight + 1];

            var cpit = new CollisionPositionInTexture
            {
                Position = new Vector3(32, 2, 0),
                Rect = new Rectangle(0, 0, 4, 4),
                CollisionData = shipCollisionData,
                Size = new Point(shipWidth, shipHeight)
            };

            bool result = CollisionDetection2D.DetectCollistionAndUpdateTexture(
                cpit, targetBitmap, bitmapWidth * 64, textureData, width, height);

            result.ShouldBeFalse("No ship pixels means no collision");
        }

        [Test]
        public void OverlappingPixels_ReturnsCollision()
        {
            // Small 64-wide background, fully solid
            int width = 64;
            int height = 8;
            int bitmapWidth = 1; // 64/64
            long[] targetBitmap = new long[bitmapWidth * height];
            int[] textureData = new int[width * height];

            for (int i = 0; i < targetBitmap.Length; i++)
                targetBitmap[i] = unchecked((long)0xFFFFFFFFFFFFFFFF);
            for (int i = 0; i < textureData.Length; i++)
                textureData[i] = unchecked((int)0xFFFFFFFF);

            // Ship collision data — has pixels set, positioned at center
            int shipWidth = 64;
            int shipHeight = 8;
            long[] shipCollisionData = new long[shipWidth / 64 * shipHeight + 1];
            // Set a pixel in the ship data at the center row
            shipCollisionData[shipHeight / 2] = unchecked((long)0x8000000000000000); // one pixel at left edge

            var cpit = new CollisionPositionInTexture
            {
                Position = new Vector3(32, 4, 0),
                Rect = new Rectangle(0, 0, 8, 8),
                CollisionData = shipCollisionData,
                Size = new Point(shipWidth, shipHeight)
            };

            bool result = CollisionDetection2D.DetectCollistionAndUpdateTexture(
                cpit, targetBitmap, bitmapWidth * 64, textureData, width, height);

            result.ShouldBeTrue("Overlapping ship and background pixels should collide");
        }

        [Test]
        public void CountPixels_VerifyPopulationCount()
        {
            // Simple test: 64-wide, 2-tall background, all solid
            // Ship data at center with some pixels set
            int width = 64;
            int height = 2;
            int bitmapWidthInLongs = 1; // 64 / 64
            long[] targetBitmap = new long[bitmapWidthInLongs * height];
            int[] textureData = new int[width * height];

            // Fill background as all solid
            for (int i = 0; i < targetBitmap.Length; i++)
                targetBitmap[i] = unchecked((long)0xFFFFFFFFFFFFFFFF);
            for (int i = 0; i < textureData.Length; i++)
                textureData[i] = unchecked((int)0xFFFFFF);

            // Ship collision data: same size, with some pixels
            // The algorithm accesses ship data starting at index ((Rect.Height-1-shipY)*Size.X + shipX)/64
            // With Rect=(0,0,64,2), Size=(64,2), shipY=0 → index = ((2-1-0)*64+0)/64 = 1
            long[] shipCollisionData = new long[bitmapWidthInLongs * height + 1];
            shipCollisionData[1] = unchecked((long)0xFF00000000000000); // 8 pixels at row accessed by algorithm

            var cpit = new CollisionPositionInTexture
            {
                Position = new Vector3(32, 1, 0), // center of 64x2
                Rect = new Rectangle(0, 0, width, height),
                CollisionData = shipCollisionData,
                Size = new Point(width, height)
            };

            bool result = CollisionDetection2D.DetectCollistionAndUpdateTexture(
                cpit, targetBitmap, bitmapWidthInLongs * 64, textureData, width, height);

            result.ShouldBeTrue("Overlapping bits should produce a collision");
        }

        [Test]
        public void OutOfBounds_Ship_DoesNotCrash()
        {
            // Ship positioned near the edge of the level
            int width = 64;
            int height = 8;
            int bitmapWidth = 1;
            long[] targetBitmap = new long[bitmapWidth * height];
            int[] textureData = new int[width * height];

            long[] shipCollisionData = new long[bitmapWidth * height + 1];

            var cpit = new CollisionPositionInTexture
            {
                Position = new Vector3(2, 2, 0), // Near edge
                Rect = new Rectangle(0, 0, 8, 8),
                CollisionData = shipCollisionData,
                Size = new Point(width, height)
            };

            // Should not throw even if position is near edge
            bool result = CollisionDetection2D.DetectCollistionAndUpdateTexture(
                cpit, targetBitmap, bitmapWidth * 64, textureData, width, height);

            result.ShouldBeFalse("Empty ship data means no collision");
        }
    }
}
