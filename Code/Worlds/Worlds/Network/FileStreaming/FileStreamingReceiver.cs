using System;
using System.IO;

using WorldsGame.Network.Message;
using WorldsGame.Utils;

namespace WorldsGame.Network
{
    internal class FileStreamingReceiver : IDisposable
    {
        private ulong _fileSize;
        private ulong _received;
        private FileStream _writeStream;

        private bool IsReceivingStarted { get; set; }

        internal bool IsDownloadFinished { get; set; }

        internal string FileName { get; set; }

        public event Action<ulong> DownloadStarted = size => { };

        public event Action<ulong, ulong> DownloadProgress = (dowloadedSize, fullsize) => { };

        public event Action<string> DownloadFinished = filename => { };

        internal void StartReceiving(FileStreamStartMessage message)
        {
            if (!IsReceivingStarted)
            {
                _fileSize = message.Filesize;
                FileName = message.Filename;
                _received = 0;

                Directory.CreateDirectory(Constants.TEMP_DIR_NAME);
                string savePath = Path.Combine(Constants.TEMP_DIR_NAME, message.Filename);
                _writeStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);

                IsReceivingStarted = true;
                IsDownloadFinished = false;

                DownloadStarted(_fileSize);
            }
        }

        internal void Receive(FileStreamDataMessage message)
        {
            if (_received >= _fileSize)
            {
                return;
            }

            byte[] byteBuffer = message.ByteBuffer;

            _received += (ulong)byteBuffer.Length;
            _writeStream.Write(byteBuffer, 0, byteBuffer.Length);

            DownloadProgress(_received, _fileSize);

            if (_received >= _fileSize)
            {
                _writeStream.Flush();
                _writeStream.Close();
                _writeStream.Dispose();

                IsReceivingStarted = false;

                DownloadFinished(FileName);

                DownloadStarted = null;
                DownloadProgress = null;
                DownloadFinished = null;

                IsDownloadFinished = true;
            }
        }

        public void Dispose()
        {
            if (_writeStream != null)
            {
                _writeStream.Close();
                _writeStream.Dispose();
            }

            DownloadStarted = null;
            DownloadProgress = null;
            DownloadFinished = null;
        }
    }
}