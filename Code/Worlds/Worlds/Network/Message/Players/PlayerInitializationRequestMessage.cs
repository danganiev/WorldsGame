using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public class PlayerInitializationRequestMessage : IGameMessage, IClientSendableMessage
    {
        public PlayerInitializationRequestMessage()
        {
        }

        public PlayerInitializationRequestMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.PlayerInitializationRequest; }
        }

        public void Decode(NetIncomingMessage im)
        {
        }

        public void Encode(NetOutgoingMessage om)
        {
        }

        public void Send(ClientNetworkManager networkManager)
        {
            networkManager.SendMessage(this);
        }
    }
}