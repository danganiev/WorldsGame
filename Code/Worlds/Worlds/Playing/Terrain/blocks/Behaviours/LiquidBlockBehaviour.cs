using System;
using Microsoft.Xna.Framework;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Blocks.Behaviours
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

    public class LiquidBlockBehaviour : IBlockBehaviour
    {
        public void Update100(GameTime gameTime, World world, Vector3i blockPosition)
        {
            BlockType right = world.GetBlock(blockPosition.X + 1, blockPosition.Y, blockPosition.Z);
            BlockType left = world.GetBlock(blockPosition.X - 1, blockPosition.Y, blockPosition.Z);
            BlockType up = world.GetBlock(blockPosition.X, blockPosition.Y + 1, blockPosition.Z);
            BlockType bottom = world.GetBlock(blockPosition.X, blockPosition.Y - 1, blockPosition.Z);
            BlockType forward = world.GetBlock(blockPosition.X, blockPosition.Y, blockPosition.Z + 1);
            BlockType backward = world.GetBlock(blockPosition.X, blockPosition.Y, blockPosition.Z - 1);

            // If block is rounded by solid blocks we do nothing
            if (right.IsSolid && left.IsSolid && bottom.IsSolid && forward.IsSolid && backward.IsSolid)
            {
                return;
            }

            if (bottom.IsAirType())
            {
                
            }
        }
    }
}