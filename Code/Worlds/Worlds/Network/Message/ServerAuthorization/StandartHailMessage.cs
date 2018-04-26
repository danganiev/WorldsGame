using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    public class StandartHailMessage : IGameMessage
    {
        public string HailText { get; set; }        

        public StandartHailMessage(string hailText)
        {
            HailText = hailText;
        }

        public StandartHailMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.StandartHailMessage; }
        }

        public void Decode(NetIncomingMessage im)
        {
            HailText = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {            
            om.Write(HailText);
        }

        public void EncodeWithType(NetOutgoingMessage om)
        {
            om.Write((byte)MessageType);
            om.Write(HailText);
        }
    }
}
