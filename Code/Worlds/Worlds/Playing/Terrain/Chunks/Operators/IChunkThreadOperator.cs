using System;

namespace WorldsGame.Playing.Terrain.Chunks
{
    internal interface IChunkThreadOperator : IDisposable
    {
        void Start();

        void Stop();
    }
}