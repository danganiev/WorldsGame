using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public class GameBundleRequestMessage : IGameMessage
    {
        public GameBundleRequestMessage()
        {
        }

        public GameBundleRequestMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.GameBundleRequest; }
        }

        public void Decode(NetIncomingMessage im)
        {
        }

        public void Encode(NetOutgoingMessage om)
        {
        }
    }
}