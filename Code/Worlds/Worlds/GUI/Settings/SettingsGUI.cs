using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Utils;
using WorldsGame.Utils.Exceptions;

namespace WorldsGame.GUI.MainMenu
{
    internal class SettingsGUI : View.GUI.GUI
    {
        private const string VIEW_DISTANCE_FORMAT_TEXT = "View Distance: {0}";

        private ButtonControl _fullScreenButton;
        private ButtonControl _viewDistanceButton;
        private ButtonControl _applyButton;
        private ButtonControl _resolutionButton;
        private ButtonControl _controlsButton;

        private bool _isRestartRequired;
        private readonly bool _oldIsFullscreen;
        private bool _resolutionCancelRequested;

        private Task _resolutionCheckTask;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        private string _newViewDistance;

        private int _currentResolutionIndex;

        private readonly Dictionary<int, DisplayMode> _displayModes;

        protected override bool IsBackable { get { return true; } }

        protected override int ButtonDistanceFromTop { get { return 150; } }

        protected override int ButtonDistanceFromLeft { get { return 120; } }

        protected override int ButtonWidth { get { return 170; } }

        protected override int AlertPanelWidth { get { return 600; } }

        internal SettingsGUI(WorldsGame game)
            : base(game)
        {
            _displayModes = new Dictionary<int, DisplayMode>();
            _isRestartRequired = false;
            _newViewDistance = SettingsManager.Settings.ViewDistance;
            _oldIsFullscreen = SettingsManager.Settings.IsFullScreen;
            _currentResolutionIndex = 0;
            _resolutionCancelRequested = false;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal override void Start()
        {
            base.Start();

            CheckIfResolutionChanged();
        }

        internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_resolutionCancelRequested)
            {
                CancelResolutionChange();
            }
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddApplyButton();
            AddFullScreenButton();
            AddResolutionButton();
            AddViewDistanceButton();
            AddControlsButton();
        }

        protected override void LoadData()
        {
            base.LoadData();

            LoadPossibleResolutions();
        }

        private void CheckIfResolutionChanged()
        {
            if (SettingsManager.Settings.IsResolutionChanged)
            {
                ShowOKCancelAlertBox(AcceptResolutionChange, CancelResolutionChange, ResolutionChangeText(10));

                _cancellationToken = _cancellationTokenSource.Token;
                _resolutionCheckTask = Task.Factory.StartNew(CancelResolutionTimer, _cancellationToken);
            }
        }

