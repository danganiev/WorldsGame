using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Models;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Saving.World;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;

using WorldsLib;

namespace WorldsGame.Playing.Terrain.Chunks
{
    // Here we save
    internal class ChunkRegion
    {
        internal static readonly Vector3i REGION_SIZE = new Vector3i(16, 4, 16);

        internal Vector3i RegionIndex { get; set; }

        internal CompiledGameBundle Bundle { get; set; }

        private ChunkRegionSave RegionSave { get; set; }

        private readonly World _world;

        internal ChunkRegion(World world, Vector3i index, CompiledGameBundle bundle, ChunkRegionSave save = null)
        {
            _world = world;
            RegionIndex = index;
            Bundle = bundle;
            RegionSave = save ?? new ChunkRegionSave { RegionIndex = RegionIndex, CompiledGameBundleName = bundle.FullName };
        }

        internal void Save()
        {
            RegionSave.Save();
        }

        internal void FillData(Chunk chunk)
        {
            var internalChunkIndex = GetInternalChunkIndex(chunk.Index);

            RegionSave.Chunks[internalChunkIndex] = new int[Chunk.SIZE.X * Chunk.SIZE.Z * Chunk.SIZE.Y];

            for (int index = 0; index < chunk.Blocks.Blocks.Length; index++)
            {
                BlockType blockType = chunk.Blocks.Blocks[index];
                RegionSave.Chunks[internalChunkIndex][index] = Bundle.BlockNameMap[blockType.Name];
            }
        }

        // This method should be used only before save
        internal void UpdateEntityData(Entity entity, Vector3 position, Vector3i chunkIndex)
        {
            Vector3i internalIndex = GetInternalChunkIndex(chunkIndex);
            if (!RegionSave.Entities.ContainsKey(internalIndex))
            {
                RegionSave.Entities[internalIndex] = new List<Tuple<Type, string>>();
            }
            RegionSave.Entities[internalIndex].Add(
                new Tuple<Type, string>(entity.TemplateType, _world.EntityWorld.SaveableBehavioursCache[entity.TemplateType].GetJSONForSave(entity)));
        }

        public void ClearEntityData(Vector3i chunkIndex)
        {
            Vector3i internalIndex = GetInternalChunkIndex(chunkIndex);

            if (RegionSave.Entities.ContainsKey(internalIndex))
            {
                RegionSave.Entities[internalIndex].Clear();
            }
        }

        internal Chunk TryLoadChunk(Vector3i chunkIndex)
        {
            Vector3i internalIndex = GetInternalChunkIndex(chunkIndex);

            if (RegionSave.Chunks.ContainsKey(internalIndex))
            {
                var chunk = new Chunk(_world, chunkIndex);
                int[] chunkData = RegionSave.Chunks[internalIndex];

                for (int index = 0; index < chunkData.Length; index++)
                {
                    BlockType blockType = BlockTypeHelper.Get(Bundle.BlockIndexMap[chunkData[index]]);
                    chunk.RawSetBlock(index, blockType);
                }

                return chunk;
            }

            return null;
        }

        internal void TryLoadEntities(Vector3i chunkIndex)
        {
            Vector3i internalIndex = GetInternalChunkIndex(chunkIndex);

            if (RegionSave.Entities.ContainsKey(internalIndex))
            {
                for (int i = 0; i < RegionSave.Entities[internalIndex].Count; i++)
                {
                    Tuple<Type, string> tuple = RegionSave.Entities[internalIndex][i];
                    _world.EntityWorld.SaveableBehavioursCache[tuple.Item1].LoadFromJSON(_world.EntityWorld, tuple.Item2);
                }
            }
        }

        private static Vector3i GetInternalChunkIndex(Vector3i chunkIndex)
        {
            return new Vector3i(
                chunkIndex.X % REGION_SIZE.X, chunkIndex.Y % REGION_SIZE.Y, chunkIndex.Z % REGION_SIZE.Z);
        }

        internal static Vector3i GetRegionPosition(Vector3i chunkIndex)
        {
            int x = chunkIndex.X;
            int y = chunkIndex.Y;
            int z = chunkIndex.Z;

            int newX = x / REGION_SIZE.X;
            int newY = y / REGION_SIZE.Y;
            int newZ = z / REGION_SIZE.Z;

            int xMod = x % REGION_SIZE.X;
            int yMod = y % REGION_SIZE.Y;
            int zMod = z % REGION_SIZE.Z;

            if (x < 0 && xMod != 0)
                newX -= 1;
            if (y < 0 && yMod != 0)
                newY -= 1;
            if (z < 0 && zMod != 0)
                newZ -= 1;

            return new Vector3i(newX, newY, newZ);
        }
    }
}