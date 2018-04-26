using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public class FileStreamDataMessage : IGameMessage, IServerSendableMessage
    {
        public FileStreamDataMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public byte[] ByteBuffer { get; set; }

        public int SendBytes { get; set; }

        public FileStreamDataMessage(byte[] byteBuffer, int sendBytes)
        {
            ByteBuffer = byteBuffer;
            SendBytes = sendBytes;
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.FileStreamData; }
        }

        public void Decode(NetIncomingMessage im)
        {
            ByteBuffer = im.ReadBytes(im.LengthBytes - 1);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(ByteBuffer, 0, SendBytes);
        }

        public void Send(NetConnection connection)
        {
            NetOutgoingMessage om = connection.Peer.CreateMessage(SendBytes + 1);

            om.Write((byte)MessageType);
            Encode(om);

            connection.SendMessage(om, NetDeliveryMethod.ReliableOrdered, 1);
        }
    }
}