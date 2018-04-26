using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using WorldsGame.GUI.Loading;
using WorldsGame.GUI.MainMenu;
using WorldsGame.Network.Chat;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Playing.ConsoleCommands;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.PauseMenu;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.Renderers.Character;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Playing.Terrain.Chunks;
using WorldsGame.Renderers;
using WorldsGame.Sound;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Settings;
using WorldsGame.Utils.WorldsDebug;
using XNAGameConsole;

namespace WorldsGame.Gamestates
{
    internal class PlayingState : WorldsGameState
    {
        internal const int GAME_STEP_1000MS = 1000;
        internal const int GAME_STEP_50MS = 50;
        internal const int GAME_STEP_33MS = 33;

        protected SkyDomeRenderer skyDomeRenderer;
        protected DiagnosticWorldRenderer diagnosticWorldRenderer;
        protected SelectionBlockRenderer selectionBlockRenderer;
        protected ICharactersRenderer networkCharactersRenderer;
        protected PlayerGUIRenderer playerGUIRenderer;
        protected WorldRenderer worldRenderer;

        protected DebugPlayingKeyProcessor debugKeyProcessor;
        protected CompiledGameBundle compiledGameBundle;
        protected World world;
        protected PauseMenu pauseMenu;
        protected WorldsContentLoader worldsContentLoader;
        protected Chat chat;

        private Task SavingTask { get; set; }

        private LoadingGUI _finalizationGUI;
        private bool _isFinalizing = false;
        private IChunkThreadOperator _chunkThreadOperator;
        private SpriteBatch _spriteBatch;
        private Screen _screen;

        private double _previousUpdateTimeStep1000;
        private double _previousUpdateTimeStep50;
        private double _previousUpdateTimeStep33;

        // Managed with events, so not used
        private readonly AudioManager _audioManager;

        private bool IsNetworkState { get; set; }

        public string WorldSettingsName { get { return compiledGameBundle.WorldSettingsName; } }

        private ClientNetworkManager NetworkManager { get; set; }

        private ClientMessageProcessor MessageProcessor { get; set; }

        private GameConsole Console { get; set; }

        private bool IsDisconnecting { get; set; }

        private ClientPlayerManager PlayerManager { get; set; }

        private ClientPlayerActionManager PlayerActionManager { get; set; }

        private SinglePlayerInventoryManager PlayerInventoryManager { get; set; }

        protected EntityWorld EntityWorld { get; set; }

