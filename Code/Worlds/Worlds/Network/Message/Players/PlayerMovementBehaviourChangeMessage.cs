using Lidgren.Network;
using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public class PlayerMovementBehaviourChangeMessage : IGameMessage, IClientSendableMessage
    {
        public PlayerMovementBehaviourChangeMessage()
        {
        }

        public PlayerMovementBehaviourChangeMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerMovementBehaviourChange; }
        }

        public void Decode(NetIncomingMessage im)
        {
        }

        public void Encode(NetOutgoingMessage om)
        {
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this, deliveryMethod: NetDeliveryMethod.ReliableSequenced);
        }
    }
}