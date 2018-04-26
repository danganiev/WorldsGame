using Lidgren.Network;

using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public class PlayerBlockTypeChangeMessage : IGameMessage, IClientSendableMessage
    {
        public int BlockTypeKey { get; set; }

        public PlayerBlockTypeChangeMessage(int blockTypeKey)
        {
            BlockTypeKey = blockTypeKey;
        }

        public PlayerBlockTypeChangeMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerBlockTypeChange; }
        }

        public void Decode(NetIncomingMessage im)
        {
            BlockTypeKey = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(BlockTypeKey);
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this, deliveryMethod: NetDeliveryMethod.ReliableSequenced);
        }
    }
}