        private void CancelResolutionTimer()
        {
            int tenSeconds = 10;
            while (tenSeconds > 0)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                alertBoxLabel.Text = ResolutionChangeText(tenSeconds);
                tenSeconds--;
                Thread.Sleep(1000);
            }

            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (tenSeconds == 0)
            {
                _resolutionCancelRequested = true;
            }
        }

        private void AcceptResolutionChange()
        {
            _cancellationTokenSource.Cancel();

            SettingsManager.Settings.IsResolutionChanged = false;
            SettingsManager.Settings.PreviousResolutionWidth = SettingsManager.Settings.ResolutionWidth;
            SettingsManager.Settings.PreviousResolutionHeight = SettingsManager.Settings.ResolutionHeight;

            SettingsManager.SaveSettings();
            CancelAlertBox();
        }

        private void CancelResolutionChange()
        {
            SettingsManager.Settings.ResolutionWidth = SettingsManager.Settings.PreviousResolutionWidth;
            SettingsManager.Settings.ResolutionHeight = SettingsManager.Settings.PreviousResolutionHeight;
            SettingsManager.Settings.IsResolutionChanged = false;
            SettingsManager.SaveSettings();

            throw new GameRestartException();
        }

        private string ResolutionChangeText(int seconds)
        {
            return "Some resolutions currently do not work in windowed mode.\n" +
                   "Please press OK if your resolution is fine, or Cancel to return to previous resolution.\n" +
                   string.Format("Resolution will change automatically in {0} seconds", seconds);
        }

        private void AddFullScreenButton()
        {
            _fullScreenButton = new ButtonControl
            {
                Text = FullscreenButtonText(),
                Bounds = new UniRectangle(ButtonDistanceFromLeft, ButtonDistanceFromTop, ButtonWidth, ButtonHeight)
            };

            _fullScreenButton.Pressed += (sender, args) => ChangeFullScreen();

            pressableControls.Add(_fullScreenButton);
            Screen.Desktop.Children.Add(_fullScreenButton);
        }

        private static string FullscreenButtonText()
        {
            return string.Format("Fullscreen: {0}", SettingsManager.Settings.IsFullScreen ? "Yes" : "No");
        }

        private static string ResolutionText(DisplayMode mode)
        {
            return ResolutionText(mode.Width, mode.Height);
        }

        private static string ResolutionText(int width, int height)
        {
            return string.Format("Resolution: {0}x{1}", width, height);
        }

        private void AddResolutionButton()
        {
            _resolutionButton = new ButtonControl
            {
                Text = ResolutionText(SettingsManager.Settings.ResolutionWidth, SettingsManager.Settings.ResolutionHeight),
                Bounds = new UniRectangle(ButtonDistanceFromLeft, _fullScreenButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _resolutionButton.Pressed += (sender, args) => ChangeResolution();

            pressableControls.Add(_resolutionButton);
            Screen.Desktop.Children.Add(_resolutionButton);
        }

        private void AddViewDistanceButton()
        {
            _viewDistanceButton = new ButtonControl
            {
                Text = _newViewDistance,
                Bounds = new UniRectangle(ButtonDistanceFromLeft, _resolutionButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            SetViewDistanceText();

            _viewDistanceButton.Pressed += (sender, args) => ChangeViewDistance();

            pressableControls.Add(_viewDistanceButton);
            Screen.Desktop.Children.Add(_viewDistanceButton);
        }

        private void AddControlsButton()
        {
            _controlsButton = new ButtonControl
            {
                Text = "Controls...",
                Bounds = new UniRectangle(ButtonDistanceFromLeft + ButtonWidth + 30, ButtonDistanceFromTop, ButtonWidth, ButtonHeight)
            };

            _controlsButton.Pressed += (sender, args) => ToControlsGUI();

            pressableControls.Add(_controlsButton);
            Screen.Desktop.Children.Add(_controlsButton);
        }

        private void ToControlsGUI()
        {
            var controlsGUI = new ControlsGUI(Game);
            MenuState.SetGUI(controlsGUI);
        }

        private void AddApplyButton()
        {
            _applyButton = new ButtonControl
            {
                Text = "Apply",
                Bounds = new UniRectangle(backButton.Bounds.Left - 110, ButtonDistanceFromBottom, 100, 30)
            };
            _applyButton.Pressed += (sender, args) => ApplySettings();

            pressableControls.Add(_applyButton);
            Screen.Desktop.Children.Add(_applyButton);
        }

        private void ChangeViewDistance()
        {
            switch (_newViewDistance)
            {
                case "Tiny":
                    _newViewDistance = "Short";
                    break;

                case "Short":
                    _newViewDistance = "Normal";
                    break;

                case "Normal":
                    _newViewDistance = "Large";
                    break;

                case "Large":
                    _newViewDistance = "Tiny";
                    break;
            }

            SettingsManager.Settings.ViewDistance = _newViewDistance;

            SetViewDistanceText();
        }

        private void SetViewDistanceText()
        {
            _viewDistanceButton.Text = string.Format(VIEW_DISTANCE_FORMAT_TEXT, _newViewDistance);
        }

        private void ChangeFullScreen()
        {
            SettingsManager.Settings.IsFullScreen = !SettingsManager.Settings.IsFullScreen;
            _fullScreenButton.Text = FullscreenButtonText();

            _isRestartRequired = true;
        }

        private void ChangeResolution()
        {
            if (_currentResolutionIndex == _displayModes.Count - 1)
            {
                _currentResolutionIndex = 0;
            }
            else
            {
                _currentResolutionIndex++;
            }
            _resolutionButton.Text = ResolutionText(_displayModes[_currentResolutionIndex]);
        }

        private void LoadPossibleResolutions()
        {
            int index = 0;
            _displayModes.Clear();

            int settingsWidth = SettingsManager.Settings.ResolutionWidth;
            int settingsHeight = SettingsManager.Settings.ResolutionHeight;

            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (mode.Width >= 1024 && mode.Height >= 768 && mode.Format == SurfaceFormat.Color)
                {
                    _displayModes.Add(index, mode);

                    if (mode.Height == settingsHeight && mode.Width == settingsWidth)
                    {
                        _currentResolutionIndex = index;
                    }

                    index++;
                }
            }
        }

        private void SetResolution()
        {
            DisplayMode mode = _displayModes[_currentResolutionIndex];

            if (SettingsManager.Settings.ResolutionWidth != mode.Width || SettingsManager.Settings.ResolutionHeight != mode.Height)
            {
                _isRestartRequired = true;

                SettingsManager.Settings.PreviousResolutionWidth = SettingsManager.Settings.ResolutionWidth;
                SettingsManager.Settings.PreviousResolutionHeight = SettingsManager.Settings.ResolutionHeight;
                SettingsManager.Settings.ResolutionWidth = mode.Width;
                SettingsManager.Settings.ResolutionHeight = mode.Height;
                SettingsManager.Settings.IsResolutionChanged = true;
            }
        }

        private void ApplySettings()
        {
            SetResolution();
            Force1024OnWindowed();

            SettingsManager.SaveSettings();

            if (_isRestartRequired)
            {
                throw new GameRestartException();
            }

            var mainMenuGUI = new MainMenuGUI(Game);
            MenuState.SetGUI(mainMenuGUI);
        }

        private void Force1024OnWindowed()
        {
            //Because some resolutions don't work on windowed mode, we force 1024x768 on going back to windowed mode.
            if (_oldIsFullscreen != SettingsManager.Settings.IsFullScreen && !SettingsManager.Settings.IsFullScreen)
            {
                SettingsManager.Settings.ResolutionWidth = 1024;
                SettingsManager.Settings.ResolutionHeight = 768;
            }
        }

        private void DropToDefaults()
        {
            SettingsManager.Settings.IsFullScreen = _oldIsFullscreen;
        }

        protected override void Back()
        {
            DropToDefaults();
            var mainMenuGUI = new MainMenuGUI(Game);
            MenuState.SetGUI(mainMenuGUI);
        }
    }
}