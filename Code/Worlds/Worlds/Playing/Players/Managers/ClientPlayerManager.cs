using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using NLog;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Server;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Players
{
    internal class PlayerDeltaMessageTimeProxy
    {
        internal NetworkPlayerDescription PlayerDescription { get; set; }

        internal double Timestamp { get; set; }

        internal double PreviousTimestamp { get; set; }
    }

    internal class ClientPlayerManager : IPlayerManager
    {
        public Player ClientPlayer { get; internal set; }

        private Player _clientPlayerShadowCopy;

        public World World { get; set; }

        internal List<Player> OtherPlayers { get; set; }

        internal List<byte> ActiveOtherPlayerSlots { get; private set; }

        private ClientNetworkManager NetworkManager { get; set; }

        private ClientMessageProcessor MessageProcessor { get; set; }

        private bool _hasClientPlayerArrived;

        private readonly Dictionary<int, PlayerDeltaMessage> _messagePool;
        private readonly List<PlayerDeltaMessageTimeProxy> _inverseMessagePool;

        private int _index = 0;
        private int _lastAcceptedIndex;

        private double PreviousTimeStamp { get; set; }

        private List<NetworkPlayerDescription> CollectedMovementChanges { get; set; }

        private readonly OtherPlayersMovementProcessor _otherPlayersMovementProcessor;

        private ClientPlayerActionManager _playerActionManager;

        internal ClientPlayerActionManager PlayerActionManager
        {
            get { return _playerActionManager; }
            set
            {
                _playerActionManager = value;

                if (value != null)
                {
                    _playerActionManager.OnPlayerDidPrimaryAction += OnClientPlayerPrimaryAction;
                    _playerActionManager.OnPlayerDidSecondaryAction += OnClientPlayerSecondaryAction;
                }
            }
        }

        internal ClientPlayerManager(ClientNetworkManager networkManager, ClientMessageProcessor messageProcessor)
        {
            NetworkManager = networkManager;
            MessageProcessor = messageProcessor;
            _hasClientPlayerArrived = false;

            _messagePool = new Dictionary<int, PlayerDeltaMessage>();
            _inverseMessagePool = new List<PlayerDeltaMessageTimeProxy>();

            OtherPlayers = new List<Player>(ServerPlayerManager.MAX_PLAYER_COUNT);
            for (int i = 0; i < ServerPlayerManager.MAX_PLAYER_COUNT; i++)
            {
                OtherPlayers.Add(null);
            }
            ActiveOtherPlayerSlots = new List<byte>();

            _otherPlayersMovementProcessor = new OtherPlayersMovementProcessor(this);

            CollectedMovementChanges = new List<NetworkPlayerDescription>();
        }

        public void PreInitialize(World world)
        {
            World = world;

            // This one is for loading client player. It is overwritten to null after loading screen.
            MessageProcessor.OnPlayerInitialization += OnPlayerInitialization;
        }

        public void Initialize(World world)
        {
            MessageProcessor.OnServerPlayerDelta += OnServerPlayerDelta;
            MessageProcessor.OnServerOtherPlayerDelta += OnServerOtherPlayerDelta;
            // This one is for loading other players.
            MessageProcessor.OnPlayerInitialization += OnPlayerInitialization;
            MessageProcessor.OnOtherPlayerDisconnect += OnOtherPlayerDisconnect;

            Messenger.On("PlayerMovementChanged", OnPlayerMovementChanged);
            Messenger.On("PlayerJumped", OnPlayerJumped);
        }

        public void SpawnMainPlayer(SpawnPlayerParams paramz)
        {
            RequestPlayersFromServer();

            while (_hasClientPlayerArrived == false)
            {
                Thread.Sleep(200);
            }
        }

        private void RequestPlayersFromServer()
        {
            var requestMessage = new PlayerInitializationRequestMessage();
            requestMessage.Send(NetworkManager);
        }

        private void OnPlayerMovementChanged()
        {
            CollectedMovementChanges.Add(ClientPlayer.PlayerDescription);
        }

        private void OnPlayerJumped()
        {
            CollectedMovementChanges.Add(
                new NetworkPlayerDescription
                {
                    JumpOccured = true,
                    Timestamp = NetTime.Now
                }
            );
        }

        private void OnPlayerInitialization(PlayerInitializationMessage message)
        {
            if (message.IsMe)
            {
                InitializeClientPlayer(message);
            }
            else
            {
                InitializeOtherPlayer(message);
            }
        }

        private void InitializeClientPlayer(PlayerInitializationMessage message)
        {
            ClientPlayer = new Player(World, message.Position)
            {
                UpDownRotation = message.UpDownRotation,
                LeftRightRotation = message.LeftRightRotation,
                // We always believe the server
                IsPlayerPositionFound = true,
                ServerSlot = message.Slot,
                Username = message.Username
            };

            ClientPlayer.Initialize();
            _clientPlayerShadowCopy = new Player(World, message.Position)
            {
                UpDownRotation = message.UpDownRotation,
                LeftRightRotation = message.LeftRightRotation,
                IsPlayerPositionFound = true,
                ServerSlot = message.Slot,
                Username = message.Username
            };

            // No initializing, because we don't want to subscribe to events

            _hasClientPlayerArrived = true;

            //            ClientPlayer.OnPlayerSecondaryAction += OnClientPlayerSecondaryAction;
            //            ClientPlayer.OnPlayerPrimaryAction += OnClientPlayerPrimaryAction;
            ClientPlayer.OnPlayerMovementBehaviourChanged += OnClientPlayerMovementBehaviourChanged;

            Messenger.On<string>("SelectedBlockChange", OnPlayerBlockTypeChange);
        }

        private void InitializeOtherPlayer(PlayerInitializationMessage message)
        {
            var player = new Player(World, message.Position, isClientPlayer: false)
            {
                UpDownRotation = message.UpDownRotation,
                LeftRightRotation = message.LeftRightRotation,
                // We always believe the server
                IsPlayerPositionFound = true,
                Username = message.Username,
                ServerSlot = message.Slot
            };

            OtherPlayers[message.Slot] = player;
            ActiveOtherPlayerSlots.Add(message.Slot);

            _otherPlayersMovementProcessor.OnPlayerAdd(message.Slot);
        }

        private void OnClientPlayerSecondaryAction()
        {
            OnClientPlayerToolUse(PlayerNetworkActionType.SecondaryAction);
        }

        private void OnClientPlayerPrimaryAction()
        {
            OnClientPlayerToolUse(PlayerNetworkActionType.PrimaryAction);
        }

        private void OnClientPlayerToolUse(PlayerNetworkActionType networkActionType)
        {
            var message = new PlayerSingleActionMessage(networkActionType)
            {
                Slot = ClientPlayer.ServerSlot
            };

            message.Send(NetworkManager);
        }

        private void OnClientPlayerMovementBehaviourChanged()
        {
            var message = new PlayerMovementBehaviourChangeMessage();
            message.Send(NetworkManager);
        }

        private void OnServerPlayerDelta(ServerPlayerDeltaMessage message)
        {
            if (_messagePool.ContainsKey(message.Index))
            {
                _lastAcceptedIndex = message.Index;

                _clientPlayerShadowCopy.Position = message.Position;

                List<int> keysToDiscard = (from m in _messagePool
                                           where m.Key < _lastAcceptedIndex
                                           select m.Key).ToList();

                foreach (int key in keysToDiscard)
                {
                    _messagePool.Remove(key);
                }

                RefillInverseMessagePool();

                foreach (PlayerDeltaMessageTimeProxy proxy in _inverseMessagePool)
                {
                    // I have a bug with arranging proxies, but was too lazy to fix the reason, so I fixed this.
                    if (proxy.PreviousTimestamp > proxy.Timestamp)
                    {
                        continue;
                    }
                    // This is not interpolation, but recomputing the current position based on previous inputs.
                    // Then we should proceed to interpolate between two values
                    _clientPlayerShadowCopy.ReconcilePosition((float)(proxy.Timestamp - proxy.PreviousTimestamp),
                                                   proxy.PlayerDescription);
                }
                Vector3 newPlayerPosition = _clientPlayerShadowCopy.Position;

                ClientPlayer.SetupInterpolation(newPlayerPosition);
            }
        }

        private void OnServerOtherPlayerDelta(ServerOtherPlayerDeltaMessage message)
        {
            Player player = OtherPlayers[message.Slot];

            if (player != null)
            {
                _otherPlayersMovementProcessor.UpdateWithMessage(message);
            }
        }

        private void RefillInverseMessagePool()
        {
            _inverseMessagePool.Clear();
            double previousTimestamp = PreviousTimeStamp;

            foreach (KeyValuePair<int, PlayerDeltaMessage> playerDeltaMessage in _messagePool.OrderByDescending(kv => kv.Key))
            {
                foreach (NetworkPlayerDescription networkPlayerDescription in playerDeltaMessage.Value.CollectedMovementChanges)
                {
                    var proxy = new PlayerDeltaMessageTimeProxy
                                {
                                    PlayerDescription = networkPlayerDescription,
                                    Timestamp = networkPlayerDescription.Timestamp,
                                    PreviousTimestamp = previousTimestamp
                                };
                    _inverseMessagePool.Add(proxy);
                    previousTimestamp = networkPlayerDescription.Timestamp;
                }
            }

            AddLastProxy();
        }

        private void AddLastProxy()
        {
            double currentTime = NetTime.Now;
            double previousTimestamp = _inverseMessagePool.Count > 0
                                           ? _inverseMessagePool.Last().PreviousTimestamp
                                           : PreviousTimeStamp;
            var lastProxy = new PlayerDeltaMessageTimeProxy
                            {
                                PlayerDescription = ClientPlayer.PlayerDescription,
                                Timestamp = currentTime,
                                PreviousTimestamp = previousTimestamp
                            };
            _inverseMessagePool.Add(lastProxy);
        }

        private void OnPlayerBlockTypeChange(string blockName)
        {
            BlockType blockType = BlockTypeHelper.Get(blockName);
            int key = blockType.Key;

            var message = new PlayerBlockTypeChangeMessage(key);
            message.Send(NetworkManager);
        }

        private void OnOtherPlayerDisconnect(PlayerDisconnectMessage message)
        {
            RemovePlayer(message.Slot);
        }

        private void RemovePlayer(byte slot)
        {
            Player player = OtherPlayers[slot];

            if (player != null)
            {
                OtherPlayers[slot] = null;
                ActiveOtherPlayerSlots.Remove(slot);

                player.Dispose();
            }
        }

        public void Update()
        {
            _index++;

            double now = NetTime.Now;

            if (CollectedMovementChanges.Count == 0)
            {
                CollectedMovementChanges.Add(ClientPlayer.PlayerDescription);
            }

            var message = new PlayerDeltaMessage(CollectedMovementChanges, now)
            {
                Index = _index,
                PreviousTimestamp = PreviousTimeStamp
            };

            //TODO: Would need to fix possible player hack of sending MaxInt
            PreviousTimeStamp = now;

            _messagePool.Add(_index, message);

            message.Send(NetworkManager);

            CollectedMovementChanges.Clear();
        }

        public void Update(GameTime gameTime)
        {
            if (ClientPlayer != null)
            {
                ClientPlayer.Update(gameTime);
            }

            _otherPlayersMovementProcessor.Update(gameTime);
        }

        public void UpdateStep(GameTime gameTime)
        {
        }

        internal string GetPlayerName(byte slot)
        {
            if (slot == ClientPlayer.ServerSlot)
            {
                return ClientPlayer.Username;
            }

            Player player = OtherPlayers[slot];
            if (player != null)
            {
                return player.Username;
            }

            return "";
        }

        public void Dispose()
        {
            if (_playerActionManager != null)
            {
                _playerActionManager.OnPlayerDidPrimaryAction -= OnClientPlayerPrimaryAction;
                _playerActionManager.OnPlayerDidSecondaryAction -= OnClientPlayerSecondaryAction;
            }

            ClientPlayer.Dispose();

            Messenger.Off<string>("SelectedBlockChange", OnPlayerBlockTypeChange);

            MessageProcessor.OnServerPlayerDelta -= OnServerPlayerDelta;
            MessageProcessor.OnServerOtherPlayerDelta -= OnServerOtherPlayerDelta;
            // This one is for loading other players.
            MessageProcessor.OnPlayerInitialization -= OnPlayerInitialization;
            MessageProcessor.OnOtherPlayerDisconnect -= OnOtherPlayerDisconnect;
        }
    }
}