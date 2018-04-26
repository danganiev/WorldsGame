using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace WorldsGame.Network.Message.ServerAuthorization
{
    public class AtlasesRequestMessage : IGameMessage
    {
        public bool IsRequestingCount { get; set; }
        public bool IsRequestingAtlas { get; set; }
        public int AtlasIndex { get; set; }

        public AtlasesRequestMessage(bool isRequestingCount, bool isRequestingAtlas, int atlasIndex)
        {
            IsRequestingCount = isRequestingCount;
            IsRequestingAtlas = isRequestingAtlas;
            AtlasIndex = atlasIndex;
        }

        public AtlasesRequestMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.AtlasesRequest; }
        }

        public void Decode(NetIncomingMessage im)
        {
            IsRequestingCount = im.ReadBoolean();
            IsRequestingAtlas = im.ReadBoolean();
            AtlasIndex = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(IsRequestingCount);
            om.Write(IsRequestingAtlas);
            om.Write(AtlasIndex);
        }
    }
}
