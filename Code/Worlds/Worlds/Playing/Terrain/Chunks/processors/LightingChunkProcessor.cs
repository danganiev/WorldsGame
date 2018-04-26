using System;
using System.Collections.Generic;
using WorldsGame.Models;
using WorldsGame.Playing.Terrain.Light;
using WorldsGame.Terrain.Blocks.Types;
using WorldsLib;

namespace WorldsGame.View.Processors
{
    internal class LightingChunkProcessor : IChunkProcessor
    {
        private readonly object _locker = new object();

        private Chunk Chunkie { get; set; }

        public void ProcessChunk(Chunk chunk)
        {
            lock (chunk)
            {
                if (chunk.IsDisposing)
                {
                    return;
                }

                lock (_locker)
                {
                    Chunkie = chunk;
                    DetectSunlight();
                    FillLighting();
                }
            }
        }

        private void FillLighting()
        {
            byte light;

            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                int xOffset = x * Chunk.FLATTEN_OFFSET;
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = xOffset + z * Chunk.SIZE.Y; // we don't want this x-z value to be calculated in eatch y-loop!
                    for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    {
                    }
                }
            }
        }

        private void DetectSunlight()
        {
            if (Chunkie.Top != null)
            {
                var topNodes = new List<LightAdditionNode>();
                for (byte x = 0; x < Chunk.SIZE.X; x++)
                {
                    int y = Chunk.MAX_VECTOR.Y;
                    for (byte z = 0; z < Chunk.SIZE.Z; z++)
                    {
                        int sunlight = Chunkie.Top.GetSunlight(x, y, z);

                        Chunkie.SetSunlight(x, 0, z, sunlight == LightAdditionNode.MAX_SUNLIGHT ? sunlight : sunlight - 1);

                        if (Chunkie.GetBlock(x, 0, z).IsTransparent)
                        {
                            topNodes.Add(new LightAdditionNode(new Vector3i(x, 0, z), Chunkie));
                        }
                    }
                }
            }
            else if (Chunkie.Position.Y < Chunkie.World.SunlitHeight)
            {
                return;
            }

            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                int xOffset = x * Chunk.FLATTEN_OFFSET;
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = xOffset + z * Chunk.SIZE.Y;
                    for (byte y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        Chunkie.SetSunlight(x, 0, z, LightAdditionNode.MAX_SUNLIGHT);
                    }
                }
            }
        }
    }
}