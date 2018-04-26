using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public class PlayerDisconnectMessage : IGameMessage, IServerSendableMessage
    {
        public byte Slot { get; set; }

        public PlayerDisconnectMessage(byte slot)
        {
            Slot = slot;
        }

        public PlayerDisconnectMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerDisconnect; }
        }

        public void Decode(NetIncomingMessage im)
        {
            Slot = im.ReadByte();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Slot);
        }

        public void Send(NetConnection connection)
        {
            NetOutgoingMessage om = connection.Peer.CreateMessage();

            om.Write((byte)MessageType);
            Encode(om);

            connection.SendMessage(om, NetDeliveryMethod.ReliableUnordered, 0);
        }
    }
}