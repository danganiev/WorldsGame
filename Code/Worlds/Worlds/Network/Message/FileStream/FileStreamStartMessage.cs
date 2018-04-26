using System;
using System.IO;
using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    // Message that is sent on the start of the file exchange. It contains things like file name, size, etc
    public class FileStreamStartMessage : IGameMessage
    {
        public FileStreamStartMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public ulong Filesize { get; set; }        
        public string Filename { get; set; }

        public FileStreamStartMessage(ulong filesize, string filename)
        {
            Filesize = filesize;
            Filename = filename;
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.FileStreamStart; }
        }

        public void Decode(NetIncomingMessage im)
        {
            Filesize = im.ReadUInt64();
            Filename = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Filesize);            
            om.Write(Filename);
        }

        public void Send(NetConnection connection)
        {
            NetOutgoingMessage om = connection.Peer.CreateMessage(8 + 1);

            om.Write((byte)MessageType);
            Encode(om);
                                   
            connection.SendMessage(om, NetDeliveryMethod.ReliableOrdered, 1);
        }
    }
}
