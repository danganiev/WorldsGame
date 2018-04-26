using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace WorldsGame.Network.Message.ServerAuthorization
{
    public class AtlasCountMessage : IGameMessage, IServerSendableMessage
    {
        public int AtlasCount { get; set; }

        public AtlasCountMessage(int count)
        {
            AtlasCount = count;
        }

        public AtlasCountMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.AtlasCount; }
        }

        public void Decode(NetIncomingMessage im)
        {
            AtlasCount = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(AtlasCount);
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
