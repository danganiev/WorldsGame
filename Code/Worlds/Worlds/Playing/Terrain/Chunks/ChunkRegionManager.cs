using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Terrain.Chunks;
using WorldsGame.Saving.World;
using WorldsGame.Terrain;
using WorldsGame.Utils.Types;
using WorldsLib;

namespace WorldsGame.Models
{
    internal class ChunkRegionManager : Dictionary3<ChunkRegion>
    {
        private readonly CompiledGameBundle _bundle;
        private readonly World _world;

        internal ChunkRegionManager(World world, CompiledGameBundle bundle)
        {
            _world = world;
            _bundle = bundle;
        }

        internal void Remove(Vector3i index)
        {
            ChunkRegion removed;
            TryRemove(KeyFromCoords(index.X, index.Y, index.Z), out removed);
        }

        internal void SaveEverything(ICollection<Chunk> chunks = null)
        {
            if (chunks == null)
            {
                chunks = _world.Chunks.Values;
            }

            foreach (Chunk chunk in chunks)
            {
                if (chunk.State >= ChunkState.Generated)
                {
                    ChunkRegion region = GetByChunkIndex(chunk.Index);
                    region.FillData(chunk);
                    region.ClearEntityData(chunk.Index);
                }
            }

            // foreach chunk get entity then save in chunk region
            // we cant loop through entities cause that'll lead to duplicate or missed data in saves

            // for starters we should loop through all active entities, and organize them
            // into dictionary dict<regionIndex, list<entity>>
            UpdateEntityList();

            foreach (ChunkRegion chunkRegion in Values)
            {
                chunkRegion.Save();
            }
        }

        private void UpdateEntityList()
        {
            lock (_world.EntityWorld.EntityManager.lock_)
            {
                foreach (KeyValuePair<int, Entity> activeEntity in _world.EntityWorld.EntityManager.ActiveEntities)
                {
                    if (activeEntity.Value.TemplateType == null)
                    {
                        continue;
                    }

                    var positionComponent = activeEntity.Value.GetComponent<PositionComponent>();
                    if (positionComponent != null)
                    {
                        var chunkIndex = Chunk.GetChunkIndex(new Vector3i(positionComponent.Position));

                        ChunkRegion region = GetByChunkIndex(chunkIndex);
                        region.UpdateEntityData(activeEntity.Value, positionComponent.Position, chunkIndex);
                    }
                }
            }
        }

        internal ChunkRegion GetByChunkIndex(Vector3i chunkIndex, bool isNetwork = false)
        {
            Vector3i regionPosition = ChunkRegion.GetRegionPosition(chunkIndex);

            ChunkRegion region = this[regionPosition.X, regionPosition.Y, regionPosition.Z];

            if (region == null)
            {
                region = isNetwork ? CreateRegion(regionPosition) : LoadOrCreateRegion(regionPosition);
                this[regionPosition.X, regionPosition.Y, regionPosition.Z] = region;
            }

            return region;
        }

        private ChunkRegion CreateRegion(Vector3i regionPosition)
        {
            return new ChunkRegion(_world, regionPosition, _bundle);
        }

        private ChunkRegion LoadOrCreateRegion(Vector3i regionPosition)
        {
            ChunkRegion region;

            var saverHelper = ChunkRegionSave.SaverHelper(_bundle.FullName);
            string regionPositionName = ChunkRegionSave.FileNameByIndex(regionPosition);

            if (saverHelper.Exists(regionPositionName))
            {
                try
                {
                    ChunkRegionSave regionSave = saverHelper.Load(regionPositionName, hasExtension: true);
                    region = regionSave.ToChunkRegion(_world, _bundle);
                }
                catch (Exception ex)
                {
                    if (ex is NullReferenceException || ex is SerializationException)
                    {
                        //Problems with loading
                        region = CreateRegion(regionPosition);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                region = CreateRegion(regionPosition);
            }

            return region;
        }
    }
}