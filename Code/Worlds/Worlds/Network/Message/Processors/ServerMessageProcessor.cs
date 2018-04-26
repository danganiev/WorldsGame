using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Lidgren.Network;

using Microsoft.Xna.Framework;

using NLog;
using WorldsGame.Gamestates;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message.Players;
using WorldsGame.Network.Message.ServerAuthorization;
using WorldsGame.Utils.EventMessenger;

using XNAGameConsole;

namespace WorldsGame.Network.Message
{
    internal class ServerMessageProcessor : IDisposable
    {
        //        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ServerNetworkManager NetworkManager { get; set; }

        private GameConsole GameConsole { get; set; }

        private Queue<Tuple<GameMessageType, NetIncomingMessage>> MessageQueue { get; set; }

        internal event Action<NetIncomingMessage, ChunkRequestMessage> OnChunkRequest = (im, message) => { };

        internal event Action<NetIncomingMessage, PlayerInitializationRequestMessage> OnPlayerInitializationRequest = (im, message) => { };

        internal event Action<NetIncomingMessage, PlayerDeltaMessage> OnPlayerDelta = (im, message) => { };

        internal event Action<NetIncomingMessage, PlayerSingleActionMessage> OnPlayerSingleAction = (im, message) => { };

        internal event Action<NetIncomingMessage, ChatMessage> OnChatMessage = (im, message) => { };

        internal event Action<NetIncomingMessage, PlayerBlockTypeChangeMessage> OnPlayerBlockTypeChange = (im, message) => { };

        internal event Action<NetIncomingMessage, PlayerMovementBehaviourChangeMessage> OnPlayerMovementBehaviourChange = (im, message) => { };

        internal event Action<NetIncomingMessage> OnDisconnect = im => { };

        internal ServerMessageProcessor(ServerNetworkManager networkManager, GameConsole gameConsole)
        {
            NetworkManager = networkManager;
            GameConsole = gameConsole;
            MessageQueue = new Queue<Tuple<GameMessageType, NetIncomingMessage>>();
        }

        internal void Update(GameTime gameTime)
        {
            ProcessMessages();
        }

