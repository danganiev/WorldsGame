using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using WorldsGame.Models;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Terrain;
using WorldsLib;

namespace WorldsGame.Server
{
    internal class ChunkNetworkHandler
    {
        private ServerNetworkManager NetworkManager { get; set; }
        private World World { get; set; }

        internal ChunkNetworkHandler(ServerNetworkManager networkManager, World world)
        {
            NetworkManager = networkManager;
            World = world;
        }

        public void OnChunkRequest(NetIncomingMessage im, ChunkRequestMessage message)
        {
            Chunk chunk = World.GetOrCreate(new Vector3i(message.X, message.Y, message.Z));
            var chunkResponseMessage = new ChunkResponseMessage(message.X, message.Y, message.Z, chunk.BlocksAsKeys);
            chunkResponseMessage.Send(im.SenderConnection);
        }
    }
}
