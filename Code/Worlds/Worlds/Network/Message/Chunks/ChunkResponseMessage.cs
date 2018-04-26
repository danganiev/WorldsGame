using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Ionic.Zlib;

using Lidgren.Network;

namespace WorldsGame.Network.Message
{
    // Message for full chunk send
    public class ChunkResponseMessage : IGameMessage
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        private int _blockArraySize;

        private int[] _blocks;

        public int[] Blocks
        {
            get { return _blocks; }
            set
            {
                _blocks = value;
                var blocksData = new byte[value.Length * sizeof(int)];
                Buffer.BlockCopy(value, 0, blocksData, 0, blocksData.Length);

                using (var stream = new MemoryStream(blocksData))
                {
                    using (var compressedStream = new MemoryStream())
                    {
                        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                        {
                            stream.CopyTo(zipStream);
                        }

                        _blocksData = compressedStream.ToArray();

                        _blockArraySize = _blocksData.Length;
                    }
                }
            }
        }

        private byte[] _blocksData;

        public byte[] BlocksData
        {
            get { return _blocksData; }
            set
            {
                _blocksData = value;

                using (var compressedStream = new MemoryStream(value))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        {
                            zipStream.CopyTo(resultStream);
                        }

                        byte[] blocksData = resultStream.ToArray();

                        _blocks = new int[blocksData.Length / sizeof(int)];
                        Buffer.BlockCopy(blocksData, 0, _blocks, 0, blocksData.Length);
                    }
                }
            }
        }

        public ChunkResponseMessage(int x, int y, int z, int[] blocks)
        {
            X = x;
            Y = y;
            Z = z;
            Blocks = blocks;
        }

        public ChunkResponseMessage(NetIncomingMessage im)
        {
            Decode(im);
        }

        public GameMessageType MessageType
        {
            get { return GameMessageType.ChunkResponse; }
        }

        public void Decode(NetIncomingMessage im)
        {
            X = im.ReadInt32();
            Y = im.ReadInt32();
            Z = im.ReadInt32();
            _blockArraySize = im.ReadInt32();
            BlocksData = im.ReadBytes(_blockArraySize);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(X);
            om.Write(Y);
            om.Write(Z);
            om.Write(_blockArraySize);
            om.Write(BlocksData);
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