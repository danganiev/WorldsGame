using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using WorldsGame.Models;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Terrain.Blocks.Types;

using WorldsLib;

namespace WorldsGame.Terrain
{
    internal class WorldObjectOperator : IDisposable
    {
        private World World { get; set; }

        private Dictionary<Vector3i, Chunk> ChunksInvolved { get; set; }

        internal WorldObjectOperator(World world)
        {
            World = world;
            ChunksInvolved = new Dictionary<Vector3i, Chunk>();
        }

        internal void SetObject(CompiledGameObject gameObject, Vector3i position, Vector2 direction, int priority)
        {
            foreach (KeyValuePair<Vector3i, string> keyValuePair in gameObject.Blocks)
            {
                var newPosition = new Vector3i((int)(position.X + keyValuePair.Key.X * direction.X), keyValuePair.Key.Y + position.Y, (int)(position.Z + keyValuePair.Key.Z * direction.Y));

                Vector3i chunkIndex = Chunk.GetChunkIndex(newPosition);
                Chunk chunk;

                if (!ChunksInvolved.ContainsKey(chunkIndex))
                {
                    chunk = World.GetChunkByIndex(chunkIndex);
                    ChunksInvolved.Add(chunkIndex, chunk);
                }
                else
                {
                    chunk = ChunksInvolved[chunkIndex];
                }

                BlockType blockType = BlockTypeHelper.Get(keyValuePair.Value);

                Vector3i localPosition = Chunk.GetLocalPosition(newPosition);

                World.Generator.AddPrecomputedBlock(newPosition, blockType, priority);

                if (chunk != null)
                {
                    chunk.SetBlock(localPosition, blockType);
                }
            }

            foreach (KeyValuePair<Vector3i, Chunk> chunky in ChunksInvolved)
            {
                if (chunky.Value != null)
                {
                    chunky.Value.DoDraw();
                }
            }

            ChunksInvolved.Clear();
        }

        public void Dispose()
        {
            ChunksInvolved.Clear();
        }
    }
}