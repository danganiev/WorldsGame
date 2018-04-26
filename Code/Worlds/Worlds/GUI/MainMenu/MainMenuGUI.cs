using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

namespace WorldsGame.GUI.MainMenu
{
    internal class MainMenuGUI : View.GUI.GUI
    {
        private ButtonControl _quitButton;
        private ButtonControl _settingsButton;
        private ButtonControl _worldSetupButton;
        private ButtonControl _playButton;

        internal MainMenuGUI(WorldsGame game)
            : base(game)
        {
        }

        protected override void CreateControls()
        {
            AddTitleAndVersion();
            AddQuitButton();
            AddSettingsButton();
            AddWorldSetupButton();
            AddPlayButton();
        }

        private void AddTitleAndVersion()
        {
            var title = new LabelControl
            {
                Text = "Worlds",
                Bounds = new UniRectangle(ButtonDistanceFromRight - 20, 40, 30, 100),
                IsTitle = true
            };

            var version = new LabelControl
            {
                Text = "Version: v8",
                Bounds = new UniRectangle(5, new UniScalar(1f, -25), 100, 30)
            };

            Screen.Desktop.Children.Add(title);
            Screen.Desktop.Children.Add(version);
        }

        //Buttons creation
        private void AddQuitButton()
        {
            // Button through which the user can quit the application
            _quitButton = new ButtonControl
            {
                Text = "Quit",
                Bounds = new UniRectangle(ButtonDistanceFromRight, ButtonDistanceFromBottom, 80, ButtonHeight)
            };
            _quitButton.Pressed += (sender, arguments) => Game.Exit();
            Screen.Desktop.Children.Add(_quitButton);
        }

        //Opens a playing sub menu
        private void AddSettingsButton()
        {
            _settingsButton = new ButtonControl
            {
                Text = "Settings",
                Bounds = new UniRectangle(_quitButton.Bounds.Left - 90, ButtonDistanceFromBottom, 80, ButtonHeight)
            };
            _settingsButton.Pressed += (sender, arguments) => SetSettingsScreen();
            Screen.Desktop.Children.Add(_settingsButton);
        }

        //Opens a World creation sub-menu
        private void AddWorldSetupButton()
        {
            _worldSetupButton = new ButtonControl
            {
                Text = "Setup worlds",
                Bounds = new UniRectangle(_settingsButton.Bounds.Left - 140, ButtonDistanceFromBottom, 130, ButtonHeight)
            };
            _worldSetupButton.Pressed += (sender, arguments) => SetWorldCreationScreen();
            Screen.Desktop.Children.Add(_worldSetupButton);
        }

        //Opens a playing sub menu
        private void AddPlayButton()
        {
            _playButton = new ButtonControl
            {
                Text = "Play!",
                Bounds = new UniRectangle(_worldSetupButton.Bounds.Left - 140, ButtonDistanceFromBottom, 130, ButtonHeight)
            };
            _playButton.Pressed += (sender, arguments) => SetNewGameScreen();
            Screen.Desktop.Children.Add(_playButton);
        }

        //World creation
        private void SetWorldCreationScreen()
        {
            var worldsListGUI = new WorldsListGUI(Game);
            MenuState.SetGUI(worldsListGUI);
        }

        private void SetNewGameScreen()
        {
            var newGameGUI = new NewGameGUI(Game);
            MenuState.SetGUI(newGameGUI);
        }

        private void SetSettingsScreen()
        {
            var settingsGUI = new SettingsGUI(Game);
            MenuState.SetGUI(settingsGUI);
        }
    }
}