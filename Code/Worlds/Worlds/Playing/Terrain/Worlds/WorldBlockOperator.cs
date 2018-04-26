using System;

using Microsoft.Xna.Framework;

using WorldsGame.Models;
using WorldsGame.Terrain.Blocks.Types;

using WorldsLib;

namespace WorldsGame.Terrain
{
    internal class WorldBlockOperator
    {
        private readonly World _world;

        internal WorldBlockOperator(World world)
        {
            _world = world;
        }

        // BUG: This function is DEFINITELY working wrong. Or named wrong.
        private bool IsChunkInView(int x, int y, int z)
        {
            int lx = x % Chunk.SIZE.X;
            int ly = y % Chunk.SIZE.Y;
            int lz = z % Chunk.SIZE.Z;

            return Math.Abs(lx) < Chunk.SIZE.X && Math.Abs(ly) < Chunk.SIZE.Y && Math.Abs(lz) < Chunk.SIZE.Z;
        }

        internal BlockType GetBlock(Vector3 position)
        {
            int newX, newY, newZ;
            newX = (int)Math.Floor(position.X);
            newY = (int)Math.Floor(position.Y);
            newZ = (int)Math.Floor(position.Z);

            return GetBlock(newX, newY, newZ);
        }

        internal static Vector3i GetBlockPosition(Vector3 position)
        {
            int newX, newY, newZ;

            newX = (int)Math.Floor(position.X);
            newY = (int)Math.Floor(position.Y);
            newZ = (int)Math.Floor(position.Z);

            return new Vector3i(newX, newY, newZ);
        }

        internal BlockType GetBlock(int x, int y, int z)
        {
            if (IsChunkInView(x, y, z))
            {
                int xMod = x % Chunk.SIZE.X;
                int yMod = y % Chunk.SIZE.Y;
                int zMod = z % Chunk.SIZE.Z;

                Chunk chunk = _world.Chunks.GetByPosition(x, y, z);

                if (chunk != null)
                {
                    var localX = x >= 0 ? xMod : xMod != 0 ? Chunk.SIZE.X + xMod : 0;
                    var localY = y >= 0 ? yMod : yMod != 0 ? Chunk.SIZE.Y + yMod : 0;
                    var localZ = z >= 0 ? zMod : zMod != 0 ? Chunk.SIZE.Z + zMod : 0;

                    return chunk.GetBlock(localX, localY, localZ);
                }

                return BlockTypeHelper.WaitBlockType;
            }

            return BlockTypeHelper.WaitBlockType;
        }

        internal int GetSunlight(int x, int y, int z)
        {
            if (IsChunkInView(x, y, z))
            {
                int xMod = x % Chunk.SIZE.X;
                int yMod = y % Chunk.SIZE.Y;
                int zMod = z % Chunk.SIZE.Z;

                Chunk chunk = _world.Chunks.GetByPosition(x, y, z);

                if (chunk != null)
                {
                    var localX = x >= 0 ? xMod : xMod != 0 ? Chunk.SIZE.X + xMod : 0;
                    var localY = y >= 0 ? yMod : yMod != 0 ? Chunk.SIZE.Y + yMod : 0;
                    var localZ = z >= 0 ? zMod : zMod != 0 ? Chunk.SIZE.Z + zMod : 0;

                    return chunk.GetSunlight(localX, localY, localZ);
                }
            }

            return -1;
        }

        internal void SetSunlight(int x, int y, int z, int sunlight)
        {
            if (IsChunkInView(x, y, z))
            {
                int xMod = x % Chunk.SIZE.X;
                int yMod = y % Chunk.SIZE.Y;
                int zMod = z % Chunk.SIZE.Z;

                Chunk chunk = _world.Chunks.GetByPosition(x, y, z);

                if (chunk != null)
                {
                    var localX = x >= 0 ? xMod : xMod != 0 ? Chunk.SIZE.X + xMod : 0;
                    var localY = y >= 0 ? yMod : yMod != 0 ? Chunk.SIZE.Y + yMod : 0;
                    var localZ = z >= 0 ? zMod : zMod != 0 ? Chunk.SIZE.Z + zMod : 0;

                    chunk.SetSunlight(localX, localY, localZ, sunlight);
                }
            }
        }

        internal void GetLightData(int x, int y, int z, out int luminosity, out int r, out int g, out int b)
        {
            luminosity = 0;
            r = 0;
            g = 0;
            b = 0;

            if (IsChunkInView(x, y, z))
            {
                int xMod = x % Chunk.SIZE.X;
                int yMod = y % Chunk.SIZE.Y;
                int zMod = z % Chunk.SIZE.Z;

                Chunk chunk = _world.Chunks.GetByPosition(x, y, z);

                if (chunk != null)
                {
                    var localX = x >= 0 ? xMod : xMod != 0 ? Chunk.SIZE.X + xMod : 0;
                    var localY = y >= 0 ? yMod : yMod != 0 ? Chunk.SIZE.Y + yMod : 0;
                    var localZ = z >= 0 ? zMod : zMod != 0 ? Chunk.SIZE.Z + zMod : 0;

                    chunk.GetLightData(localX, localY, localZ, out luminosity, out r, out g, out b);
                }
            }
        }

        internal void SetLightData(int x, int y, int z, int luminosity, int r, int g, int b)
        {
            if (IsChunkInView(x, y, z))
            {
                int xMod = x % Chunk.SIZE.X;
                int yMod = y % Chunk.SIZE.Y;
                int zMod = z % Chunk.SIZE.Z;

                Chunk chunk = _world.Chunks.GetByPosition(x, y, z);

                if (chunk != null)
                {
                    var localX = x >= 0 ? xMod : xMod != 0 ? Chunk.SIZE.X + xMod : 0;
                    var localY = y >= 0 ? yMod : yMod != 0 ? Chunk.SIZE.Y + yMod : 0;
                    var localZ = z >= 0 ? zMod : zMod != 0 ? Chunk.SIZE.Z + zMod : 0;

                    chunk.SetLightData(localX, localY, localZ, luminosity, r, g, b);
                }
            }
        }

        // TODO: There is a need in SetBlockBulk
        internal void SetBlockAndRedraw(int x, int y, int z, BlockType newBlock)
        {
            if (IsChunkInView(x, y, z))
            {
                Vector3i localChunkPosition = Chunk.GetLocalPosition(new Vector3i(x, y, z));

                var localX = localChunkPosition.X;
                var localY = localChunkPosition.Y;
                var localZ = localChunkPosition.Z;

                Chunk chunk = _world.Chunks.GetByPosition(x, y, z);

                // chunk.setBlock is also called by terrain generators for Y loops min max optimisation
                try
                {
                    chunk.SetBlock(localX, localY, localZ, newBlock);
                    chunk.RedrawWithNeighbours(localX, localY, localZ);
                }
                catch (NullReferenceException)
                {
                }
            }
        }
    }
}