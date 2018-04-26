using WorldsGame.Playing.DataClasses;

namespace WorldsGame.Models.Terrain
{
    internal class EmptyChunkGenerator : ChunkGenerator
    {
        public EmptyChunkGenerator(CompiledGameBundle bundle)
            : base(bundle)
        {
        }

        internal override void Generate(Chunk chunk)
        {
        }

        internal override void ClearAfterGenerating()
        {
        }

        public override void Dispose()
        {
        }
    }
}