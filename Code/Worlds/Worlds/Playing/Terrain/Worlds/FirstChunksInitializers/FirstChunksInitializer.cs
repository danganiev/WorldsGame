using System;
using System.Collections.Generic;

using WorldsGame.Models;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;

using WorldsLib;

namespace WorldsGame.Playing.Terrain.Worlds
{
    // Does the required work to initalize a world.

    internal class FirstChunksInitializer : IFirstChunksInitializer
    {
        private readonly World _world;
        private int _chunkCount;
        private string _currentLoadMessage;
        private Vector3i _origin;

        internal FirstChunksInitializer(World world, Vector3i origin)
        {
            _world = world;
            _origin = origin;
        }

        public void Initialize()
        {
            for (int x = _origin.X - World.CHUNK_INITIAL_GENERATION_RANGE; x <= _origin.X + World.CHUNK_INITIAL_GENERATION_RANGE; x++)
            {
                for (int z = _origin.Z - World.CHUNK_INITIAL_GENERATION_RANGE; z <= _origin.Z + World.CHUNK_INITIAL_GENERATION_RANGE; z++)
                {
                    for (int y = _origin.Y - World.CHUNK_GENERATION_RANGE_Y; y <= _origin.Y + World.CHUNK_GENERATION_RANGE_Y; y++)
                    {
                        _chunkCount++;
                    }
                }
            }

            Messenger.Invoke("LoadingMessageChange", "Generating initial chunks");
            _currentLoadMessage = "Generating initial chunks: {0} of {1}";
            VisitChunks(DoInitialGenerate, World.CHUNK_INITIAL_GENERATION_RANGE);

            PostGenerate();

            Messenger.Invoke("LoadingMessageChange", "Building initial chunks");
            _currentLoadMessage = "Building initial chunks: {0} of {1}";
            VisitChunks(DoBuild, World.CHUNK_INITIAL_GENERATION_RANGE);
        }

        private void PostGenerate()
        {
            if (_world.WorldType == WorldType.ObjectCreationWorld)
            {
                if (_world.GameObject != null)
                {
                    foreach (KeyValuePair<Vector3i, string> keyValuePair in _world.GameObject.Blocks)
                    {
                        _world.SetBlock(keyValuePair.Key, BlockTypeHelper.AvailableBlockTypes[keyValuePair.Value]);
                    }
                }
            }
        }

        private void VisitChunks(Action<Vector3i> visitor, byte radius)
        {
            int chunksVisited = 0;

            for (int x = _origin.X - radius; x <= _origin.X + radius; x++)
            {
                for (int z = _origin.Z - radius; z <= _origin.Z + radius; z++)
                {
                    for (int y = _origin.Y - World.CHUNK_GENERATION_RANGE_Y; y <= _origin.Y + World.CHUNK_GENERATION_RANGE_Y; y++)
                    {
                        chunksVisited++;
                        Messenger.Invoke("LoadingMessageChange", string.Format(_currentLoadMessage, chunksVisited, _chunkCount));
                        visitor(new Vector3i(x, y, z));
                    }
                }
            }
        }

        private void DoInitialGenerate(Vector3i chunkIndex)
        {
            if (_world.Chunks.Get(chunkIndex) == null)
            {
                var chunk = new Chunk(_world, chunkIndex);
                _world.Chunks[chunkIndex.X, chunkIndex.Y, chunkIndex.Z] = chunk;

                chunk.DoGenerate();
            }
        }

        private void DoBuild(Vector3i target)
        {
            Chunk chunk = _world.Chunks.Get(target);
            chunk.DoDraw();

            //Redrawing right after the start to avoid boundary problems
            chunk.SetGeneratedState();
        }
    }
}