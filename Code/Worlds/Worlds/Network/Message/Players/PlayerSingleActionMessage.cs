using Lidgren.Network;
using Microsoft.Xna.Framework;
using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public enum PlayerNetworkActionType : byte
    {
        PrimaryAction,
        SecondaryAction
    }

    // Universal message when player performed a single action. There should be a different message for continuous actions
    public class PlayerSingleActionMessage : IGameMessage, IClientSendableMessage, IServerSendableMessage, ITimeStampedMessage
    {
        private byte _actionTypeByte;

        private PlayerNetworkActionType _actionType;

        public PlayerNetworkActionType ActionType
        {
            get { return (PlayerNetworkActionType)_actionTypeByte; }
            set { _actionTypeByte = (byte)value; }
        }

        public double Timestamp { get; set; }

        public byte Slot { get; set; }

        public PlayerSingleActionMessage(PlayerNetworkActionType actionType)
        {
            ActionType = actionType;
        }

        public PlayerSingleActionMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerSingleAction; }
        }

        public void Decode(NetIncomingMessage im)
        {
            _actionTypeByte = im.ReadByte();

            Slot = im.ReadByte();

            Timestamp = im.ReadDouble();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(_actionTypeByte);

            om.Write(Slot);

            Timestamp = NetTime.Now;

            om.Write(Timestamp);
        }

        public void Send(NetConnection connection)
        {
            NetOutgoingMessage om = connection.Peer.CreateMessage();

            om.Write((byte)MessageType);
            Encode(om);

            connection.SendMessage(om, NetDeliveryMethod.UnreliableSequenced, 0);
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this, deliveryMethod: NetDeliveryMethod.UnreliableSequenced);
        }
    }
}