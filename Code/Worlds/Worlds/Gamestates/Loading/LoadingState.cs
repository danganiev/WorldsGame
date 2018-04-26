using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using NLog;
using Nuclex.UserInterface;

using WorldsGame.GUI.Loading;
using WorldsGame.GUI.MainMenu;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.World;
using WorldsGame.Sound;
using WorldsGame.Terrain;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Gamestates
{
    internal class LoadingState : BaseLoadingState
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly WorldSettings _worldSettings;
        private readonly string _worldName;
        private readonly bool _isObjectCreationState;
        private readonly LoadingGUI _gui;

        private Timer _timer;
        private GameBundleCompiler _compiler;

        private Task _compileTask;
        private Task _bundleLoadTask;
        private Task _worldInitializingTask;

        private World _world;
        private WorldSave _worldSave;
        private CompiledGameBundle _bundle;

        private string _seed;
        private bool _isNewGame;
        private ServerSettings _serverSettings;
        private AudioManager _audioManager;

        private bool IsServer { get { return _serverSettings != null; } }

        private LoadingState(WorldsGame game)
            : base(game)
        {
            _gui = new LoadingGUI(Game);
        }

        // New game
        internal LoadingState(WorldsGame game, WorldSettings worldSettings, string worldName, bool isObjectCreationState = false, string seed = "")
            : this(game)
        {
            _worldSettings = worldSettings;
            _worldName = worldName;
            _worldSave = null;
            _isObjectCreationState = isObjectCreationState;
            _seed = seed;
            _isNewGame = true;
        }

        //Load game
        internal LoadingState(WorldsGame game, WorldSave worldSave)
            : this(game)
        {
            _worldSettings = null;
            _worldName = worldSave.Name;
            _worldSave = worldSave;
            _isObjectCreationState = false;
            // We absolutely have to initialize seed
            _seed = "";
            _isNewGame = false;
        }

        internal LoadingState(WorldsGame game, ServerSettings serverSettings)
            : this(game)
        {
            _serverSettings = serverSettings;
            _isNewGame = serverSettings.IsNewGame;

            if (_isNewGame)
            {
                _worldSettings = serverSettings.WorldSettings;
                _worldName = serverSettings.WorldName;
                _worldSave = null;
                _isObjectCreationState = false;
                _seed = serverSettings.Seed;
            }
            else
            {
                _worldSettings = null;
                _worldName = serverSettings.WorldSave.Name;
                _worldSave = serverSettings.WorldSave;
                _isObjectCreationState = false;
                // We absolutely have to initialize seed
                _seed = "";
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _gui.Start();
            _gui.MiddleText = "Started loading";
            Messenger.On<string>("LoadingMessageChange", message => _gui.MiddleText = message);

            _timer = new Timer(state => { _gui.AdviceBottomText = AdviceText.GetNewAdvice(); }, null, 5000, 5000);

            if (_isObjectCreationState)
            {
                ClearObjectCreationFiles();
            }

            if (_isNewGame)
            {
                InitializeCompilerAndLoad();
            }
            else
            {
                InitializeBundleLoadTask();
            }

            Graphics.PreferMultiSampling = false;
            Graphics.ApplyChanges();
        }

        private static void ClearObjectCreationFiles()
        {
            Messenger.Invoke("LoadingMessageChange", "Clearing old object creation files");

            WorldSave.DeleteWorldSave(WorldSave.OBJECT_CREATION_WORLD_NAME);
        }

        private void InitializeCompilerAndLoad()
        {
            _compiler = new GameBundleCompiler(_worldSettings, GraphicsDevice, Game, _worldName);
            if (_isObjectCreationState)
            {
                _compiler.CompileForObjectCreation = true;
            }

            _compileTask = Task.Factory.StartNew(CompileAndLoad);
        }

        private void CompileAndLoad()
        {
            _compiler.Compile();
            _bundle = _compiler.CompiledGameBundle;
            LoadAudio();
        }

        private void InitializeBundleLoadTask()
        {
            _bundleLoadTask = Task.Factory.StartNew(LoadEverything);
        }

        private void LoadEverything()
        {
            LoadBundle();
            LoadAudio();
        }

        private void LoadBundle()
        {
            CompiledGameBundleSave bundleSave = CompiledGameBundleSave.SaverHelper("").Load(_worldSave.FullName);
            _bundle = bundleSave.ToCompiledGameBundle(Content, GraphicsDevice);
        }

        private void LoadAudio()
        {
            _audioManager = new AudioManager(Game);
            _audioManager.Initialize();

            EffectSound.GetWorldSounds(_bundle, _audioManager);
        }

        private void InitializeWorld()
        {
            WorldType worldType = _isObjectCreationState
                                      ? WorldType.ObjectCreationWorld
                                      : IsServer ? WorldType.ServerWorld : WorldType.LocalWorld;

            _world = new World(
                GraphicsDevice, _bundle,
                worldType: worldType,
                seedBase: _seed, worldSave: _worldSave);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (_isNewGame && _compileTask != null)
            {
                if (CheckTaskForExceptions(gameTime, _compileTask) && _compileTask.IsCompleted && _compiler.IsCompileFinished)
                {
                    _compileTask.Dispose();
                    _compileTask = null;
                    //                    _bundle = _compiler.CompiledGameBundle;

                    _worldInitializingTask = Task.Factory.StartNew(InitializeWorld);
                }
            }
            if (!_isNewGame && _bundleLoadTask != null)
            {
                if (CheckTaskForExceptions(gameTime, _bundleLoadTask) && _bundleLoadTask.IsCompleted)
                {
                    _bundleLoadTask.Dispose();
                    _bundleLoadTask = null;

                    _worldInitializingTask = Task.Factory.StartNew(InitializeWorld);
                }
            }
            if (_worldInitializingTask != null && _worldInitializingTask.IsCompleted)
            {
                if (CheckTaskForExceptions(gameTime, _worldInitializingTask))
                {
                    _worldInitializingTask.Dispose();
                    _worldInitializingTask = null;
                    PlayTheGame();
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.LightGray);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        private void SetClearScreen()
        {
            var clearScreen = new Screen(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            Game.GUIManager.Screen = clearScreen;
        }

        private void PushPlayingState()
        {
            if (!IsServer)
            {
                Game.GameStateManager.Push(_isObjectCreationState
                                               ? new ObjectCreationState(Game, _bundle, _world)
                                               : new PlayingState(Game, _bundle, _world, _audioManager));
            }
            else
            {
                Game.GameStateManager.Push(new ServerState(Game, _bundle, _world, _serverSettings));
            }
        }

        protected override void ToMainMenu()
        {
            Game.GameStateManager.Pop();

            var menuState = (MenuState)Game.GameStateManager.ActiveState;
            var playGUI = new NewGameGUI(Game);
            menuState.SetGUI(playGUI);
        }

        private void PlayTheGame()
        {
            Messenger.Off<string>("LoadingMessageChange", null);
            SetClearScreen();
            Game.IsMouseVisible = false;
            PushPlayingState();
        }

        public override void Dispose()
        {
            base.Dispose();
            Messenger.Off<string>("LoadingMessageChange", null);
            _timer.Dispose();
            _gui.Dispose();

            if (_compileTask != null)
            {
                try
                {
                    _compileTask.Dispose();
                }
                catch (InvalidOperationException e)
                {
                    Logger.Error(string.Format("InvalidOperationException: {0}", e.ToString()));
                }
            }

            if (_worldInitializingTask != null)
            {
                try
                {
                    _worldInitializingTask.Dispose();
                }
                catch (InvalidOperationException e)
                {
                    Logger.Error(string.Format("InvalidOperationException: {0}", e.ToString()));
                }
            }

            if (_bundleLoadTask != null)
            {
                try
                {
                    _bundleLoadTask.Dispose();
                }
                catch (InvalidOperationException e)
                {
                    Logger.Error(string.Format("InvalidOperationException: {0}", e.ToString()));
                }
            }
        }
    }
}