        internal void UpdateStep(GameTime gameTime)
        {
            ProcessQueueMessages();
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
                        NetworkManager.Recycle(im);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        OnStatusChange(im);
                        NetworkManager.Recycle(im);
                        break;

                    case NetIncomingMessageType.ConnectionApproval:
                        OnConnectionAttempt(im);
                        NetworkManager.Recycle(im);
                        break;

                    case NetIncomingMessageType.Data:
                        OnData(im);
                        break;
                }
            }
        }

        private void ProcessQueueMessages()
        {
            while (MessageQueue.Count != 0)
            {
                Tuple<GameMessageType, NetIncomingMessage> tuple = MessageQueue.Dequeue();
                OnTimedMessage(tuple.Item1, tuple.Item2);
            }
        }

        private void OnData(NetIncomingMessage im)
        {
            var gameMessageType = (GameMessageType)im.ReadByte();

            switch (gameMessageType)
            {
                case GameMessageType.GameBundleRequest:
                    OnGameBundleRequest(im.SenderConnection);
                    NetworkManager.Recycle(im);
                    break;

                case GameMessageType.AtlasesRequest:
                    OnAtlasesRequest(im);
                    NetworkManager.Recycle(im);
                    break;

                case GameMessageType.ChunkRequest:
                    var chunkRequestMessage = new ChunkRequestMessage(im);
                    OnChunkRequest(im, chunkRequestMessage);
                    NetworkManager.Recycle(im);
                    break;

                case GameMessageType.PlayerInitializationRequest:
                    var playerInitializationRequestMessage = new PlayerInitializationRequestMessage(im);
                    OnPlayerInitializationRequest(im, playerInitializationRequestMessage);
                    NetworkManager.Recycle(im);
                    break;

                case GameMessageType.ChatMessage:
                    var chatMessage = new ChatMessage(im);
                    OnChatMessage(im, chatMessage);

                    NetworkManager.Recycle(im);
                    break;

                case GameMessageType.PlayerDelta:
                case GameMessageType.PlayerSingleAction:
                case GameMessageType.PlayerBlockTypeChange:
                case GameMessageType.PlayerMovementBehaviourChange:
                    EnqueueMessage(im, gameMessageType);
                    break;
            }
        }

        private void EnqueueMessage(NetIncomingMessage im, GameMessageType gameMessageType)
        {
            MessageQueue.Enqueue(Tuple.Create(gameMessageType, im));
        }

        private void OnTimedMessage(GameMessageType gameMessageType, NetIncomingMessage im)
        {
            switch (gameMessageType)
            {
                case GameMessageType.PlayerDelta:
                    var playerDeltaMessage = new PlayerDeltaMessage(im);
                    OnPlayerDelta(im, playerDeltaMessage);
                    break;

                case GameMessageType.PlayerSingleAction:
                    var playerSingleActionMessage = new PlayerSingleActionMessage(im);
                    OnPlayerSingleAction(im, playerSingleActionMessage);
                    break;

                case GameMessageType.PlayerBlockTypeChange:
                    var playerBlockTypeChangeMessage = new PlayerBlockTypeChangeMessage(im);
                    OnPlayerBlockTypeChange(im, playerBlockTypeChangeMessage);
                    break;

                case GameMessageType.PlayerMovementBehaviourChange:
                    var playerMovementBehaviourChangeMessage = new PlayerMovementBehaviourChangeMessage(im);
                    OnPlayerMovementBehaviourChange(im, playerMovementBehaviourChangeMessage);
                    break;
            }
            NetworkManager.Recycle(im);
        }

        private void OnGameBundleRequest(NetConnection connection)
        {
            Messenger.Invoke("GameBundleRequested", connection);
        }

        private void OnAtlasesRequest(NetIncomingMessage im)
        {
            var atlasRequestMessage = new AtlasesRequestMessage(im);

            if (atlasRequestMessage.IsRequestingCount)
            {
                Messenger.Invoke("AtlasCountRequested", im, atlasRequestMessage);
            }
            else if (atlasRequestMessage.IsRequestingAtlas)
            {
                Messenger.Invoke("AtlasRequested", im, atlasRequestMessage);
            }
        }

        private void OnError(NetIncomingMessage im)
        {
            GameConsole.WriteLine(im.ReadString());
        }

        private void OnStatusChange(NetIncomingMessage im)
        {
            switch ((NetConnectionStatus)im.ReadByte())
            {
                case NetConnectionStatus.Connected:
                    GameConsole.WriteLine(string.Format("{0} Connected", im.SenderEndPoint));
                    break;

                case NetConnectionStatus.Disconnected:
                    OnDisconnect(im);
                    GameConsole.WriteLine(string.Format("{0} Disconnected", im.SenderEndPoint));
                    break;

                case NetConnectionStatus.RespondedAwaitingApproval:
                    break;
            }
        }

        private void OnConnectionAttempt(NetIncomingMessage im)
        {
            var gameMessageType = (GameMessageType)im.ReadByte();

            if (gameMessageType == GameMessageType.ServerAuthorizationRequest)
            {
                var hailMessage = new StandartHailMessage("Hail and welcome, my friend, hail!");

                NetOutgoingMessage hailOutgoingMessage = NetworkManager.CreateMessage();
                hailMessage.EncodeWithType(hailOutgoingMessage);

                im.SenderConnection.Approve(hailOutgoingMessage);

                var message = new ServerAuthorizationRequestMessage(im);
                Messenger.Invoke("AddConnectedIP", im.SenderConnection, message.Username);

                // The idea is to add connected IP, then send approvement to the client, and then the client
                // sends checking messages, and if the client decides to download the bundle, it sends the downloading request.
            }
        }

        public void Dispose()
        {
            OnChunkRequest = null;
            OnPlayerInitializationRequest = null;
            OnPlayerDelta = null;
            OnDisconnect = null;

            MessageQueue.Clear();
        }
    }
}