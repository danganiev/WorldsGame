using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsLib
{
    public static class ChunkHelper
    {
        public static readonly Vector3i SIZE = new Vector3i(16, 64, 16);
        public static readonly Vector3i MAX_VECTOR = new Vector3i(15, 63, 15);

        public static Vector3i GetLocalPosition(Vector3i worldPosition)
        {
            int xMod = worldPosition.X % SIZE.X;
            int yMod = worldPosition.Y % SIZE.Y;
            int zMod = worldPosition.Z % SIZE.Z;

            int localX = worldPosition.X >= 0 ? xMod : xMod != 0 ? SIZE.X + xMod : 0;
            int localY = worldPosition.Y >= 0 ? yMod : yMod != 0 ? SIZE.Y + yMod : 0;
            int localZ = worldPosition.Z >= 0 ? zMod : zMod != 0 ? SIZE.Z + zMod : 0;

            return new Vector3i(localX, localY, localZ);
        }
    }
}
