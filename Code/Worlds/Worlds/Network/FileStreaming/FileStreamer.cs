using System;
using System.Collections.Generic;
using System.IO;

using Lidgren.Network;
using WorldsGame.Network.Message;

namespace WorldsGame.Network
{
    internal class FileStreamer : IDisposable
    {
        private FileStream _inputStream;
        private int _sentOffset;
        private int _chunkLen;
        private byte[] _tmpBuffer;
        private NetConnection _connection;

        internal bool IsFinished { get; set; }

        internal FileStreamer(NetConnection conn, string fileName)
        {
            _connection = conn;
            _inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            _chunkLen = _connection.Peer.Configuration.MaximumTransmissionUnit - 20;
            _tmpBuffer = new byte[_chunkLen];
            _sentOffset = 0;
            IsFinished = false;
        }

        // If we put this in a general Update, it would send only 60 * 1408 bytes per second,
        // that's very slow, so we would need another thread

        // 82 kb / sec
        internal void Heartbeat(string key, List<string> keysToRemove)
        {
            if (_inputStream == null)
                return;

            int windowSize, freeWindowSlots;
            _connection.GetSendQueueInfo(NetDeliveryMethod.ReliableOrdered, 1, out windowSize, out freeWindowSlots);

            if (freeWindowSlots > 0)
            {
                // send another part of the file!
                int remaining = (int)(_inputStream.Length - _sentOffset);
                int sendBytes = (remaining > _chunkLen ? _chunkLen : remaining);

                if (_sentOffset == 0)
                {
                    // first message; send length, chunk length and file name
                    var startMessage = new FileStreamStartMessage(
                        (ulong)_inputStream.Length, Path.GetFileName(_inputStream.Name));
                    startMessage.Send(_connection);
                }

                if (sendBytes < _chunkLen)
                {
                    _tmpBuffer = new byte[sendBytes];
                }

                // just assume we can read the whole thing in one Read()
                _inputStream.Read(_tmpBuffer, 0, sendBytes);

                var message = new FileStreamDataMessage(_tmpBuffer, sendBytes);
                message.Send(_connection);

                _sentOffset += sendBytes;

                if (remaining - sendBytes <= 0)
                {
                    _inputStream.Close();
                    _inputStream.Dispose();
                    _inputStream = null;

                    IsFinished = true;
                    keysToRemove.Add(key);
                }
            }
        }

        public void Dispose()
        {
            if (_inputStream != null)
            {
                _inputStream.Close();
                _inputStream.Dispose();
            }
        }
    }
}