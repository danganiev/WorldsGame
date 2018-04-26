using System;
using Lidgren.Network;

namespace WorldsGame.Network.Manager
{
    internal interface INetworkManager : IDisposable
    {
        void Disconnect();

        NetIncomingMessage ReadMessage();

        void Recycle(NetIncomingMessage im);

        NetOutgoingMessage CreateMessage();
    }
}