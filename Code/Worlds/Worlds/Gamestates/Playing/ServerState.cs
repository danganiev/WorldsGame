using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using WorldsGame.GUI.Loading;
using WorldsGame.Network;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Players;
using WorldsGame.Server;
using WorldsGame.Server.ConsoleCommands;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;

using XNAGameConsole;

namespace WorldsGame.Gamestates
{
    internal class ServerState : WorldsGameState
    {
        internal const int GAME_STEP_IN_MILLISECONDS = 50;
        internal const float GAME_STEP_IN_SECONDS = (float)GAME_STEP_IN_MILLISECONDS / 1000;

        private double PreviousUpdateTimeInMS { get; set; }

        private ServerSettings Settings { get; set; }

        private World World { get; set; }

        private CompiledGameBundle CompiledGameBundle { get; set; }

        private Task SavingTask { get; set; }

        private ServerMessageProcessor MessageProcessor { get; set; }

        private GameConsole GameConsole { get; set; }

        private LoadingGUI StatusGUI { get; set; }

        private ServerNetworkManager NetworkManager { get; set; }

        private ServerPlayerManager PlayerManager { get; set; }

        private BundleStreamingManager BundleStreamingManager { get; set; }

        private ChunkNetworkHandler ChunkNetworkHandler { get; set; }

        internal Vector3 SpawnPoint { get; set; }

        internal ServerState(WorldsGame game, CompiledGameBundle compiledGameBundle, World world, ServerSettings settings)
            : base(game)
        {
            GameConsole = game.GameConsole;
            CompiledGameBundle = compiledGameBundle;
            World = world;
            Settings = settings;
            StatusGUI = new LoadingGUI(game);

            NetworkManager = new ServerNetworkManager(settings.Port);
            MessageProcessor = new ServerMessageProcessor(NetworkManager, GameConsole);
            PlayerManager = new ServerPlayerManager(this, NetworkManager, MessageProcessor);
            BundleStreamingManager = new BundleStreamingManager(CompiledGameBundle, PlayerManager);

            ChunkNetworkHandler = new ChunkNetworkHandler(NetworkManager, World);

            SpawnPoint = new Vector3();
        }

        protected override void Initialize()
        {
            base.Initialize();

            NetworkManager.Start();
            BundleStreamingManager.Start();

            World.NetworkManager = NetworkManager;
            World.ServerMessageProcessor = MessageProcessor;
            World.PlayerManager = PlayerManager;

            MessageProcessor.OnChunkRequest += ChunkNetworkHandler.OnChunkRequest;

            World.Initialize();

            StatusGUI.Start();
            StatusGUI.MiddleText = "Server is working as expected. Press ~ to open console.";

            InitializeConsoleCommands();
        }

        private void InitializeConsoleCommands()
        {
            GameConsole.AddCommand(new SetSpawnPointCommand(this));
            GameConsole.AddCommand(new SaveCommand(this));
        }

        public override void Update(GameTime gameTime)
        {
            MessageProcessor.Update(gameTime);
            World.Update(gameTime);

            // This actually works fine, but I suspect only on fixed 60fps timing
            // UPD: didn't work fine, so I approached differently
            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;
            if (totalMilliseconds - PreviousUpdateTimeInMS >= GAME_STEP_IN_MILLISECONDS)
            {
                UpdateStep(gameTime);
                PreviousUpdateTimeInMS = totalMilliseconds;
            }
        }

        private void UpdateStep(GameTime gameTime)
        {
            MessageProcessor.UpdateStep(gameTime);
            PlayerManager.UpdateStep(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Game.GraphicsDevice.Clear(Color.LightGray);
        }

        protected override void OnLeaving()
        {
            Save();

            // This is needed if game exits straight to desktop
            if (SavingTask != null && !SavingTask.IsCompleted)
            {
                SavingTask.Wait();
            }

            base.OnLeaving();
        }

        internal void Save()
        {
            if (SavingTask == null || SavingTask.IsCompleted || SavingTask.IsCanceled || SavingTask.IsFaulted)
            {
                SavingTask = Task.Factory.StartNew(() => World.Save());
            }
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                World.Dispose();
                CompiledGameBundle.Dispose();
                BundleStreamingManager.Dispose();
                MessageProcessor.Dispose();
                NetworkManager.Dispose();

                Messenger.Clear();
                BlockTypeHelper.Clear();

                IsDisposed = true;
            }

            base.Dispose();
        }
    }
}