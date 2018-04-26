using Lidgren.Network;

using Microsoft.Xna.Framework;

namespace WorldsGame.Network.Message
{
    // Player requested initialization, so we do it
    // Just a server spawn analogue
    public class PlayerInitializationMessage : IGameMessage, IServerSendableMessage, ITimeStampedMessage
    {
        public float UpDownRotation { get; set; }

        public float LeftRightRotation { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        // True if client player gets initialized
        public bool IsMe { get; set; }

        public byte Slot { get; set; }

        public string Username { get; set; }

        public Vector3 Position
        {
            get { return new Vector3(PositionX, PositionY, PositionZ); }
        }

        public double Timestamp { get; set; }

        public PlayerInitializationMessage(float upDownRotation, float leftRightRotation, Vector3 position, bool isMe = true)
        {
            // I don't care about compressing here, cause we don't spawn players all the time.
            UpDownRotation = upDownRotation;
            LeftRightRotation = leftRightRotation;

            PositionX = position.X;
            PositionY = position.Y;
            PositionZ = position.Z;
            IsMe = isMe;
        }

        public PlayerInitializationMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerInitialization; }
        }

        public void Decode(NetIncomingMessage im)
        {
            UpDownRotation = im.ReadFloat();
            LeftRightRotation = im.ReadFloat();

            PositionX = im.ReadFloat();
            PositionY = im.ReadFloat();
            PositionZ = im.ReadFloat();

            IsMe = im.ReadBoolean();

            Slot = im.ReadByte();

            Username = im.ReadString();

            Timestamp = im.ReadDouble();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(UpDownRotation);
            om.Write(LeftRightRotation);

            om.Write(PositionX);
            om.Write(PositionY);
            om.Write(PositionZ);

            om.Write(IsMe);

            om.Write(Slot);

            om.Write(Username);

            Timestamp = NetTime.Now;

            om.Write(Timestamp);
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