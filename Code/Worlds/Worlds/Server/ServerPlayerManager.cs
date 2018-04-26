using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Lidgren.Network;

using Microsoft.Xna.Framework;

using NLog;
using WorldsGame.Gamestates;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Network.Message.Players;
using WorldsGame.Playing.Players;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;

using XNAGameConsole;

namespace WorldsGame.Server
{
    internal class ServerPlayerManager : IPlayerManager
    {
        internal const byte MAX_PLAYER_COUNT = 32;

        private ServerMessageProcessor MessageProcessor { get; set; }

        private ServerNetworkManager NetworkManager { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal Dictionary<IPEndPoint, Player> ConnectedPlayers { get; set; }

        // Save it for later
        //        private List<Player> PlayerList { get; set; }

        private List<byte> AvailableSlots { get; set; }

        private bool AreSlotsAvailable
        {
            get { return AvailableSlots.Count > 0; }
        }

        // Username - IP projection
        // Used only on connect/disconnect/bundle sending
        internal Dictionary<IPEndPoint, string> UsernameIPs { get; set; }

        internal List<string> ClaimedUsernames { get; set; }

        // We store LOCAL timestamp, since ReadTime automagically makes it from remote time
        internal Dictionary<IPEndPoint, double> LastUpdateTimes { get; set; }

        internal Queue<Tuple<NetConnection, string>> ConnectionsWaiting { get; set; }

        private ServerState _serverState;

        public Player ClientPlayer
        {
            get { throw new InvalidOperationException("No client player on server"); }
        }

        internal ServerPlayerManager(ServerState state, ServerNetworkManager networkManager, ServerMessageProcessor messageProcessor)
        {
            _serverState = state;

            InitializePlayerFields();

            NetworkManager = networkManager;
            MessageProcessor = messageProcessor;
        }

        private void InitializePlayerFields()
        {
            ConnectedPlayers = new Dictionary<IPEndPoint, Player>();
            ClaimedUsernames = new List<string>();
            //            PlayerList = new List<Player>(MAX_PLAYER_COUNT);
            AvailableSlots = new List<byte>();
            for (byte i = 0; i < MAX_PLAYER_COUNT; i++)
            {
                AvailableSlots.Add(i);
            }

            UsernameIPs = new Dictionary<IPEndPoint, string>();
            LastUpdateTimes = new Dictionary<IPEndPoint, double>();
        }

        public World World { get; private set; }

        public void Initialize(World world)
        {
            World = world;

            MessageProcessor.OnPlayerInitializationRequest += OnPlayerInitializationRequest;
            MessageProcessor.OnPlayerDelta += OnPlayerDelta;
            MessageProcessor.OnPlayerSingleAction += OnPlayerSingleAction;
            MessageProcessor.OnChatMessage += OnChatMessage;
            MessageProcessor.OnPlayerBlockTypeChange += OnPlayerBlockTypeChange;
            MessageProcessor.OnPlayerMovementBehaviourChange += OnPlayerMovementBehaviourChange;
            MessageProcessor.OnDisconnect += OnDisconnect;

            ConnectionsWaiting = new Queue<Tuple<NetConnection, string>>();

            Messenger.On<NetConnection, string>("AddConnectedIP", QueueConnection);
        }

        public void Update(GameTime gameTime)
        {
            ProcessWaitingConnections();
        }

        public void UpdateStep(GameTime gameTime)
        {
            BroadcastPlayerInformation();
        }

        private void BroadcastPlayerInformation()
        {
            foreach (KeyValuePair<IPEndPoint, Player> connectedPlayer in ConnectedPlayers)
            {
                ServerNetworkPlayerDescription serverDescription = ServerNetworkPlayerDescription.CreateFromNetworkDescription(
                    connectedPlayer.Value.PlayerDescription);
                serverDescription.Position = connectedPlayer.Value.Position;
                serverDescription.YVelocity = connectedPlayer.Value.YVelocity;

                var otherPlayerDeltaMessage = new ServerOtherPlayerDeltaMessage(serverDescription)
                {
                    Slot = connectedPlayer.Value.ServerSlot
                };
                SendToAll(otherPlayerDeltaMessage, connectedPlayer.Value.Connection);
            }
        }

        public void SpawnMainPlayer(SpawnPlayerParams paramz)
        {
            throw new NotSupportedException("SpawnMainPlayer not supported on server");
        }

        internal Player AddPlayer(string username, NetConnection connection)
        {
            var player = new Player(World, GetNewPlayerPosition(), isClientPlayer: false)
            {
                UpDownRotation = 0,
                LeftRightRotation = -MathHelper.PiOver2,
                IsPlayerPositionFound = false,
                Connection = connection,
                Username = username,
                ServerSlot = GetAvailableSlot()
            };

            ConnectedPlayers.Add(connection.RemoteEndPoint, player);

            return player;
        }

        private byte GetAvailableSlot()
        {
            byte slot = AvailableSlots.First();
            AvailableSlots.Remove(slot);
            return slot;
        }

        internal Player GetPlayer(IPEndPoint ip)
        {
            Player player;
            ConnectedPlayers.TryGetValue(ip, out player);
            return player;
        }

        private Vector3 GetNewPlayerPosition()
        {
            return _serverState.SpawnPoint;
        }

        private void OnPlayerInitializationRequest(NetIncomingMessage im, PlayerInitializationRequestMessage message)
        {
            SendOtherPlayersList(im);
            SpawnClientPlayer(im);
        }

        private void SendOtherPlayersList(NetIncomingMessage im)
        {
            foreach (KeyValuePair<IPEndPoint, Player> connectedPlayer in ConnectedPlayers)
            {
                SendInitializationMessage(im, connectedPlayer.Value, areOtherPlayersRequested: true);
            }
        }

        private void SpawnClientPlayer(NetIncomingMessage im)
        {
            var playerName = PlayerName(im.SenderEndPoint);
            Player player = AddPlayer(playerName, im.SenderConnection);

            SendInitializationMessage(im, player);
        }

        private void SendInitializationMessage(NetIncomingMessage im, Player player, bool areOtherPlayersRequested = false)
        {
            var initializationMessage = new PlayerInitializationMessage(
                player.UpDownRotation, player.LeftRightRotation, player.Position)
            {
                Slot = player.ServerSlot,
                Username = player.Username
            };

            if (!areOtherPlayersRequested)
            {
                initializationMessage.Send(im.SenderConnection);
            }

            initializationMessage.IsMe = false;

            if (areOtherPlayersRequested)
            {
                // Strange if picking, but okay in readability, I guess
                initializationMessage.Send(im.SenderConnection);
            }
            else
            {
                NetworkManager.SendToAll(initializationMessage, im.SenderConnection, NetDeliveryMethod.ReliableUnordered);
            }
        }

        internal string PlayerName(IPEndPoint endPoint)
        {
            return UsernameIPs.ContainsKey(endPoint) ? UsernameIPs[endPoint] : "";
        }

        private void OnPlayerDelta(NetIncomingMessage im, PlayerDeltaMessage playerDeltaMessage)
        {
            double messageTime = playerDeltaMessage.Timestamp;

            // Exception once was catched here
            if (!LastUpdateTimes.ContainsKey(im.SenderEndPoint))
            {
                // We probably should return and update nothing, because this is an exceptional situation,
                // and player probably doesn't exist
                return;
            }

            if (messageTime < LastUpdateTimes[im.SenderEndPoint])
            {
                return;
            }

            var player = UpdatePlayer(im, playerDeltaMessage);

            var serverPlayerDeltaMessage = new ServerPlayerDeltaMessage(player.Position)
            {
                Index = playerDeltaMessage.Index
            };
            serverPlayerDeltaMessage.Send(im.SenderConnection);
        }

        private Player UpdatePlayer(NetIncomingMessage im, PlayerDeltaMessage playerDeltaMessage)
        {
            double lastUpdateTime = LastUpdateTimes[im.SenderEndPoint];

            Player player = GetPlayer(im.SenderEndPoint);

            foreach (var networkPlayerDescription in playerDeltaMessage.CollectedMovementChanges)
            {
                float passedTime = (float)(networkPlayerDescription.Timestamp - lastUpdateTime);

                player.UpDownRotation = networkPlayerDescription.UpDownRotation;
                player.LeftRightRotation = networkPlayerDescription.LeftRightRotation;

                if (networkPlayerDescription.JumpOccured)
                {
                    player.Physics.Jump();
                    continue;
                }

                player.Update(passedTime, networkPlayerDescription);
                lastUpdateTime = networkPlayerDescription.Timestamp;
            }

            LastUpdateTimes[im.SenderEndPoint] = lastUpdateTime;
            return player;
        }

        private void OnPlayerSingleAction(NetIncomingMessage im, PlayerSingleActionMessage message)
        {
            Player player = GetPlayer(im.SenderEndPoint);
            player.DoPlayerAction(message.ActionType);
        }

        private void QueueConnection(NetConnection connection, string username)
        {
            ConnectionsWaiting.Enqueue(new Tuple<NetConnection, string>(connection, username));
        }

        private void ProcessWaitingConnections()
        {
            if (ConnectionsWaiting.Count > 0)
            {
                foreach (Tuple<NetConnection, string> tuple in ConnectionsWaiting)
                {
                    AddConnectedIP(tuple.Item1, tuple.Item2);
                }

                ConnectionsWaiting.Clear();
            }
        }

        private void AddConnectedIP(NetConnection connection, string username)
        {
            if (ClaimedUsernames.Contains(username))
            {
                DisconnectPlayer("This username is already in use.", connection);
                return;
            }

            if (!AreSlotsAvailable)
            {
                DisconnectPlayer("Server is full.", connection);
            }

            UsernameIPs[connection.RemoteEndPoint] = username;
            ClaimedUsernames.Add(username);
            LastUpdateTimes[connection.RemoteEndPoint] = 0;
        }

        private void RemovePlayer(IPEndPoint endPoint)
        {
            Player player;
            ConnectedPlayers.TryGetValue(endPoint, out player);
            ConnectedPlayers.Remove(endPoint);
            UsernameIPs.Remove(endPoint);
            // Save PlayerList for later
            //            PlayerList.RemoveAt(player.ServerSlot);

            if (player != null)
            {
                AvailableSlots.Add(player.ServerSlot);
                ClaimedUsernames.Remove(player.Username);
                LastUpdateTimes.Remove(player.Connection.RemoteEndPoint);

                var disconnectMessage = new PlayerDisconnectMessage(player.ServerSlot);
                SendToAll(disconnectMessage);

                player.Dispose();
            }
        }

        private void OnDisconnect(NetIncomingMessage im)
        {
            RemovePlayer(im.SenderEndPoint);
        }

        private void DisconnectPlayer(string disconnectText, NetConnection connection = null)
        {
            if (connection != null)
            {
                Player player;
                ConnectedPlayers.TryGetValue(connection.RemoteEndPoint, out player);

                connection.Disconnect(disconnectText);
                if (player != null)
                {
                    player.Connection.Disconnect(disconnectText);
                }

                RemovePlayer(connection.RemoteEndPoint);
            }
        }

        internal void SendToAll(IGameMessage message, NetConnection except = null)
        {
            NetworkManager.SendToAll(message, except);
        }

        // Save it for later
        // private void SendToAllInRadius()

        private void OnChatMessage(NetIncomingMessage im, ChatMessage message)
        {
            byte playerSlot = GetPlayer(im.SenderEndPoint).ServerSlot;
            message.PlayerSlot = playerSlot;

            NetworkManager.SendToAll(message, null, NetDeliveryMethod.ReliableUnordered);
        }

        private void OnPlayerBlockTypeChange(NetIncomingMessage im, PlayerBlockTypeChangeMessage message)
        {
            Player player = GetPlayer(im.SenderEndPoint);
            if (player != null)
            {
                player.SetAdderBlockType(message.BlockTypeKey);
            }
        }

        private void OnPlayerMovementBehaviourChange(NetIncomingMessage im, PlayerMovementBehaviourChangeMessage message)
        {
            Player player = GetPlayer(im.SenderEndPoint);

            if (player != null)
            {
                player.ToggleMovementBehaviourChange();
            }
        }

        public void Dispose()
        {
            MessageProcessor.OnPlayerInitializationRequest -= OnPlayerInitializationRequest;
            MessageProcessor.OnPlayerDelta -= OnPlayerDelta;
            MessageProcessor.OnPlayerSingleAction -= OnPlayerSingleAction;
            MessageProcessor.OnChatMessage -= OnChatMessage;
            MessageProcessor.OnDisconnect -= OnDisconnect;

            Messenger.Off("AddConnectedIP");

            foreach (var connectedPlayer in ConnectedPlayers)
            {
                connectedPlayer.Value.Dispose();
            }
        }
    }
}