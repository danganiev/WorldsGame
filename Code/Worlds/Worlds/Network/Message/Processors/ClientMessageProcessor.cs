using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;

using Microsoft.Xna.Framework;

using WorldsGame.Network.Manager;
using WorldsGame.Network.Message.Chunks;
using WorldsGame.Network.Message.Players;
using WorldsGame.Network.Message.ServerAuthorization;

using XNAGameConsole;

namespace WorldsGame.Network.Message
{
    internal class ClientMessageProcessor : IDisposable
    {
        private GameConsole GameConsole { get; set; }

        internal ClientNetworkManager NetworkManager { get; private set; }

        internal event Action<string> OnDisconnect = reason => { };

        internal event Action<GameMessageType> OnApprove = type => { };

        internal event Action<FileStreamStartMessage> OnFileStreamingStart = message => { };

        internal event Action<FileStreamDataMessage> OnFileStreamingData = message => { };

        internal event Action<AtlasCountMessage> OnAtlasesCountAnswer = message => { };

        internal event Action<FileStreamStartMessage> OnAtlasStreamingStart = message => { };

        internal event Action<ChunkResponseMessage> OnChunkResponse = message => { };

        internal event Action<PlayerInitializationMessage> OnPlayerInitialization = message => { };

        internal event Action<ServerPlayerDeltaMessage> OnServerPlayerDelta = message => { };

        internal event Action<ServerOtherPlayerDeltaMessage> OnServerOtherPlayerDelta = message => { };

        internal event Action<BlockUpdateMessage> OnBlockUpdate = message => { };

        internal event Action<ChatMessage> OnChatMessage = message => { };

        internal event Action<PlayerDisconnectMessage> OnOtherPlayerDisconnect = message => { };

        internal bool IsDisposed { get; private set; }

        internal ClientMessageProcessor(ClientNetworkManager networkManager, GameConsole gameConsole)
        {
            NetworkManager = networkManager;
            GameConsole = gameConsole;
            IsDisposed = false;
        }

        internal void Update(GameTime gameTime)
        {
            ProcessMessages();
        }

        private void ProcessMessages()
        {
            NetIncomingMessage im;

            while ((im = NetworkManager.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        OnError(im);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        OnStatusChange(im);
                        break;

                    case NetIncomingMessageType.Data:
                        OnData(im);
                        break;
                }

                NetworkManager.Recycle(im);
            }
        }

        private void OnData(NetIncomingMessage im)
        {
            var messageType = (GameMessageType)im.ReadByte();

            switch (messageType)
            {
                case GameMessageType.FileStreamStart:
                    var startMessage = new FileStreamStartMessage(im);
                    if (!startMessage.Filename.Contains("atlas"))
                    {
                        OnFileStreamingStart(startMessage);
                    }
                    else
                    {
                        OnAtlasStreamingStart(startMessage);
                    }
                    break;

                case GameMessageType.FileStreamData:
                    var dataMessage = new FileStreamDataMessage(im);
                    OnFileStreamingData(dataMessage);
                    break;

                case GameMessageType.AtlasCount:
                    var countMessage = new AtlasCountMessage(im);
                    OnAtlasesCountAnswer(countMessage);
                    break;

                case GameMessageType.ChunkResponse:
                    var chunkResponseMessage = new ChunkResponseMessage(im);
                    OnChunkResponse(chunkResponseMessage);
                    break;

                case GameMessageType.PlayerInitialization:
                    var playerInitializationMessage = new PlayerInitializationMessage(im);
                    OnPlayerInitialization(playerInitializationMessage);
                    break;

                case GameMessageType.ServerPlayerDelta:
                    var serverPlayerDeltaMessage = new ServerPlayerDeltaMessage(im);
                    OnServerPlayerDelta(serverPlayerDeltaMessage);
                    break;

                case GameMessageType.ServerOtherPlayerDelta:
                    var serverOtherPlayerDeltaMessage = new ServerOtherPlayerDeltaMessage(im);
                    OnServerOtherPlayerDelta(serverOtherPlayerDeltaMessage);
                    break;

                case GameMessageType.BlockUpdate:
                    var blockUpdateMessage = new BlockUpdateMessage(im);
                    OnBlockUpdate(blockUpdateMessage);
                    break;

                case GameMessageType.ChatMessage:
                    var chatMessage = new ChatMessage(im);
                    OnChatMessage(chatMessage);
                    break;

                case GameMessageType.PlayerDisconnect:
                    var disconnectMessage = new PlayerDisconnectMessage(im);
                    OnOtherPlayerDisconnect(disconnectMessage);
                    break;
            }
        }

        private void OnError(NetIncomingMessage im)
        {
        }

        private void OnStatusChange(NetIncomingMessage im)
        {
            var status = (NetConnectionStatus)im.ReadByte();
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    // This is due to hardcode in Lidgren, haha
                    // https://code.google.com/p/lidgren-network-gen3/issues/detail?id=66

                    NetIncomingMessage incomingMessage = im.SenderConnection.RemoteHailMessage;
                    var messageType = (GameMessageType)incomingMessage.ReadByte();

                    OnApprove(messageType);
                    break;

                case NetConnectionStatus.Disconnected:
                    string reason = im.ReadString();
                    OnDisconnect(reason);
                    break;
            }
        }

        public void Dispose()
        {
            ClearEvents();

            NetworkManager = null;
            GameConsole = null;

            IsDisposed = true;
        }

        internal void ClearEvents()
        {
            OnDisconnect = null;
            OnApprove = null;
            OnFileStreamingStart = null;
            OnFileStreamingData = null;
            OnAtlasesCountAnswer = null;
            OnAtlasStreamingStart = null;
            OnChunkResponse = null;
            OnPlayerInitialization = null;
            OnServerPlayerDelta = null;
            OnServerOtherPlayerDelta = null;
            OnBlockUpdate = null;
        }

        internal void ResetEvents()
        {
            OnDisconnect = reason => { };
            OnApprove = type => { };
            OnFileStreamingStart = message => { };
            OnFileStreamingData = message => { };
            OnAtlasesCountAnswer = message => { };
            OnAtlasStreamingStart = message => { };
            OnChunkResponse = message => { };
            OnPlayerInitialization = message => { };
            OnServerPlayerDelta = message => { };
            OnServerOtherPlayerDelta = message => { };
            OnBlockUpdate = message => { };
        }
    }
}