using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WorldsGame.Models;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Playing.Players;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Types;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Chunks
{
    internal class NetworkChunkThreadOperator : IChunkThreadOperator
    {
        private bool _running = true;

        private readonly World _world;
        //        private object _lockObject;

        private Thread _workerCheckThread;
        private Thread _workerRemoveThread;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposing;

        private ClientNetworkManager NetworkManager { get; set; }

        private ClientMessageProcessor NetworkMessageProcessor { get; set; }

        private int CurrentIteration { get; set; }

        private Player Player { get; set; }

        internal NetworkChunkThreadOperator(World world, ClientNetworkManager networkManager, ClientMessageProcessor messageProcessor)
        {
            _world = world;
            Player = _world.ClientPlayer;
            //            _lockObject = new object();
            NetworkManager = networkManager;
            NetworkMessageProcessor = messageProcessor;
            CurrentIteration = 1;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _workerCheckThread = new Thread(() => WorkerCheckThread(_cancellationTokenSource.Token))
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "WorkerCheckThread"
            };
            _workerCheckThread.Start();

            _workerRemoveThread = new Thread(() => WorkerRemoveThread(_cancellationTokenSource.Token))
            {
                Priority = ThreadPriority.Normal,
                IsBackground = true,
                Name = "WorkerRemoveThread"
            };
            _workerRemoveThread.Start();

            NetworkMessageProcessor.OnChunkResponse += OnChunkResponse;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            while (_workerCheckThread.IsAlive || _workerRemoveThread.IsAlive)
            {
                Thread.Sleep(20);
            }
        }

        public void Dispose()
        {
            if (!_isDisposing)
            {
                NetworkMessageProcessor.OnChunkResponse -= OnChunkResponse;
                _isDisposing = true;
                _running = false;

                Stop();
                _cancellationTokenSource.Dispose();
            }
        }

        private void OnChunkResponse(ChunkResponseMessage message)
        {
            AddChunk(new Vector3i(message.X, message.Y, message.Z), message.Blocks);
        }

        private void WorkerCheckThread(CancellationToken token)
        {
            while (_running)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                //NOTE: This check can be optimized, if we start this whole operator when player is ready.
                if (IsPlayerNull())
                {
                    return;
                }

                Vector3i currentPlayerChunk = Player.GetCurrentChunkIndex();
                SpiralLoop(currentPlayerChunk, token);

                CurrentIteration++;

                //Performance
                Thread.Sleep(2000);
            }
        }

        private bool IsPlayerNull()
        {
            if (Player == null)
            {
                if (_world.ClientPlayer != null)
                {
                    Player = _world.ClientPlayer;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        // Function almost the same as in single player ChunkOperator, so refactor on 3rd occurence
        private void SpiralLoop(Vector3i playerPosition, CancellationToken token)
        {
            var radius = World.ChunkGenerationRange;
            int x = 0, z = 0;
            int dx = 0;
            int dz = -1;
            int centerX = playerPosition.X;
            int centerY = playerPosition.Y;
            int centerZ = playerPosition.Z;

            for (int i = 0; i < Math.Pow(radius, 2); i++)
            {
                if ((-radius / 2 < x && x <= radius / 2) && (-radius / 2 < z && z <= radius / 2))
                {
                    for (int y = centerY - World.CHUNK_GENERATION_RANGE_Y; y <= centerY + World.CHUNK_GENERATION_RANGE_Y; y++)
                    {
                        if (Player.GetCurrentChunkIndex() != playerPosition || token.IsCancellationRequested)
                        {
                            return;
                        }
                        DrawOrRequest(centerX + x, y, centerZ + z);
                    }
                }
                if (x == z || (x < 0 && x == -z) || (x > 0 && x == 1 - z))
                {
                    int oldDx = dx;

                    dx = -dz;
                    dz = oldDx;
                }

                x = x + dx;
                z = z + dz;
            }
        }

        private void DrawOrRequest(int x, int y, int z)
        {
            //            var chunkIndex = new Vector3i(x, y, z);

            //Generate
            //            lock (_lockObject)
            //            {
            // This actually tries to get from either RAM (chunk already loaded) or from disk.
            Chunk chunk = _world.Chunks[x, y, z];
            if (chunk == null)
            {
                var chunkRequestMessage = new ChunkRequestMessage(x, y, z);
                chunkRequestMessage.Send(NetworkManager);
            }
            else if (chunk.State == ChunkState.Generated)
            {
                chunk.DoDraw();
                Thread.Sleep(5);
            }
            //            }
        }

        private void AddChunk(Vector3i index, int[] blocks)
        {
            var chunk = new Chunk(_world, index);
            chunk.SetBlocksFromKeys(blocks);
            //            lock (_lockObject)
            //            {
            _world.Chunks.AddOrUpdate(index, chunk);
            //            }
            chunk.SetGeneratedState();
            //                chunk.RedrawWithNeighbours();
        }

        //        private static bool IsChunkNotInPlayerRange(Vector3i playerIndex, Chunk chunkToGenerate)
        //        {
        //            return Math.Abs(chunkToGenerate.Index.X) < Math.Abs(playerIndex.X) - World.chunkGenerationRange ||
        //                   Math.Abs(chunkToGenerate.Index.X) > Math.Abs(playerIndex.X) + World.chunkGenerationRange ||
        //                   Math.Abs(chunkToGenerate.Index.Y) < Math.Abs(playerIndex.Y) - World.CHUNK_GENERATION_RANGE_Y ||
        //                   Math.Abs(chunkToGenerate.Index.Y) > Math.Abs(playerIndex.Y) + World.CHUNK_GENERATION_RANGE_Y ||
        //                   Math.Abs(chunkToGenerate.Index.Z) < Math.Abs(playerIndex.Z) - World.chunkGenerationRange ||
        //                   Math.Abs(chunkToGenerate.Index.Z) > Math.Abs(playerIndex.Z) + World.chunkGenerationRange;
        //        }

        //Garbage collector
        private void WorkerRemoveThread(CancellationToken token)
        {
            var coordinates = new List<ulong>();
            while (_running)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (IsPlayerNull())
                {
                    return;
                }

                Vector3i currentPlayerChunk = Player.GetCurrentChunkIndex();

                for (int ix = currentPlayerChunk.X - World.ChunkGenerationRange; ix < currentPlayerChunk.X + World.ChunkGenerationRange + 1; ix++)
                {
                    for (int iz = currentPlayerChunk.Z - World.ChunkGenerationRange; iz < currentPlayerChunk.Z + World.ChunkGenerationRange + 1; iz++)
                    {
                        for (int iy = currentPlayerChunk.Y - World.CHUNK_GENERATION_RANGE_Y; iy < currentPlayerChunk.Y + World.CHUNK_GENERATION_RANGE_Y + 1; iy++)
                        {
                            coordinates.Add(_world.Chunks.KeyFromCoords(ix, iy, iz));
                        }
                    }
                }

                RemoveChunks(coordinates, token);
                RemoveChunkRegions(token);
                coordinates.Clear();

                Thread.Sleep(1000);
            }
        }

        private void RemoveChunks(List<ulong> coordinates, CancellationToken token)
        {
            var chunksToRemove = new List<Chunk>();

            // DO NOT UNDER ANY CIRCUMSTANCES TRANSFER THIS TO LINQ, IT SEEMS TO SUCK BADLY WITH THREADING
            foreach (KeyValuePair<ulong, Chunk> keyValuePair in _world.Chunks)
            {
                if (!coordinates.Contains(keyValuePair.Key))
                {
                    chunksToRemove.Add(keyValuePair.Value);
                }
            }

            foreach (var chunk in chunksToRemove)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (chunk != null)
                {
                    Messenger.Invoke("ChunkDisposing", chunk.Index);
                    try
                    {
                        foreach (var representation in chunk.ChunkBlocksRepresentation)
                        {
                            representation.Value.Dispose();
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                    _world.Chunks.Remove(chunk.Index);
                }
            }
        }

        private void RemoveChunkRegions(CancellationToken token)
        {
            List<Vector3i> activeRegionIndices =
                (from chunk in _world.Chunks
                 select ChunkRegion.GetRegionPosition(chunk.Value.Index)).Distinct().ToList();

            var additionalRegionIndices = new List<Vector3i>();
            foreach (Vector3i activeRegionIndex in activeRegionIndices)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            additionalRegionIndices.Add(activeRegionIndex + new Vector3i(x, y, z));
                        }
                    }
                }
            }
            activeRegionIndices.AddRange(additionalRegionIndices);

            foreach (KeyValuePair<ulong, ChunkRegion> chunkRegion in _world.ChunkRegions)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (!activeRegionIndices.Contains(chunkRegion.Value.RegionIndex))
                {
                    _world.ChunkRegions.Remove(chunkRegion.Value.RegionIndex);
                }
            }
        }
    }
}