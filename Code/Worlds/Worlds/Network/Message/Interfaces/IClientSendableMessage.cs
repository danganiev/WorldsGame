using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public interface IClientSendableMessage
    {
        void Send(ClientNetworkManager networkManager);
    }
}