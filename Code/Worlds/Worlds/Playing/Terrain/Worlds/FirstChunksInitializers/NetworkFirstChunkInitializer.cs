using System;
using System.Collections.Generic;
using WorldsGame.Models;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Worlds
{
    internal class NetworkFirstChunkInitializer : IFirstChunksInitializer
    {
        private readonly World _world;
        private int _chunkCount;
        private int _acceptedChunkCount;
        private bool _isFinished;
        private string _currentLoadMessage;
        private Vector3i _origin;
        private readonly ClientNetworkManager _networkManager;
        private readonly ClientMessageProcessor _messageProcessor;

        internal NetworkFirstChunkInitializer(World world, Vector3i origin, ClientMessageProcessor messageProcessor)
        {
            _world = world;
            _origin = origin;
            _networkManager = messageProcessor.NetworkManager;
            _messageProcessor = messageProcessor;
        }

        public void Initialize()
        {
            _acceptedChunkCount = 0;
            _chunkCount = 0;
            _isFinished = false;

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

            _messageProcessor.OnChunkResponse += OnChunkResponse;

            Messenger.Invoke("LoadingMessageChange", "Loading initial chunks");
            _currentLoadMessage = "Requesting initial chunks: {0} of {1}";
            VisitChunks(DoInitialRequest, World.CHUNK_INITIAL_GENERATION_RANGE);
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

        private void DoInitialRequest(Vector3i chunkIndex)
        {
            if (_world.Chunks.Get(chunkIndex) == null)
            {
//                var chunk = new Chunk(_world, chunkIndex);
//                _world.Chunks[chunkIndex.X, chunkIndex.Y, chunkIndex.Z] = chunk;
//
//                chunk.DoGenerate();
                var chunkRequestMessage = new ChunkRequestMessage(chunkIndex.X, chunkIndex.Y, chunkIndex.Z);
                chunkRequestMessage.Send(_networkManager);
            }            
        }

        private void OnChunkResponse(ChunkResponseMessage message)
        {
            var index = new Vector3i(message.X, message.Y, message.Z);
            var chunk = new Chunk(_world, index);
            chunk.SetBlocksFromKeys(message.Blocks);
            
            chunk.DoDraw();
            //Redrawing right after the start to avoid boundary problems
            chunk.SetGeneratedState();

            _world.Chunks.AddOrUpdate(index, chunk);

            _acceptedChunkCount++;

            Messenger.Invoke("LoadingMessageChange", string.Format("Accepted {0} of {1} chunks", _acceptedChunkCount, _chunkCount));

            if (_acceptedChunkCount == _chunkCount)
            {
                Finish();
            }
        }

        private void Finish()
        {
            _messageProcessor.OnChunkResponse -= OnChunkResponse;
            _isFinished = true;
        }

//        private void DoBuild(Vector3i target)
//        {
//            Chunk chunk = _world.Chunks.Get(target);
//            chunk.DoDraw();
//
//            chunk.SetGeneratedState();
//        }
    }
}