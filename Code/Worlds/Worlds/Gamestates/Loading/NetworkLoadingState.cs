using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nuclex.UserInterface;
using WorldsGame.GUI.Loading;
using WorldsGame.GUI.MainMenu;
using WorldsGame.Network;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Network.Message.ServerAuthorization;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Players;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Sound;
using WorldsGame.Terrain;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Gamestates
{
    internal class NetworkLoadingState : BaseLoadingState
    {
        private readonly LoadingGUI _gui;

        private Timer _timer;
        private bool _isDisconnected;
        private Task _worldLoadingTask;
        private World _world;
        private CompiledGameBundle _bundle;
        private AudioManager _audioManager;

        private bool IsBundleDownloaded { get; set; }

        private ClientNetworkManager ClientNetworkManager { get; set; }

        private ClientMessageProcessor MessageProcessor { get; set; }

        private string Username { get; set; }

        private string BundleName { get; set; }

        private FileStreamingReceiver BundleReceiver { get; set; }

        private AtlasReceiver AtlasReceiver { get; set; }

        private int AtlasCount { get; set; }

        private int DownloadedAtlasesCount { get; set; }

        internal NetworkLoadingState(WorldsGame game, string url, string username, string port)
            : base(game)
        {
            _gui = new LoadingGUI(Game);
            ClientNetworkManager = new ClientNetworkManager(url, port);
            MessageProcessor = new ClientMessageProcessor(ClientNetworkManager, game.GameConsole);
            Username = username;
            _isDisconnected = false;
        }

        protected override void Initialize()
        {
            _gui.Start();
            _gui.MiddleText = "Started loading";
            Messenger.On<string>("LoadingMessageChange", message => _gui.MiddleText = message);

            _timer = new Timer(state => { _gui.AdviceBottomText = AdviceText.GetNewAdvice(); }, null, 5000, 5000);

            Graphics.PreferMultiSampling = false;
            Graphics.ApplyChanges();

            IsBundleDownloaded = false;
            BundleName = "";

            BundleReceiver = new FileStreamingReceiver();
            AtlasReceiver = null;

            MessageProcessor.OnDisconnect += OnDisconnect;
            MessageProcessor.OnApprove += OnApprove;
            MessageProcessor.OnFileStreamingStart += BundleReceiver.StartReceiving;
            MessageProcessor.OnFileStreamingData += BundleReceiver.Receive;

            // This one is not in the "BundleReceiver.DownloadFinished" because we don't know atlas count this far.
            MessageProcessor.OnAtlasesCountAnswer += OnAtlasCount;
            MessageProcessor.OnAtlasStreamingStart += OnAtlasStreamingStart;

            BundleReceiver.DownloadStarted += size => Messenger.Invoke(
                "LoadingMessageChange", string.Format("Downloading bundle... Progress: 0 KB / {0} KB", size / 1024));

            BundleReceiver.DownloadProgress += (size, fullsize) => Messenger.Invoke(
                "LoadingMessageChange", string.Format("Downloading bundle... Progress: {0} KB / {1} KB", size / 1024, fullsize / 1024));

            BundleReceiver.DownloadFinished += OnBundleDownloaded;

            _worldLoadingTask = null;

            Authorize();
        }

        private void OnDisconnect(string reason)
        {
            DelayedDisconnect(string.Format("You have been disconnected by server. Reason: {0}", reason));
        }

        private void DelayedDisconnect(string message)
        {
            Messenger.Invoke("LoadingMessageChange", string.Format("{0} You'll be returned to previous menu in 5 seconds.", message));
            TaskHelper.Delay(5000).ContinueWith(task => { _isDisconnected = true; });
        }

        private void OnApprove(GameMessageType messageType)
        {
            if (messageType == GameMessageType.StandartHailMessage)
            {
                RequestGameBundle();
            }
        }

        private void OnAtlasCount(AtlasCountMessage message)
        {
            AtlasCount = message.AtlasCount;
            StartNextAtlasReceiver();
        }

        private void StartNextAtlasReceiver()
        {
            AtlasReceiver = new AtlasReceiver(ClientNetworkManager);
            AtlasReceiver.InitializeReceiver(DownloadedAtlasesCount);
            AtlasReceiver.DownloadStarted += size => Messenger.Invoke(
                "LoadingMessageChange", string.Format("Downloading atlas {0}... Progress: 0 KB / {1} KB", DownloadedAtlasesCount + 1, size / 1024));

            AtlasReceiver.DownloadProgress += (size, fullsize) => Messenger.Invoke(
                "LoadingMessageChange",
                string.Format("Downloading atlas {0}... Progress: {1} KB / {2} KB", DownloadedAtlasesCount + 1, size / 1024, fullsize / 1024));
            AtlasReceiver.DownloadFinished += OnAtlasDownloaded;
        }

        private void OnBundleDownloaded(string filename)
        {
            Messenger.Invoke("LoadingMessageChange", "Bundle downloaded! Loading it now.");

            TakeBundleName(filename);
            MoveDownloadedFile(filename);
            RequestAtlasCount(filename);
        }

        private void TakeBundleName(string filename)
        {
            var fileNameArray = filename.Split('.').ToList();
            var fileNameQuery = fileNameArray.Take(fileNameArray.Count - 1);
            BundleName = string.Join(".", fileNameQuery);
        }

        private void OnAtlasDownloaded(string filename)
        {
            MoveDownloadedFile(filename, isAtlas: true);
            DownloadedAtlasesCount++;

            if (DownloadedAtlasesCount < AtlasCount)
            {
                MessageProcessor.OnFileStreamingData -= AtlasReceiver.Receive;
                AtlasReceiver.Dispose();
                StartNextAtlasReceiver();
            }
            else
            {
                OnEverythingDownloaded();
            }
        }

        private void OnEverythingDownloaded()
        {
            _worldLoadingTask = Task.Factory.StartNew(LoadBundleAndWorld);
        }

        private void OnAtlasStreamingStart(FileStreamStartMessage message)
        {
            MessageProcessor.OnFileStreamingData -= BundleReceiver.Receive;
            MessageProcessor.OnFileStreamingData += AtlasReceiver.Receive;

            AtlasReceiver.StartReceiving(message);
        }

        private void LoadBundleAndWorld()
        {
            Messenger.Invoke("LoadingMessageChange", "Loading bundle");

            CompiledGameBundleSave bundleSave = CompiledGameBundleSave.SaverHelper("", BundleType.Network).Load(BundleReceiver.FileName);

            _bundle = bundleSave.ToCompiledGameBundle(Content, GraphicsDevice, bundleType: BundleType.Network);

            _world = new World(GraphicsDevice, _bundle, worldType: WorldType.NetworkWorld)
            {
                ClientMessageProcessor = MessageProcessor,
                NetworkManager = ClientNetworkManager
            };

            _world.InitializeChunks(null);

            LoadAudio();

            Messenger.Invoke("LoadingMessageChange", "Loading player information");

            InitializePlayerManagerAndLoadPlayer();
        }

        private void LoadAudio()
        {
            _audioManager = new AudioManager(Game);
            _audioManager.Initialize();
            throw new NotImplementedException();
        }

        private void InitializePlayerManagerAndLoadPlayer()
        {
            var playerManager = new ClientPlayerManager(ClientNetworkManager, MessageProcessor);
            playerManager.PreInitialize(_world);
            _world.PlayerManager = playerManager;
            playerManager.SpawnMainPlayer(new SpawnPlayerParams());
        }

        private void Authorize()
        {
            Messenger.Invoke("LoadingMessageChange", "Connecting to server");

            if (!ClientNetworkManager.IsURLOK())
            {
                DelayedDisconnect("Wrong IP or URL.");
            }

            ClientNetworkManager.Connect(new ServerAuthorizationRequestMessage(Username));
        }

        public override void Update(GameTime gameTime)
        {
            if (_isDisconnected)
            {
                ToMainMenu();
                return;
            }

            if (_worldLoadingTask != null)
            {
                if (CheckTaskForExceptions(gameTime, _worldLoadingTask) && _worldLoadingTask.IsCompleted)
                {
                    _worldLoadingTask.Dispose();
                    _worldLoadingTask = null;
                    PlayTheGame();
                    return;
                }
            }

            MessageProcessor.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.LightGray);
        }

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
            Game.GameStateManager.Push(new PlayingState(Game, _bundle, _world, ClientNetworkManager, MessageProcessor, _audioManager));
        }

        protected override void ToMainMenu()
        {
            Game.GameStateManager.Pop();

            var menuState = (MenuState)Game.GameStateManager.ActiveState;
            var multiplayerGUI = new MultiplayerGUI(Game);
            menuState.SetGUI(multiplayerGUI);
        }

        private void MoveDownloadedFile(string fileName, bool isAtlas = false)
        {
            string bundlePath = Path.Combine(Constants.TEMP_DIR_NAME, fileName);
            string additionalContainerName = isAtlas ? BundleName : "";
            string networkBundlesPath = CompiledGameBundleSave.GetFullFilePath(fileName, BundleType.Network, isAtlas: isAtlas, additionalContainerName: additionalContainerName);

            var bundleFile = new FileInfo(bundlePath);

            Directory.CreateDirectory(Path.GetDirectoryName(networkBundlesPath));

            if (!File.Exists(networkBundlesPath))
            {
                bundleFile.MoveTo(networkBundlesPath);
            }
            else
            {
                // This check needs to be done BEFORE the downloading too
                // And hashsum needs to be checked if file sizes are equal
                var oldBundleFile = new FileInfo(networkBundlesPath);
                if (oldBundleFile.Length != bundleFile.Length)
                {
                    oldBundleFile.Delete();
                    bundleFile.MoveTo(networkBundlesPath);
                }
            }

            IsBundleDownloaded = true;
        }

        private void RequestGameBundle()
        {
            Messenger.Invoke("LoadingMessageChange", "Connected to server. Requesting game bundle.");

            var bundleRequestMessage = new GameBundleRequestMessage();
            ClientNetworkManager.SendMessage(bundleRequestMessage);
        }

        private void RequestAtlasCount(string bundleName)
        {
            Messenger.Invoke("LoadingMessageChange", "Requesting atlases.");

            var atlasesRequestMessage = new AtlasesRequestMessage(true, false, 0);
            ClientNetworkManager.SendMessage(atlasesRequestMessage);
        }

        private void PlayTheGame()
        {
            MessageProcessor.ResetEvents();

            Messenger.Off<string>("LoadingMessageChange", null);
            SetClearScreen();
            Game.IsMouseVisible = false;
            PushPlayingState();
        }

        public override void Dispose()
        {
            base.Dispose();
            Messenger.Off<string>("LoadingMessageChange", null);

            MessageProcessor.Dispose();
            BundleReceiver.Dispose();
            ClientNetworkManager.Dispose();
            _timer.Dispose();
        }
    }
}