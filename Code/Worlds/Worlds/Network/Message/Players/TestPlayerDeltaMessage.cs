using Lidgren.Network;
using Microsoft.Xna.Framework;
using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public class TestPlayerDeltaMessage : IGameMessage, IClientSendableMessage, ITimeStampedMessage
    {
        public Vector3 Position
        {
            get { return new Vector3(PositionX, PositionY, PositionZ); }
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
                PositionZ = value.Z;
            }
        }

        public float LeftRightRotation { get; set; }

        public float UpDownRotation { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        public int Index { get; set; }

        public double Timestamp { get; set; }

        public TestPlayerDeltaMessage(float leftRightRotation, float upDownRotation, Vector3 position)
        {
            // NOTE: LookVector could and will be hacked too, if the game becomes popular, so I'm saving the task for later
            LeftRightRotation = leftRightRotation;
            UpDownRotation = upDownRotation;

            Position = position;
        }

        public TestPlayerDeltaMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.TestPlayerDelta; }
        }

        public void Decode(NetIncomingMessage im)
        {
            LeftRightRotation = im.ReadFloat();
            UpDownRotation = im.ReadFloat();

            PositionX = im.ReadFloat();
            PositionY = im.ReadFloat();
            PositionZ = im.ReadFloat();

            Index = im.ReadInt32();

            Timestamp = im.ReadDouble();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(LeftRightRotation);
            om.Write(UpDownRotation);

            om.Write(PositionX);
            om.Write(PositionY);
            om.Write(PositionZ);

            om.Write(Index);

            Timestamp = NetTime.Now;
            om.Write(Timestamp);
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this, deliveryMethod: NetDeliveryMethod.UnreliableSequenced);
        }
    }
}