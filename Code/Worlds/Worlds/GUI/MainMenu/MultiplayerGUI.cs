using System;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI.MainMenu
{
    internal class MultiplayerGUI : View.GUI.GUI
    {
        private BaseWorldsTextControl _urlInput;
        private BaseWorldsTextControl _portInput;
        private BaseWorldsTextControl _usernameInput;

        protected override int LabelWidth { get { return 70; } }

        protected override bool IsBackable { get { return true; } }

        protected override string LabelText { get { return "Connect to server"; } }

        internal MultiplayerGUI(WorldsGame game)
            : base(game)
        {
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddURLInput();
            AddUsernameInput();
            AddBottomButtons();
        }

        private void AddURLInput()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, 30),
                Text = "URL or IP:"
            };

            _urlInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, 250, 30)
            };
            BaseWorldsTextControls.Add(_urlInput);

            var portLabel = new LabelControl
            {
                Bounds = new UniRectangle(_urlInput.Bounds.Right + 10, Y, 30, 30),
                Text = "Port:"
            };

            _portInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(portLabel.Bounds.Right + 10, Y, 60, 30),
                Text = "4815"
            };
            BaseWorldsTextControls.Add(_portInput);

            Screen.Desktop.Children.Add(nameLabel);
            Screen.Desktop.Children.Add(_urlInput);
            Screen.Desktop.Children.Add(portLabel);
            Screen.Desktop.Children.Add(_portInput);
        }

        private void AddUsernameInput()
        {
            var Y = _urlInput.Bounds.Bottom + 10;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, 30),
                Text = "Username:"
            };

            _usernameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, 250, 30)
            };

            BaseWorldsTextControls.Add(_usernameInput);

            Screen.Desktop.Children.Add(nameLabel);
            Screen.Desktop.Children.Add(_usernameInput);
        }

        private void AddBottomButtons()
        {
            var connectButton = new ButtonControl
            {
                Bounds = new UniRectangle(backButton.Bounds.Left - 160, ButtonDistanceFromBottom, 150, ButtonHeight),
                Text = "Connect"
            };
            connectButton.Pressed += (sender, args) => ConnectToServer();
            pressableControls.Add(connectButton);

            Screen.Desktop.Children.Add(connectButton);
        }

        private void ConnectToServer()
        {
            if (IsURLOK() && IsUsernameOK() && IsPortOK())
            {
                string url = _urlInput.Text;
                string username = _usernameInput.Text;
                string port = _portInput.Text;
                Game.GameStateManager.Push(new NetworkLoadingState(Game, url, username, port));
            }
        }

        private bool IsUsernameOK()
        {
            if (_usernameInput.Text == "")
            {
                ShowAlertBox("Please enter a username");
                return false;
            }

            return true;
        }

        private bool IsURLOK()
        {
            if (_urlInput.Text == "")
            {
                ShowAlertBox("Please enter a valid URL or IP address");
                return false;
            }

            return true;
        }

        private bool IsPortOK()
        {
            bool isOK = true;
            if (_portInput.Text == "")
            {
                isOK = false;
            }
            else
            {
                int port = 0;
                try
                {
                    port = Convert.ToInt32(_portInput.Text);
                }
                catch (OverflowException)
                {
                    isOK = false;
                }
                catch (FormatException)
                {
                    isOK = false;
                }

                if (port <= 1024 || port > 49151)
                {
                    isOK = false;
                }
            }

            if (!isOK)
            {
                ShowAlertBox("Please enter a valid port. Port must be a number from 1024 to 49151");
                return false;
            }

            return true;
        }

        protected override void Back()
        {
            var newGameGUI = new NewGameGUI(Game);
            MenuState.SetGUI(newGameGUI);
        }
    }
}