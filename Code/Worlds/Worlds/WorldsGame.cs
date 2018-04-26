#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion License

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using Nuclex.Game.States;

using Nuclex.Input;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Visuals.Flat;

using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Exceptions;
using WorldsGame.Utils.Profiling;

using XNAGameConsole;

namespace WorldsGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class WorldsGame : Game
    {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isRestarted;
        private SpriteBatch _consoleSpriteBatch;

        internal GameStateManager GameStateManager { get; set; }

        internal GraphicsDeviceManager Graphics { get; set; }

        internal GuiManager GUIManager { get; private set; }

        internal InputManager Input { get; private set; }

        internal InputController InputController { get; private set; }

        internal bool IsServer { get; private set; }

        internal ServerSettings ServerSettings { get; set; }

        internal GameConsole GameConsole { get; private set; }

        internal event Action<GameTime> OnAfterDraw = gameTime => { };

        private static SaverHelper<WorldSettings> WorldsSaverHelper
        {
            get { return new SaverHelper<WorldSettings>(WorldSettings.StaticContainerName); }
        }

        public WorldsGame(bool isRestarted = false)
        {
            Graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = SettingsManager.Settings.ResolutionHeight,
                PreferredBackBufferWidth = SettingsManager.Settings.ResolutionWidth,
                PreferMultiSampling = false,
                IsFullScreen = false,
            };

            Content.RootDirectory = "Content";
            Graphics.SynchronizeWithVerticalRetrace = true;

            // If developing on virtual machine this should be on
            //GraphicsAdapter.UseReferenceDevice = true;

            IsMouseVisible = true;
            _isRestarted = isRestarted;

            Constants.SaveGamesFolder = Constants.SINGLEPLAYER_SAVE_FOLDER_NAME;
        }

        public WorldsGame(ServerSettings settings)
            : this()
        {
            IsServer = true;
            InternalSystemSettings.IsServer = IsServer;
            ServerSettings = settings;

            Constants.SaveGamesFolder = Constants.SERVER_SAVE_FOLDER_NAME;
        }

        private void AddGUIManager()
        {
            GUIManager = new GuiManager(Services);
            Components.Add(GUIManager);
        }

        private void AddInputManager()
        {
            Input = new InputManager(Services, Window.Handle);
            Components.Add(Input);
        }

        private void AddGameStateManager()
        {
            GameStateManager = new GameStateManager { DisposeDroppedStates = true };
        }

        private void AddFramerateComponent()
        {
            var frameRate = new FrameRateCounter(this) { DrawOrder = 1 };
            Components.Add(frameRate);
        }

        protected override void Initialize()
        {
            AddGameStateManager();
            AddGUIManager();
            AddInputManager();
            Messenger.Clear();

            if (!IsServer)
            {
                if (SettingsManager.Settings.IsFullScreen)
                {
                    Graphics.ToggleFullScreen();
                }

                Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                Graphics.ApplyChanges();

#if DEBUG
                AddFramerateComponent();
#endif
            }
            else
            {
                LoadAdditionalServerSettings();
            }

            InitializeInput();

            SetInitialGameState();

            //#if DEBUG
            //            InitializeConsole();
            //#else
            //            if (IsServer)
            //            {
            //                InitializeConsole();
            //            }
            //#endif

            base.Initialize();
        }

        private void InitializeInput()
        {
            InputController = new InputController(Graphics);

            Input.GetKeyboard().KeyPressed += InputController.KeyPressed;
            Input.GetKeyboard().KeyReleased += InputController.KeyReleased;
            Input.GetKeyboard().CharacterEntered += InputController.CharacterEntered;

            Input.GetMouse().MouseButtonPressed += InputController.MouseButtonPressed;
            Input.GetMouse().MouseButtonReleased += InputController.MouseButtonReleased;
            Input.GetMouse().MouseWheelRotated += InputController.MouseWheelRotated;
        }

        internal void InitializeConsole()
        {
            // Bear in mind that console has big problems with doing something like
            // Graphics.PreferMultiSampling = false; Graphics.ApplyChanges();
            // on change of gamestate. Throws GraphicsDevice ObjectDisposed exception

            _consoleSpriteBatch = new SpriteBatch(GraphicsDevice);
            // Really don't know why this was here
            //            Services.AddService(typeof(SpriteBatch), _consoleSpriteBatch);

            var commands = new IConsoleCommand[] { };
            GameConsole = new GameConsole(this, _consoleSpriteBatch, commands, new GameConsoleOptions
            {
                Font = Content.Load<SpriteFont>("Fonts/DefaultFont"),
                Prompt = ">",
                BackgroundColor = new Color(0, 0, 0, 125),
                FontColor = Color.LightGray,
                OpenOnWrite = false,
                RoundedCorner = Content.Load<Texture2D>("Textures/roundedCorner")
            });

            GameConsole.ConsoleInstance = GameConsole;

            Messenger.On<Keys>("KeyPressed", GameConsole.KeyDown);
            Messenger.On<char>("CharacterEntered", GameConsole.CharEntered);
        }

        internal void RemoveConsole()
        {
            _consoleSpriteBatch.End();
            _consoleSpriteBatch.Dispose();

            GameConsole.Dispose();
        }

        private void SetInitialGameState()
        {
            if (IsServer)
            {
                ChangeState(new LoadingState(this, ServerSettings));
            }
            else
            {
                GameStateManager.Push(new MenuState(this, isRestarted: _isRestarted));
            }
        }

        private void LoadAdditionalServerSettings()
        {
            if (ServerSettings.IsNewGame)
            {
                WorldSettings world = WorldsSaverHelper.Load(ServerSettings.WorldTypeName);
                ServerSettings.WorldSettings = world;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            try
            {
                // We are constrained to our own mouse handling because of 1st person view;
                InputController.Update();

                // TODO: Here be problems with multiplayer, need to check it out
                // Checking if game is active is BAD, i.e. you won't receive network files if you've minimized game to the tray.
                GameStateManager.Update(gameTime);

                base.Update(gameTime);
            }
            catch (GameRestartException)
            {
                GameStateManager.Dispose();
                throw;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GameStateManager.Draw(gameTime);

                base.Draw(gameTime);
                OnAfterDraw(gameTime);
            }
            catch (GameRestartException)
            {
                GameStateManager.Dispose();

                throw;
            }
            // Two exceptions of nuclex GUI
            catch (InvalidOperationException e)
            {
                // 1st is for some nuclex exception
                // 2nd is for strange bugs without stack trace when disconnecting from server. Probably nuclex too
                Logger.Log(LogLevel.Error, string.Format("InvalidOperationException: Either a nuclex or strange bugs: {0}", e.ToString()));
                if (!e.Message.Contains("Obtaining") && !e.Message.Contains("Texture2D") && !e.Message.Contains("Begin"))
                {
                    throw;
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
#if !DEBUG
                Logger.Log(LogLevel.Error, string.Format("ArgumentOutOfRangeException: {0}", e.ToString()));
#endif

                if (GameStateManager.ActiveState.GetType() != typeof(MenuState))
                {
                    throw;
                }
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            if (!IsServer)
            {
                GUIManager.Visualizer = FlatGuiVisualizer.FromFile(Services, @"Content\Skin\Suave\Suave.skin.xml");
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        internal void ChangeState(DrawableGameState state)
        {
            if (GameStateManager.ActiveState != null && GameStateManager.ActiveState.GetType() == typeof(MenuState))
            {
                ((MenuState)GameStateManager.ActiveState).OnPush();
            }

            GameStateManager.Push(state);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            GameStateManager.Dispose();

            base.OnExiting(sender, args);
        }
    }
}