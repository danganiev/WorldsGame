using WorldsGame.Playing.Renderers.ContentLoaders;

namespace WorldsGame.Playing.Entities
{
    // Constant component
    internal class ContentLoaderComponent : IEntityComponent
    {
        internal WorldsContentLoader ContentLoader { get; set; }

        internal ContentLoaderComponent(WorldsContentLoader loader)
        {
            ContentLoader = loader;
        }

        public void Dispose()
        {
            ContentLoader = null;
        }
    }
}