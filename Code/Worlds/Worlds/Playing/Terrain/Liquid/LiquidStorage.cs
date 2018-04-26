using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Terrain.Blocks.Types;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Liquid
{
    // Liquid rules
    // * If liquid drops on flat surface, it goes 2 times in each x/z direction, but 1 time diagonally. Adds 0.5, and 0.25 volume to containers if hits a container
    // * If liquid hits a container with another liquid, old liquid persists (will be overridden later with liquid rules)
    // * If liquid drops into empty container, it starts to fill it up
    // * We store liquid stuff in a liquid volume/container volume way
    // * If container volume exceeds finite limits, we consider it infinite and don't do any computations on it.

    // Say, 1 block full of liquid is said to have 16 measures of volume there.
    // If we have air under us, all 16 measures go down
    // If we have ground, and air in all linear directions from block, we spread 16 measures into
    // 4 * 4 measures. Therefore, if we have 3 unfilled blocks, we spread 16 into 5,5,6 measures

    internal class LiquidPool
    {
        internal static int NextPoolId = 0;

        internal int PoolId { get; set; }

        //        internal HashSet<BoundingBox> PoolVolumes { get; set; }

        internal HashSet<Vector3i> FullBlocks { get; set; }

        internal Dictionary<Vector3i, int> NonFullBlocks { get; set; } // int here is a filled volume

        internal float Volume { get; set; }

        internal BlockType LiquidType { get; set; }

        internal bool IsInfinite { get; set; }

        // V = h * A. I'll have to calculate mean area I guess
        internal float SurfaceHeight { get; set; }

        // I should also think how pools on border will behave (when ocean isn't fully loaded for example)

        // No dynamic surface height for now
        internal LiquidPool()
        {
            PoolId = NextPoolId;
            NextPoolId++;
        }

        internal bool IsPositionInPool(Vector3i position)
        {
            return FullBlocks.Contains(position) || NonFullBlocks.ContainsKey(position);
        }
    }

    internal class LiquidStorage
    {
        internal List<LiquidPool> ActivePools { get; set; }

        internal List<LiquidPool> AllPools { get; set; }
    }
}