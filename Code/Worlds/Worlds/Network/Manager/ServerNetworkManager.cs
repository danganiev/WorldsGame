using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using WorldsGame.Network.Message;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Network.Manager
{
    internal class ServerNetworkManager : INetworkManager
    {
        private bool isDisposed;

        private NetServer NetServer { get; set; }

        private int Port { get; set; }

        internal ServerNetworkManager(int port)
        {
            Port = port;
        }

        public void Start()
        {
            var config = new NetPeerConfiguration("Worlds")
            {
                Port = Port,
                //                SimulatedMinimumLatency = 0.2f,
                //                SimulatedLoss = 0.1f
            };
            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.EnableMessageType(NetIncomingMessageType.Error);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            NetServer = new NetServer(config);
            NetServer.Start();
        }

        public NetOutgoingMessage CreateMessage()
        {
            return NetServer.CreateMessage();
        }

        public void Disconnect()
        {
            NetServer.Shutdown("Bye");
        }

        public NetIncomingMessage ReadMessage()
        {
            if (NetServer != null)
            {
                return NetServer.ReadMessage();
            }

            return null;
        }

        public void Recycle(NetIncomingMessage im)
        {
            NetServer.Recycle(im);
        }

        public void SendToAll(IGameMessage message, NetConnection except = null, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.UnreliableSequenced)
        {
            NetOutgoingMessage om = NetServer.CreateMessage();
            om.Write((byte)message.MessageType);
            message.Encode(om);

            NetServer.SendToAll(om, except, deliveryMethod, 0);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                Disconnect();

                isDisposed = true;
            }
        }
    }
}