using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lidgren.Network;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message.ServerAuthorization;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Server;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Network
{
    internal class BundleStreamingManager : IDisposable
    {
        private Thread _fileStreamingThread;

        private readonly ConcurrentDictionary<string, FileStreamer> Streamers;

        private CompiledGameBundle Bundle { get; set; }

        private ServerPlayerManager PlayerManager { get; set; }

        internal string BundleFullFilePath { get; set; }

        internal bool IsStreaming { get; private set; }

        internal BundleStreamingManager(CompiledGameBundle bundle, ServerPlayerManager playerManager)
        {
            Bundle = bundle;
            PlayerManager = playerManager;
            Streamers = new ConcurrentDictionary<string, FileStreamer>();
        }

        internal void Start()
        {
            Messenger.On<NetConnection>("GameBundleRequested", OnGameBundleRequest);
            Messenger.On<NetIncomingMessage, AtlasesRequestMessage>("AtlasCountRequested", OnAtlasCountRequest);
            Messenger.On<NetIncomingMessage, AtlasesRequestMessage>("AtlasRequested", OnAtlasRequest);

            _fileStreamingThread = new Thread(StreamFiles);
            _fileStreamingThread.Start();
        }

        private void OnGameBundleRequest(NetConnection connection)
        {
            string username = PlayerManager.PlayerName(connection.RemoteEndPoint);
            
            // There was a disconnect because of the same username, or some other reason
            if (username == "")
            {
                return;
            }

            Streamers.TryAdd(username, new FileStreamer(connection, Bundle.GetFullFilePath()));
        }

        private void OnAtlasCountRequest(NetIncomingMessage im, AtlasesRequestMessage decodedMessage)
        {
            var responseMessage = new AtlasCountMessage(Bundle.TextureAtlases.Count - 1);
            responseMessage.Send(im.SenderConnection);
        }

        private void OnAtlasRequest(NetIncomingMessage im, AtlasesRequestMessage decodedMessage)
        {
            string username = PlayerManager.PlayerName(im.SenderEndPoint);
            string atlasName = CompiledGameBundleSave.GetFullFilePath(
                string.Format("atlas{0}.png", decodedMessage.AtlasIndex),
                BundleType.Normal, isAtlas: true, additionalContainerName: Bundle.FullName);

            Streamers.TryAdd(
                string.Format("{0}__atlas{1}", username, decodedMessage.AtlasIndex),
                new FileStreamer(im.SenderConnection, atlasName));
        }

        private void StreamFiles()
        {
            IsStreaming = true;

            var keysToRemove = new List<string>();

            while (IsStreaming)
            {
                if (Streamers.Count > 0)
                {
                    if (keysToRemove.Count > 0)
                    {
                        foreach (string key in keysToRemove)
                        {
                            FileStreamer streamer;
                            bool result = Streamers.TryRemove(key, out streamer);
                            streamer.Dispose();
                        }

                        keysToRemove.Clear();
                    }

                    foreach (KeyValuePair<string, FileStreamer> keyValue in Streamers)
                    {
                        FileStreamer streamer = keyValue.Value;

                        if (streamer != null)
                        {
                            streamer.Heartbeat(keyValue.Key, keysToRemove);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public void Dispose()
        {
            IsStreaming = false;

            foreach (KeyValuePair<string, FileStreamer> fileStreamer in Streamers)
            {
                fileStreamer.Value.Dispose();
            }
        }
    }
}