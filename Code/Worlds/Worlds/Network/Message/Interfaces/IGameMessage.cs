using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public interface IGameMessage
    {
        GameMessageType MessageType { get; }

        void Decode(NetIncomingMessage im);

        void Encode(NetOutgoingMessage om);
    }
}