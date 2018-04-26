using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public class ServerAuthorizationRequestMessage : IGameMessage
    {
        public ServerAuthorizationRequestMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public ServerAuthorizationRequestMessage(string username)
        {
            Username = username;
        }

        public string Username { get; set; }

        public GameMessageType MessageType
        {
            get { return GameMessageType.ServerAuthorizationRequest; }
        }

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
        }
    }
}