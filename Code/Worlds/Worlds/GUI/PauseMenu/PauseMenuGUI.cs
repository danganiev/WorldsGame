using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;

namespace WorldsGame.GUI
{
    internal class PauseMenuGUI : View.GUI.GUI
    {
        protected readonly PlayingState playingState;
        protected new ButtonControl backButton;
        protected ButtonControl toMainMenuButton;

        internal PauseMenuGUI(WorldsGame game, PlayingState playingState, Screen screen)
            : base(game, screen)
        {
            this.playingState = playingState;
        }

        protected override void CreateControls()
        {
            CreateBackButton();
            CreateToMainMenuButton();
            CreateQuitButton();
        }

        private void CreateBackButton()
        {
            backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(ButtonDistanceFromRight, ButtonDistanceFromBottom, 100, 32)
            };
            backButton.Pressed += (sender, arguments) => playingState.Resume();
            Screen.Desktop.Children.Add(backButton);
        }

        protected virtual void CreateToMainMenuButton()
        {
            toMainMenuButton = new ButtonControl
            {
                Text = "Exit to Main Menu",
                Bounds = new UniRectangle(backButton.Bounds.Left - 165, ButtonDistanceFromBottom, 155, 32)
            };
            toMainMenuButton.Pressed += (sender, arguments) => ToMainMenu();
            Screen.Desktop.Children.Add(toMainMenuButton);
        }

        private void CreateQuitButton()
        {
            var quitButton = new ButtonControl
            {
                Text = "Exit to Desktop",
                Bounds = new UniRectangle(toMainMenuButton.Bounds.Left - 160, ButtonDistanceFromBottom, 150, 32)
            };
            quitButton.Pressed += (sender, arguments) => Game.Exit();
            Screen.Desktop.Children.Add(quitButton);
        }

        protected virtual void ToMainMenu()
        {
            playingState.StartFinalization();
        }

        internal virtual void Stop()
        {
            Screen.Desktop.Children.Clear();
        }
    }
}