        internal PlayingState(WorldsGame game, CompiledGameBundle compiledGameBundle, World world, AudioManager audioManager)
            : base(game)
        {
            //            Console = game.GameConsole;
            this.compiledGameBundle = compiledGameBundle;
            this.world = world;
            _audioManager = audioManager;

            _screen = new Screen(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        }

        internal PlayingState(WorldsGame game, CompiledGameBundle compiledGameBundle, World world, ClientNetworkManager networkManager,
            ClientMessageProcessor messageProcessor, AudioManager audioManager)
            : this(game, compiledGameBundle, world, audioManager)
        {
            NetworkManager = networkManager;
            MessageProcessor = messageProcessor;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Not a method due to high demand
            IsNetworkState = world.WorldType == WorldType.NetworkWorld;

            Graphics.PreferMultiSampling = false;
            Graphics.ApplyChanges();

            InitializeEntitySystem();

            InitializeMainVariables();

            CreateRenderers();
            InitializeRenderers();

            StartChunkThreads();

            Messenger.On("ToggleMouseCentering", Game.InputController.ToggleMouseCentering);

            Messenger.Invoke("ToggleMouseCentering");

            InitializeConsole();
        }

        private void InitializeConsole()
        {
            if (world == null)
            {
                throw new InvalidOperationException("World can't be null at this point");
            }

            Game.InitializeConsole();
            Console = Game.GameConsole;

            //            GameConsole.ConsoleInstance.

            Console.AddCommand(new SpawnNPCCommand(world));
            Console.AddCommand(new AddItemCommand(world));
            Console.AddCommand(new SpawnItemCommand(world));
            Console.AddCommand(new SpawnBlockCommand(world));

            Console.ConsoleOpened += OnConsoleOpened;
            Console.ConsoleClosed += OnConsoleClosed;
        }

        private void OnConsoleClosed()
        {
            PlayerActionManager.Enabled = true;
        }

        private void OnConsoleOpened()
        {
            PlayerActionManager.Enabled = false;
        }

        // This is not in "World" class due to different player initialization routines when single/multiplayer
        private void InitializePlayerActionManager()
        {
            PlayerActionManager = new ClientPlayerActionManager();
            PlayerActionManager.SubscribeToInputs();
            world.ClientPlayer.PlayerActionManager = PlayerActionManager;
            chat.PlayerActionManager = PlayerActionManager;

            if (IsNetworkState)
            {
                PlayerManager.PlayerActionManager = PlayerActionManager;
            }
        }

        private void InitializeInventoryManager()
        {
            PlayerInventoryManager = new SinglePlayerInventoryManager(Game, world, _screen);

            PlayerInventoryManager.PlayerActionManager = PlayerActionManager;

            PlayerInventoryManager.OnInventoryOpened += OnInventoryOpened;
            PlayerInventoryManager.OnInventoryClosed += OnInventoryClosed;
        }

        private void OnInventoryOpened()
        {
            PlayerActionManager.Enabled = false;
            PlayerActionManager.ExceptionalToDisableKeys.Add(
                SettingsManager.ControlSettings.GetAllActions()[AllPlayerActions.InventoryToggle]);
        }

        private void OnInventoryClosed()
        {
            PlayerActionManager.Enabled = true;
            PlayerActionManager.ExceptionalToDisableKeys.Remove(
                SettingsManager.ControlSettings.GetAllActions()[AllPlayerActions.InventoryToggle]);
        }

        protected virtual void InitializeMainVariables()
        {
            debugKeyProcessor = new DebugPlayingKeyProcessor(Graphics, Game, world);
            pauseMenu = new PauseMenu(Game, world.WorldType, this, _screen);

            chat = new Chat(Graphics);
            chat.SayLineIsOn += ChatOnSayLineIsOn;
            chat.SayLineIsOff += ChatOnSayLineIsOff;

            if (IsNetworkState)
            {
                InitializeNetworkVariables();
            }

            world.Initialize();

            InitializePlayerActionManager();

            InitializeInventoryManager();
        }

        private void ChatOnSayLineIsOff()
        {
            PlayerActionManager.SubscribeToInputs();
        }

        private void ChatOnSayLineIsOn()
        {
            PlayerActionManager.UnsubscribeFromKeyboardInputs();
        }

        private void InitializeNetworkVariables()
        {
            NetworkManager.OnDisconnect += OnDisconnect;

            PlayerManager = (ClientPlayerManager)world.PlayerManager;

            world.ClientMessageProcessor = MessageProcessor;
            world.NetworkManager = NetworkManager;

            chat.ClientMessageProcessor = MessageProcessor;
            chat.NetworkManager = NetworkManager;
            chat.ClientPlayerManager = PlayerManager;
        }

        protected virtual void CreateRenderers()
        {
            selectionBlockRenderer = new SelectionBlockRenderer(GraphicsDevice, world.ClientPlayer);
            playerGUIRenderer = new PlayerGUIRenderer(GraphicsDevice, world);
            diagnosticWorldRenderer = new DiagnosticWorldRenderer(GraphicsDevice, world);
            skyDomeRenderer = new SkyDomeRenderer(GraphicsDevice, world);
            worldRenderer = new WorldRenderer(GraphicsDevice, world);

            if (IsNetworkState)
            {
                networkCharactersRenderer = new NetworkCharactersRenderer(GraphicsDevice, PlayerManager);
            }
            else
            {
                networkCharactersRenderer = new EmptyCharactersRenderer();
            }
        }

        private void StartChunkThreads()
        {
            _chunkThreadOperator = IsNetworkState
                                       ? new NetworkChunkThreadOperator(world, NetworkManager, MessageProcessor)
                                       : (IChunkThreadOperator)new ChunkThreadOperator(world);

            _chunkThreadOperator.Start();
        }

        internal void StopChunkThreads()
        {
            _chunkThreadOperator.Stop();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            ProcessDebugKeys(gameTime);

            if (_isFinalizing)
            {
                ToMainMenu();
                return;
            }

            if (Paused && !IsNetworkState)
            {
                return;
            }

            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;

            if (IsNetworkState)
            {
                MessageProcessor.Update(gameTime);

                if (IsDisconnecting)
                {
                    StartFinalization("You were disconnected from server.");
                }
            }

            if (totalMilliseconds - _previousUpdateTimeStep33 >= GAME_STEP_33MS)
            {
                if (IsNetworkState)
                {
                    PlayerManager.Update();
                }
                _previousUpdateTimeStep33 = totalMilliseconds;
            }

            if (totalMilliseconds - _previousUpdateTimeStep50 >= GAME_STEP_50MS)
            {
                chat.Update(gameTime);
                _previousUpdateTimeStep50 = totalMilliseconds;
            }
            if (totalMilliseconds - _previousUpdateTimeStep1000 >= GAME_STEP_1000MS)
            {
                world.Update1000(gameTime);
                _previousUpdateTimeStep1000 = totalMilliseconds;
            }

            world.Update(gameTime);
            EntityWorld.Update(gameTime);
        }

        private void ToMainMenu()
        {
            if (SavingTask == null || SavingTask.IsCompleted)
            {
                UnloadContent();
                Dispose();

                // To loading state
                Game.GameStateManager.Pop();
                // To main menu state
                Game.GameStateManager.Pop();

                var state = (MenuState)Game.GameStateManager.ActiveState;
                var mainGUI = new MainMenuGUI(Game);
                state.SetGUI(mainGUI);
            }
        }

        internal void StartFinalization(string finalizationText = "")
        {
            _isFinalizing = true;
            _finalizationGUI = new LoadingGUI(Game);
            _finalizationGUI.Start();
            _finalizationGUI.MiddleText = finalizationText != "" ? finalizationText : IsNetworkState ? "Disconnecting..." : "Saving everything...";

            StopChunkThreads();

            Messenger.Off("EscapeKeyPressed");
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_isFinalizing)
            {
                Game.GraphicsDevice.Clear(Color.LightGray);
                return;
            }

            Game.GraphicsDevice.Clear(Color.Black);

            skyDomeRenderer.Draw(gameTime);

            // The line below is still magic to me (seems like z-buffer is automatically cleared each draw call)
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            networkCharactersRenderer.Draw(gameTime);
            worldRenderer.Draw(gameTime);

            if (SettingsManager.Settings.DiagnosticMode)
                diagnosticWorldRenderer.Draw(gameTime);

            selectionBlockRenderer.Draw(gameTime);
            EntityWorld.Draw(gameTime);
            playerGUIRenderer.Draw(gameTime);
            AdditionalDraw(gameTime);

            Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            worldRenderer.DrawTransparent(gameTime);
            selectionBlockRenderer.DrawTransparent(gameTime);
            networkCharactersRenderer.DrawTransparent(gameTime);

            AdditionalDrawTransparent(gameTime);

            TwoDimensionalDraw(gameTime);

            pauseMenu.Draw();
        }

