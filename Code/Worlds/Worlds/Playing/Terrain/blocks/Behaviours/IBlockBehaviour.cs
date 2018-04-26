using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Terrain;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Blocks.Behaviours
{
    public interface IBlockBehaviour
    {
        void Update100(GameTime gameTime, World world, Vector3i blockPosition);
    }
}
