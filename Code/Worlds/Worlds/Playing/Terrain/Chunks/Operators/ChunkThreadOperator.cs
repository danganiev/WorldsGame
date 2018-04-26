using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using WorldsGame.Models;
using WorldsGame.Playing.Players;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Chunks
{
    internal class ChunkThreadOperator : IChunkThreadOperator
    {
        private bool _running = true;

        private readonly World _world;
        private readonly Player _player;
        private object _lockObject;

        private Thread _workerCheckThread;
        private Thread _workerRemoveThread;
        private Thread _workerSaverThread;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposing;

        internal ChunkThreadOperator(World world)
        {
            _world = world;
            _player = _world.ClientPlayer;
            _lockObject = new object();
        }

        public void Start()
        {
            Initialize();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            while (_workerCheckThread.IsAlive || _workerRemoveThread.IsAlive/* || _workerSaverThread.IsAlive*/)
            {
                Thread.Sleep(20);
            }
        }

        public void Dispose()
        {
            if (!_isDisposing)
            {
                _isDisposing = true;
                _running = false;

                Stop();
                _cancellationTokenSource.Dispose();
            }
        }

        private void Initialize()
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

            // Saverthread would be probably useful, but not as a thread, and a recurring task on server or client instead.
            //            _workerSaverThread = new Thread(() => WorkerSaverThread(_cancellationTokenSource.Token))
            //            {
            //                Priority = ThreadPriority.Normal,
            //                IsBackground = true,
            //                Name = "WorkerSaverThread"
            //            };
            //            _workerSaverThread.Start();
        }

        private void WorkerCheckThread(CancellationToken token)
        {
            while (_running)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Vector3i currentPlayerChunk = _player.GetCurrentChunkIndex();
                SpiralLoop(currentPlayerChunk, token);

                //Performance
                Thread.Sleep(10);
            }
        }

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
                        if (_player.GetCurrentChunkIndex() != playerPosition || token.IsCancellationRequested)
                        {
                            return;
                        }
                        StartGeneration(centerX + x, y, centerZ + z);
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

        private void StartGeneration(int x, int y, int z)
        {
            var chunkIndex = new Vector3i(x, y, z);

            //Generate
            lock (_lockObject)
            {
                // This actually tries to get from either RAM (chunk already loaded) or from disk.
                Chunk toBuild = _world.Chunks[x, y, z];
                if (toBuild == null)
                {
                    var chunk = new Chunk(_world, chunkIndex);
                    _world.Chunks.AddOrUpdate(chunkIndex, chunk);
                    GenerateChunk(chunk);
                }
                else if (toBuild.State == ChunkState.Generated)
                {
                    GenerateChunk(toBuild);
                }
            }
        }

        private void GenerateChunk(Chunk chunkToGenerate)
        {
            try
            {
                if (chunkToGenerate == null)
                {
                    return;
                }

                Vector3i playerIndex = _player.GetCurrentChunkIndex();

                if (IsChunkNotInPlayerRange(playerIndex, chunkToGenerate))
                {
                    return;
                }
                if (chunkToGenerate.State == ChunkState.Ready)
                {
                    return;
                }
                if (chunkToGenerate.State == ChunkState.New)
                {
                    chunkToGenerate.DoGenerate();
                }
                if (chunkToGenerate.State == ChunkState.Generated)
                {
                    if (!chunkToGenerate.AreStraightNeighbourChunksFilled())
                    {
                        if (chunkToGenerate.N == null)
                        {
                            CreateChunk(chunkToGenerate.NPos);
                        }
                        if (chunkToGenerate.E == null)
                        {
                            CreateChunk(chunkToGenerate.EPos);
                        }
                        if (chunkToGenerate.S == null)
                        {
                            CreateChunk(chunkToGenerate.SPos);
                        }
                        if (chunkToGenerate.W == null)
                        {
                            CreateChunk(chunkToGenerate.WPos);
                        }
                    }

                    if (chunkToGenerate.N.State == ChunkState.New)
                    {
                        chunkToGenerate.N.DoGenerate();
                        Thread.Sleep(5);
                    }
                    if (chunkToGenerate.E.State == ChunkState.New)
                    {
                        chunkToGenerate.E.DoGenerate();
                        Thread.Sleep(5);
                    }
                    if (chunkToGenerate.S.State == ChunkState.New)
                    {
                        chunkToGenerate.S.DoGenerate();
                        Thread.Sleep(5);
                    }
                    if (chunkToGenerate.W.State == ChunkState.New)
                    {
                        chunkToGenerate.W.DoGenerate();
                        Thread.Sleep(5);
                    }

                    chunkToGenerate.DoDraw();
                }
            }
            catch (NullReferenceException)
            {
            }
        }

        private void CreateChunk(Vector3i index)
        {
            lock (_lockObject)
            {
                var chunk = new Chunk(_world, index);
                _world.Chunks.AddOrUpdate(index, chunk);
            }
        }

        private static bool IsChunkNotInPlayerRange(Vector3i playerIndex, Chunk chunkToGenerate)
        {
            return Math.Abs(chunkToGenerate.Index.X) < Math.Abs(playerIndex.X) - World.ChunkGenerationRange ||
                   Math.Abs(chunkToGenerate.Index.X) > Math.Abs(playerIndex.X) + World.ChunkGenerationRange ||
                   Math.Abs(chunkToGenerate.Index.Y) < Math.Abs(playerIndex.Y) - World.CHUNK_GENERATION_RANGE_Y ||
                   Math.Abs(chunkToGenerate.Index.Y) > Math.Abs(playerIndex.Y) + World.CHUNK_GENERATION_RANGE_Y ||
                   Math.Abs(chunkToGenerate.Index.Z) < Math.Abs(playerIndex.Z) - World.ChunkGenerationRange ||
                   Math.Abs(chunkToGenerate.Index.Z) > Math.Abs(playerIndex.Z) + World.ChunkGenerationRange;
        }

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

                Vector3i currentPlayerChunk = _player.GetCurrentChunkIndex();

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

            //#if RELEASE
            //            catch (Exception)
            //            {
            //                if (!SystemSettings.IsRestarting && !SystemSettings.IsCleaningPlayingState)
            //                {
            //                    throw;
            //                }
            //            }
            //#endif
        }

        private void RemoveChunks(List<ulong> coordinates, CancellationToken token)
        {
            var chunksToRemove = new List<Chunk>();
            var chunksToSave = new List<Chunk>();

            // DO NOT UNDER ANY CIRCUMSTANCES TRANSFER THIS TO LINQ, IT SEEMS TO SUCK BADLY WITH THREADING
            foreach (KeyValuePair<ulong, Chunk> keyValuePair in _world.Chunks)
            {
                if (!coordinates.Contains(keyValuePair.Key))
                {
                    chunksToRemove.Add(keyValuePair.Value);

                    if (keyValuePair.Value.State >= ChunkState.Generated)
                    {
                        chunksToSave.Add(keyValuePair.Value);
                    }
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

            if (chunksToSave.Count > 0)
            {
                _world.ChunkRegions.SaveEverything(chunksToSave);
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

        //Saver
        private void WorkerSaverThread(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            while (_running)
            {
                _world.Save();
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}