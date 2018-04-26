using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using WorldsGame.Network.Manager;

namespace WorldsGame.Network.Message
{
    public class ChunkRequestMessage : IGameMessage, IClientSendableMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public ChunkRequestMessage(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public ChunkRequestMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.ChunkRequest; }
        }

        public void Decode(NetIncomingMessage im)
        {
            X = im.ReadInt32();
            Y = im.ReadInt32();
            Z = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(X);
            om.Write(Y);
            om.Write(Z);
        }

        public void Send(ClientNetworkManager networkManager)
        {
//            DefaultMessageSender.Send(connection, this);
            networkManager.SendMessage(this);
        }
    }
}
