using WorldsGame.Network.Manager;
using WorldsGame.Network.Message.Chunks;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsLib;

namespace WorldsGame.Network
{
    /// <summary>
    /// Processes all network stuff for the world
    /// </summary>
    internal class ServerWorldNetworkProcessor
    {
        private World World { get; set; }

        private ServerNetworkManager NetworkManager { get; set; }

        internal ServerWorldNetworkProcessor(World world, ServerNetworkManager networkManager)
        {
            World = world;
            NetworkManager = networkManager;
        }

        public void OnWorldBlockChange(Vector3i position, BlockType newBlock)
        {
            var message = new BlockUpdateMessage(position, newBlock.Key);

            NetworkManager.SendToAll(message);
        }
    }
}