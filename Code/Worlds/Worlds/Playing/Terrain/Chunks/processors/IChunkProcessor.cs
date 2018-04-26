using WorldsGame.Models;

namespace WorldsGame.View
{
    internal interface IChunkProcessor
    {
        void ProcessChunk(Chunk chunk);
    }
}