using System;
using System.Net;

using Lidgren.Network;

using WorldsGame.Network.Message;

namespace WorldsGame.Network.Manager
{
    public class ClientNetworkManager : INetworkManager
    {
        private bool IsDisposed { get; set; }

        private NetClient NetClient { get; set; }

        internal string URL { get; private set; }

        internal string Port { get; private set; }

        internal bool IsDisconnected { get; set; }

        internal event Action OnDisconnect = () => { };

        public NetConnection Connection { get { return NetClient.ServerConnection; } }

        public ClientNetworkManager(string url, string port = "4815")
        {
            URL = url;
            Port = port;

            var config = new NetPeerConfiguration("Worlds")
            {
//                SimulatedMinimumLatency = 0.2f,
//                SimulatedLoss = 0.1f
            };

            config.EnableMessageType(NetIncomingMessageType.WarningMessage);
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
            config.EnableMessageType(NetIncomingMessageType.Error);
            config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            NetClient = new NetClient(config);
        }

        internal bool IsURLOK()
        {
            IPAddress address = NetUtility.Resolve(URL);

            return address != null;
        }

        public void Connect(ServerAuthorizationRequestMessage message)
        {
            if (!IsURLOK())
            {
                return;
            }

            IPAddress address = NetUtility.Resolve(URL);

            NetClient.Start();

            NetOutgoingMessage hailMessage = PrepareMessage(message);
            NetClient.Connect(new IPEndPoint(address, Convert.ToInt32(Port)), hailMessage);
        }

        public NetOutgoingMessage CreateMessage()
        {
            return NetClient.CreateMessage();
        }

        public void Disconnect()
        {
            if (NetClient != null)
            {
                NetClient.Disconnect("Bye");
                NetClient.Shutdown("Bye");
                NetClient = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public NetIncomingMessage ReadMessage()
        {
            if (NetClient != null)
            {
                return NetClient.ReadMessage();
            }

            return null;
        }

        public void Recycle(NetIncomingMessage im)
        {
            NetClient.Recycle(im);
        }

        public void SendMessage(IGameMessage gameMessage, int sequenceChannel = 0, NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableUnordered)
        {
            NetOutgoingMessage om = PrepareMessage(gameMessage);

            NetClient.SendMessage(om, deliveryMethod, sequenceChannel);

            if (NetClient.ConnectionStatus == NetConnectionStatus.Disconnected)
            {
                IsDisconnected = true;
                OnDisconnect();
            }
        }

        private NetOutgoingMessage PrepareMessage(IGameMessage gameMessage)
        {
            NetOutgoingMessage om = NetClient.CreateMessage();
            om.Write((byte)gameMessage.MessageType);
            gameMessage.Encode(om);

            return om;
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                IsDisposed = true;
            }
        }
    }
}