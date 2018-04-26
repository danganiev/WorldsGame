using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Network.Message.ServerAuthorization;

namespace WorldsGame.Network
{
    internal class AtlasReceiver : IDisposable
    {
        private FileStreamingReceiver AtlasFileReceiver { get; set; }
        private ClientNetworkManager ClientNetworkManager { get; set; }

        internal bool AreAtlasesDownloaded { get; set; }

        internal event Action<ulong> DownloadStarted = size => { };
        internal event Action<ulong, ulong> DownloadProgress = (downloadedSize, fullsize) => { };
        internal event Action<string> DownloadFinished = filename => { };

        internal AtlasReceiver(ClientNetworkManager clientNetworkManager)
        {            
            AreAtlasesDownloaded = false;
            ClientNetworkManager = clientNetworkManager;            
        }

        internal void InitializeReceiver(int downloadedAtlasesCount)
        {
            AtlasFileReceiver = new FileStreamingReceiver();
            AtlasFileReceiver.DownloadStarted += OnDownloadStarted;
            AtlasFileReceiver.DownloadProgress += OnDownloadProgress;            
            AtlasFileReceiver.DownloadFinished += OnDownloadFinished;

            var atlasRequestMessage = new AtlasesRequestMessage(false, true, downloadedAtlasesCount);
            ClientNetworkManager.SendMessage(atlasRequestMessage);            
        }

        internal void StartReceiving(FileStreamStartMessage message)
        {
            AtlasFileReceiver.StartReceiving(message);
        }

        public void Receive(FileStreamDataMessage message)
        {
            AtlasFileReceiver.Receive(message);
        }

        private void OnDownloadStarted(ulong size)
        {
            DownloadStarted(size);
        }

        private void OnDownloadProgress(ulong downloadedSize, ulong fullsize)
        {
            DownloadProgress(downloadedSize, fullsize);
        }

        private void OnDownloadFinished(string filename)
        {
            DownloadFinished(filename);
        }

        public void Dispose()
        {
            DownloadStarted = null;
            DownloadProgress = null;
            DownloadFinished = null;            

            if (AtlasFileReceiver != null)
            {
                AtlasFileReceiver.Dispose();
            }
        }
    }
}
