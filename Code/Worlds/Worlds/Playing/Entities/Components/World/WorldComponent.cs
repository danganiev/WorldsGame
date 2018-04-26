using Microsoft.Xna.Framework;
using WorldsGame.Terrain;

namespace WorldsGame.Playing.Entities
{
    internal class WorldComponent : IEntityComponent
    {
        internal World World { get; set; }

        internal WorldComponent(World world)
        {
            World = world;
        }

        public void Dispose()
        {
        }
    }
}