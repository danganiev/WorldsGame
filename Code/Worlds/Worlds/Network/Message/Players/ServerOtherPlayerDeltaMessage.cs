using Lidgren.Network;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Players;

namespace WorldsGame.Network.Message
{
    internal class ServerOtherPlayerDeltaMessage : IGameMessage, ITimeStampedMessage, IServerSendableMessage
    {
        public byte Slot { get; set; }

        public ServerNetworkPlayerDescription PlayerDescription { get; set; }

        public double Timestamp { get; set; }

        public GameMessageType MessageType { get { return GameMessageType.ServerOtherPlayerDelta; } }

        public ServerOtherPlayerDeltaMessage(ServerNetworkPlayerDescription description)
        {
            PlayerDescription = description;
        }

        public ServerOtherPlayerDeltaMessage(NetIncomingMessage im)
        {        
            Decode(im);
        }

        public void Decode(NetIncomingMessage im)
        {
            Slot = im.ReadByte();
            PlayerDescription = new ServerNetworkPlayerDescription
            {
                Position = new Vector3(im.ReadFloat(), im.ReadFloat(), im.ReadFloat()),
                YVelocity = im.ReadFloat(),

                IsMovingForward = im.ReadBoolean(),
                IsMovingBackward = im.ReadBoolean(),
                IsStrafingLeft = im.ReadBoolean(),
                IsStrafingRight = im.ReadBoolean(),

                LeftRightRotation = im.ReadFloat(),
                UpDownRotation = im.ReadFloat(),
            };
            Timestamp = im.ReadTime(false);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Slot);

            om.Write(PlayerDescription.Position.X);
            om.Write(PlayerDescription.Position.Y);
            om.Write(PlayerDescription.Position.Z);

            om.Write(PlayerDescription.YVelocity);

            om.Write(PlayerDescription.IsMovingForward);
            om.Write(PlayerDescription.IsMovingBackward);
            om.Write(PlayerDescription.IsStrafingLeft);
            om.Write(PlayerDescription.IsStrafingRight);

            om.Write(PlayerDescription.LeftRightRotation);
            om.Write(PlayerDescription.UpDownRotation);

            Timestamp = NetTime.Now;
            om.WriteTime(Timestamp, false);
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