using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public interface IServerSendableMessage
    {
        void Send(NetConnection connection);
    }
}
