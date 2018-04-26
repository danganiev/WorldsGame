using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace WorldsGame.Network.Message
{
    public class ServerPlayerDeltaMessage : IGameMessage, IServerSendableMessage, ITimeStampedMessage
    {
        public Vector3 Position
        {
            get
            {
                return new Vector3(PositionX, PositionY, PositionZ);
            }
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
                PositionZ = value.Z;
            }
        }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        public int Index { get; set; }

        public double Timestamp { get; set; }

        public virtual GameMessageType MessageType { get { return GameMessageType.ServerPlayerDelta; } }

        public ServerPlayerDeltaMessage(Vector3 position)
        {
            Position = position;
        }

        public ServerPlayerDeltaMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public void Decode(NetIncomingMessage im)
        {
            PositionX = im.ReadFloat();
            PositionY = im.ReadFloat();
            PositionZ = im.ReadFloat();

            Index = im.ReadInt32();

            AdditionalDecode(im);

            Timestamp = im.ReadDouble();
        }

        protected virtual void AdditionalDecode(NetIncomingMessage im)
        {
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(PositionX);
            om.Write(PositionY);
            om.Write(PositionZ);

            om.Write(Index);

            AdditionalEncode(om);

            Timestamp = NetTime.Now;
            om.Write(Timestamp);
        }

        protected virtual void AdditionalEncode(NetOutgoingMessage om)
        {
        }

        public void Send(NetConnection connection)
        {
            NetOutgoingMessage om = connection.Peer.CreateMessage();

            om.Write((byte)MessageType);
            Encode(om);

            connection.SendMessage(om, NetDeliveryMethod.UnreliableSequenced, 0);
        }
    }
}