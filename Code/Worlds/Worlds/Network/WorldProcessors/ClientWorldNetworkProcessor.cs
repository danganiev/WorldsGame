using WorldsGame.Network.Manager;
using WorldsGame.Network.Message.Chunks;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;

namespace WorldsGame.Network
{
    /// <summary>
    /// Processes all client network stuff for the world
    /// </summary>
    internal class ClientWorldNetworkProcessor
    {
        private World World { get; set; }

        private ClientNetworkManager NetworkManager { get; set; }

        internal ClientWorldNetworkProcessor(World world, ClientNetworkManager networkManager)
        {
            World = world;
            NetworkManager = networkManager;
        }

        internal void OnBlockUpdate(BlockUpdateMessage message)
        {
            World.SetBlock(message.Position, BlockTypeHelper.Get(message.BlockTypeKey), suppressEvents: true);
        }
    }
}