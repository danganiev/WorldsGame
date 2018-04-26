using Lidgren.Network;

namespace WorldsGame.Network.Manager
{
    internal class EmptyNetworkManager : INetworkManager
    {
        public void Dispose()
        {
        }

        public void Disconnect()
        {
        }

        public NetIncomingMessage ReadMessage()
        {
            return null;
        }

        public void Recycle(NetIncomingMessage im)
        {
        }

        public NetOutgoingMessage CreateMessage()
        {
            return null;
        }
    }
}