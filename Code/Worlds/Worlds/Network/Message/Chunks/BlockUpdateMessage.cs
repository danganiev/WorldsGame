using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using WorldsGame.Terrain.Blocks.Types;
using WorldsLib;

namespace WorldsGame.Network.Message.Chunks
{
    public class BlockUpdateMessage : IGameMessage, ITimeStampedMessage
    {
        public GameMessageType MessageType
        {
            get { return GameMessageType.BlockUpdate; }
        }

        public double Timestamp { get; set; }

        public Vector3i Position
        {
            get { return new Vector3i(PositionX, PositionY, PositionZ); }
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
                PositionZ = value.Z;
            }
        }

        public int PositionX { get; set; }

        public int PositionY { get; set; }

        public int PositionZ { get; set; }

        public int BlockTypeKey { get; set; }

        public BlockUpdateMessage(Vector3i position, int key)
        {
            Position = position;
            BlockTypeKey = key;
        }

        public BlockUpdateMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public void Decode(NetIncomingMessage im)
        {
            PositionX = im.ReadInt32();
            PositionY = im.ReadInt32();
            PositionZ = im.ReadInt32();

            BlockTypeKey = im.ReadInt32();

            Timestamp = im.ReadDouble();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(PositionX);
            om.Write(PositionY);
            om.Write(PositionZ);

            om.Write(BlockTypeKey);

            Timestamp = NetTime.Now;

            om.Write(Timestamp);
        }
    }
}