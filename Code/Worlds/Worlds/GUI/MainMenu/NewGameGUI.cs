using System;
using System.Linq;
using System.Threading.Tasks;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI.MainMenu
{
    internal class NewGameGUI : View.GUI.GUI
    {
        private BaseWorldsTextControl _nameInput;
        private BaseWorldsTextControl _seedInput;
        private BaseWorldsTextControl _urlInput;

        private ListControl _worldsList;
        private ButtonControl _playButton;

        private PanelControl _loadFromURLPanel;

        private string _lastURL = "";

        protected override string LabelText { get { return "Start a new game"; } }

        protected override int LabelWidth { get { return 130; } }

        protected override bool IsBackable { get { return true; } }

        private static SaverHelper<WorldSettings> WorldsSaverHelper
        {
            get { return new SaverHelper<WorldSettings>(WorldSettings.StaticContainerName); }
        }

        private bool IsWorldSelected
        {
            get { return _worldsList.SelectedItems.Count != 0; }
        }

        private WorldSettings SelectedWorld
        {
            get
            {
                string selectedWorldName = _worldsList.Items[_worldsList.SelectedItems[0]];

                WorldSettings world = WorldsSaverHelper.Load(selectedWorldName);
                return world;
            }
        }

        internal NewGameGUI(WorldsGame game)
            : base(game)
        {
        }

        internal override void Start()
        {
            base.Start();
            Messenger.On("EscapeKeyPressed", Back);
            LoadDefaultWorlds();
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddNameInput();
            AddSeedInput();
            AddWorldTypesList();
            AddBottomButtons();
        }

        protected override void LoadData()
        {
            LoadList(_worldsList, WorldsSaverHelper);
        }

        private void AddNameInput()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, 30),
                Text = "Name:"
            };

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, 390, 30),
                Text = "New World"
            };

            BaseWorldsTextControls.Add(_nameInput);

            Screen.Desktop.Children.Add(nameLabel);
            Screen.Desktop.Children.Add(_nameInput);
        }

        private void AddSeedInput()
        {
            var Y = _nameInput.Bounds.Bottom + 10;

            var seedLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, 30),
                Text = "Seed:"
            };

            _seedInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(seedLabel.Bounds.Right + 10, Y, 390, 30),
            };

            BaseWorldsTextControls.Add(_seedInput);

            Screen.Desktop.Children.Add(seedLabel);
            Screen.Desktop.Children.Add(_seedInput);
        }

        private void AddWorldTypesList()
        {
            var Y = _seedInput.Bounds.Bottom + 10;

            var selectWorldTypeLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, 20),
                Text = "Select world type:"
            };

            _worldsList = new ListControl
            {
                Bounds = new UniRectangle(selectWorldTypeLabel.Bounds.Right + 10, Y, 230, 280),
                SelectionMode = ListSelectionMode.Single
            };
            _worldsList.SelectionChanged += (sender, args) => _playButton.Enabled = true;

            var loadLocalWorldsButton = new ButtonControl
            {
                Bounds = new UniRectangle(_worldsList.Bounds.Right + 10, Y, 150, ButtonHeight),
                Text = "Load local worlds"
            };
            loadLocalWorldsButton.Pressed += (sender, args) => StartLoadingTask();

            var loadFromURLButton = new ButtonControl
            {
                Bounds = new UniRectangle(_worldsList.Bounds.Right + 10, loadLocalWorldsButton.Bounds.Bottom + 10, 150, ButtonHeight),
                Text = "Load from URL"
            };
            loadFromURLButton.Pressed += (sender, args) => MakeURLPanel();

            listControls.Add(_worldsList);
            pressableControls.Add(loadLocalWorldsButton);
            pressableControls.Add(loadFromURLButton);

            Screen.Desktop.Children.Add(selectWorldTypeLabel);
            Screen.Desktop.Children.Add(_worldsList);
            Screen.Desktop.Children.Add(loadLocalWorldsButton);
            Screen.Desktop.Children.Add(loadFromURLButton);
        }

        private void AddBottomButtons()
        {
            var connectToServerButton = new ButtonControl
            {
                Bounds = new UniRectangle(backButton.Bounds.Left - 160, ButtonDistanceFromBottom, 150, ButtonHeight),
                Text = "Connect to server"
            };
            connectToServerButton.Pressed += (sender, args) => ToMultiplayerGUI();
            pressableControls.Add(connectToServerButton);

            var loadGameButton = new ButtonControl
            {
                Bounds = new UniRectangle(connectToServerButton.Bounds.Left - 130, ButtonDistanceFromBottom, 120, ButtonHeight),
                Text = "Load game"
            };
            loadGameButton.Pressed += (sender, args) => ToLoadGUI();

            pressableControls.Add(loadGameButton);

            _playButton = new ButtonControl
            {
                Bounds = new UniRectangle(loadGameButton.Bounds.Left - 110, ButtonDistanceFromBottom, 100, ButtonHeight),
                Text = "Play!",
                Enabled = false
            };
            _playButton.Pressed += (sender, args) => PlayTheGame();
            pressableControls.Add(_playButton);

            Screen.Desktop.Children.Add(_playButton);
            Screen.Desktop.Children.Add(loadGameButton);
            Screen.Desktop.Children.Add(connectToServerButton);
        }

        protected override void Back()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            var mainMenuGUI = new MainMenuGUI(Game);
            MenuState.SetGUI(mainMenuGUI);
        }

        private void SetClearScreen()
        {
            var clearScreen = new Screen(viewport.Width, viewport.Height);
            Game.GUIManager.Screen = clearScreen;
        }

        private void ToLoadGUI()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            var loadGameGUI = new LoadGameGUI(Game);
            MenuState.SetGUI(loadGameGUI);
        }

        private void ToMultiplayerGUI()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            var multiplayerGUI = new MultiplayerGUI(Game);
            MenuState.SetGUI(multiplayerGUI);
        }

        private void PushPlayingState()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            Game.ChangeState(new LoadingState(Game, SelectedWorld, _nameInput.Text != "" ? _nameInput.Text : "New world", seed: _seedInput.Text));
        }

        private void StartLoadingTask()
        {
            ShowMessageBox("Loading");
            GetLoadingTask();
        }

        private Task GetLoadingTask()
        {
            return Task.Factory.StartNew(LoadLocalWorlds).ContinueWith(task => LoadData()).ContinueWith(task => CancelAlertBox());
        }

        private void LoadLocalWorlds()
        {
            WorldLoader.LoadLocalWorlds();
        }

        private void LoadDefaultWorlds()
        {
            if (!SettingsManager.Settings.AreDefaultWorldsLoaded)
            {
                ShowMessageBox("Loading");
                GetLoadingTask().ContinueWith(task =>
                {
                    SettingsManager.Settings.AreDefaultWorldsLoaded = true;
                    SettingsManager.SaveSettings();
                });
            }
        }

        private void MakeURLPanel()
        {
            DisableControls();

            var alertPanelDistanceFromTop = (int)((Screen.Height - ALERT_PANEL_HEIGHT) / 2);
            var alertPanelDistanceFromLeft = (int)((Screen.Width - AlertPanelWidth) / 2);

            _loadFromURLPanel = new PanelControl
            {
                Bounds =
                    new UniRectangle(new UniScalar(0f, alertPanelDistanceFromLeft),
                                        new UniScalar(0f, alertPanelDistanceFromTop), AlertPanelWidth,
                                        ALERT_PANEL_HEIGHT)
            };

            int Y = 40;

            var urlLabel = new LabelControl("URL:")
            {
                Bounds = new UniRectangle(10, Y, 30, 30)
            };

            _urlInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(urlLabel.Bounds.Right + 10, Y, 390, 30),
                Text = _lastURL.Length > 0 ? _lastURL : "http://"
            };

            var cancelButton = new ButtonControl
            {
                Bounds = new UniRectangle(_urlInput.Bounds.Right - ButtonWidth, _urlInput.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
                Text = "Cancel"
            };
            cancelButton.Pressed += (sender, args) => CancelURLPanel();

            var loadButton = new ButtonControl
            {
                Bounds = new UniRectangle(cancelButton.Bounds.Left - ButtonWidth - 10, _urlInput.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
                Text = "Load"
            };
            loadButton.Pressed += (sender, args) => LoadFromURL();

            _loadFromURLPanel.Children.Add(urlLabel);
            _loadFromURLPanel.Children.Add(_urlInput);
            _loadFromURLPanel.Children.Add(loadButton);
            _loadFromURLPanel.Children.Add(cancelButton);

            Screen.Desktop.Children.Add(_loadFromURLPanel);
            _loadFromURLPanel.BringToFront();
        }

        private void LoadFromURL()
        {
            Task.Factory.StartNew(ActuallyLoadFromURL).ContinueWith(task => LoadData()).ContinueWith(task => CancelAlertBox());
        }

        private void ActuallyLoadFromURL()
        {
            _lastURL = _urlInput.Text;
            CancelURLPanel();
            ShowMessageBox("Downloading");
            WorldLoader.LoadFromURL(_lastURL);
        }

        private void CancelURLPanel()
        {
            _loadFromURLPanel.Bounds = new UniRectangle(-1000, -1000, 0, 0);

            foreach (Control control in _loadFromURLPanel.Children)
            {
                control.Bounds = new UniRectangle(0, 0, 0, 0);
            }
            _loadFromURLPanel = null;

            TaskHelper.Delay(30).ContinueWith(task => RemoveURLPanel());
            EnableControls();
        }

        private void RemoveURLPanel()
        {
            Screen.Desktop.Children.Remove(_loadFromURLPanel);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Please enter a name for a new world.");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in world name.");
                return false;
            }

            return true;
        }

        private void PlayTheGame()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            if (!IsWorldSelected)
            {
                ShowAlertBox("Please select a world type.");
                return;
            }

            SetClearScreen();
            Game.IsMouseVisible = false;
            PushPlayingState();
        }
    }
}