        public void AfterDraw(GameTime gameTime)
        {
            TwoDimensionalAfterDraw(gameTime);
        }

        private void TwoDimensionalAfterDraw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            PlayerInventoryManager.DrawAfterGUI(gameTime, _spriteBatch);

            _spriteBatch.End();
        }

        protected virtual void AdditionalDrawTransparent(GameTime gameTime)
        {
        }

        protected virtual void AdditionalDraw(GameTime gameTime)
        {
        }

        private void TwoDimensionalDraw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            chat.Draw(gameTime, _spriteBatch);
            PlayerInventoryManager.Draw(gameTime, _spriteBatch);

            _spriteBatch.End();
        }

        private void ProcessDebugKeys(GameTime gameTime)
        {
            //#if DEBUG
            debugKeyProcessor.Process(gameTime);
            //#endif
        }

        protected override void OnPause()
        {
            base.OnPause();
            pauseMenu.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();
            pauseMenu.Stop();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            worldsContentLoader = new WorldsContentLoader(compiledGameBundle, Content);

            EntityWorld.ComponentManager.AddConstant(new ContentLoaderComponent(worldsContentLoader));

            worldsContentLoader.LoadContent();

            worldRenderer.LoadContent(Content, worldsContentLoader);
            diagnosticWorldRenderer.LoadContent(Content, worldsContentLoader);
            skyDomeRenderer.LoadContent(Content, worldsContentLoader);
            selectionBlockRenderer.LoadContent(Content, worldsContentLoader);
            playerGUIRenderer.LoadContent(Content, worldsContentLoader);
            networkCharactersRenderer.LoadContent(Content, worldsContentLoader);

            chat.LoadContent(Content);
            PlayerInventoryManager.LoadContent(Content);
        }

        protected override void OnLeaving()
        {
            // This is needed if game exits straight to desktop
            if (SavingTask != null && !SavingTask.IsCompleted)
            {
                SavingTask.Wait();
            }

            base.OnLeaving();
        }

        private void OnDisconnect()
        {
            IsDisconnecting = true;
        }

        protected void InitializeRenderers()
        {
            selectionBlockRenderer.Initialize();
            playerGUIRenderer.Initialize();
            worldRenderer.Initialize();
            diagnosticWorldRenderer.Initialize();
            skyDomeRenderer.Initialize();
            networkCharactersRenderer.Initialize();

            Messenger.On("EscapeKeyPressed", OnEscape);

            Game.OnAfterDraw += AfterDraw;
        }

        private void InitializeEntitySystem()
        {
            EntityWorld = new EntityWorld(world);
            world.EntityWorld = EntityWorld;
        }

        private void OnEscape()
        {
            if (Paused)
            {
                Resume();
            }
            else if (chat.IsSayLineOn)
            {
                chat.ToggleSayLineOff();
            }
            else if (PlayerInventoryManager.IsInventoryOpened)
            {
                PlayerInventoryManager.CloseInventory();
            }
            else
            {
                if (world.WorldType == WorldType.LocalWorld && (SavingTask == null || SavingTask.IsCompleted))
                {
                    if (SavingTask != null)
                    {
                        SavingTask.Dispose();
                    }
                    SavingTask = Task.Factory.StartNew(() => world.Save());
                }

                Pause();
            }
        }

        protected override void UnloadContent()
        {
            if (worldsContentLoader != null)
            {
                worldsContentLoader.Unload();
            }
            Content.Unload();
        }

        private void DisposeRenderers()
        {
            selectionBlockRenderer.Dispose();
            skyDomeRenderer.Dispose();
            worldRenderer.Dispose();
            diagnosticWorldRenderer.Dispose();
            playerGUIRenderer.Dispose();
            networkCharactersRenderer.Dispose();
        }

        public override void Dispose()
        {
            Game.OnAfterDraw -= AfterDraw;

            if (Console != null)
            {
                Console.ConsoleClosed -= OnConsoleClosed;
                Console.ConsoleOpened -= OnConsoleOpened;

                Console.ClearCommands();
                Console.Dispose();
            }

            if (PlayerInventoryManager != null)
            {
                PlayerInventoryManager.OnInventoryClosed -= OnInventoryClosed;
                PlayerInventoryManager.OnInventoryOpened -= OnInventoryOpened;
            }

            if (IsNetworkState)
            {
                NetworkDispose();
            }

            DisposeRenderers();

            pauseMenu.Dispose();

            if (_chunkThreadOperator != null)
            {
                _chunkThreadOperator.Dispose();
            }

            world.Dispose();
            compiledGameBundle.Dispose();

            if (chat != null)
            {
                chat.Dispose();
            }

            Messenger.Clear();
            BlockTypeHelper.Clear();
            ItemHelper.Clear();

            base.Dispose();
        }

        private void NetworkDispose()
        {
            NetworkManager.Disconnect();
            MessageProcessor.Dispose();
            PlayerManager.Dispose();
            PlayerActionManager.Dispose();
        }
    }
}