using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Utils;
using WorldsGame.Utils.Settings;

namespace WorldsGame.GUI.MainMenu
{
    internal enum Actions
    {
        None,
        MoveForward,
        MoveBack,
        StrafeLeft,
        StrafeRight,
        Jump,
        ChangeView,
        PrimaryAction,
        SecondaryAction,
        Say,
        Inventory
    }

    internal class ControlsGUI : View.GUI.GUI
    {
        private const string ONCLICK_TEXT = "Press it!";

        private ButtonControl _moveForwardButton;
        private ButtonControl _moveBackButton;
        private ButtonControl _strafeLeftButton;
        private ButtonControl _strafeRightButton;
        private ButtonControl _jumpButton;
        private ButtonControl _changeCameraViewButton;

        private ButtonControl _primaryActionButton;
        private ButtonControl _secondaryActionButton;
        private ButtonControl _sayButton;
        private ButtonControl _inventoryButton;

        private bool _isWaitingForKeyPress;
        private KeyboardState _previousState;
        private MouseState _previousMouseState;
        private Actions _awaitingAction = Actions.None;

        private Keys[] _excludedKeys = new[] { Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
            Keys.F22, Keys.F23, Keys.F24, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0 };

        protected override bool IsBackable { get { return true; } }

        protected override int LabelWidth { get { return 130; } }

        protected override int ButtonWidth { get { return 100; } }

        private int FirstRowButtonX { get { return ButtonDistanceFromLeft + LabelWidth; } }

        private int SecondRowButtonX { get { return SecondRowLabelX + LabelWidth; } }

        protected override int FirstRowLabelX { get { return ButtonDistanceFromLeft; } }

        protected int SecondRowLabelX { get { return FirstRowButtonX + ButtonWidth + 30; } }

        internal ControlsGUI(WorldsGame game)
            : base(game)
        {
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddMoveForwardButton();
            AddMoveBackButton();
            AddStrafeLeftButton();
            AddStrafeRightButton();
            AddJumpButton();
            AddCameraViewButton();

            AddPrimaryActionButton();
            AddSecondaryActionButton();
            AddSayButton();
            AddInventoryButton();
            AddInventoryNumButton();
        }

        private void AddMoveForwardButton()
        {
            var label = new LabelControl
            {
                Text = "Move forward:",
                Bounds =
                    new UniRectangle(FirstRowLabelX, ButtonDistanceFromTop, LabelWidth,
                                        ButtonHeight)
            };

            _moveForwardButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.MoveForward,
                Bounds = new UniRectangle(FirstRowButtonX, ButtonDistanceFromTop, ButtonWidth, ButtonHeight)
            };

            _moveForwardButton.Pressed += (sender, args) => SetMoveForward();

            pressableControls.Add(_moveForwardButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_moveForwardButton);
        }

        private void AddMoveBackButton()
        {
            var label = new LabelControl
            {
                Text = "Move back:",
                Bounds =
                    new UniRectangle(FirstRowLabelX, _moveForwardButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _moveBackButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.MoveBack,
                Bounds = new UniRectangle(FirstRowButtonX, _moveForwardButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _moveBackButton.Pressed += (sender, args) => SetMoveBack();

            pressableControls.Add(_moveBackButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_moveBackButton);
        }

        private void AddStrafeLeftButton()
        {
            var label = new LabelControl
            {
                Text = "Strafe left:",
                Bounds =
                    new UniRectangle(FirstRowLabelX, _moveBackButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _strafeLeftButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.StrafeLeft,
                Bounds = new UniRectangle(FirstRowButtonX, _moveBackButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _strafeLeftButton.Pressed += (sender, args) => SetStrafeLeft();

            pressableControls.Add(_strafeLeftButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_strafeLeftButton);
        }

        private void AddStrafeRightButton()
        {
            var label = new LabelControl
            {
                Text = "Strafe right:",
                Bounds =
                    new UniRectangle(FirstRowLabelX, _strafeLeftButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _strafeRightButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.StrafeRight,
                Bounds = new UniRectangle(FirstRowButtonX, _strafeLeftButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _strafeRightButton.Pressed += (sender, args) => SetStrafeRight();

            pressableControls.Add(_strafeRightButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_strafeRightButton);
        }

        private void AddJumpButton()
        {
            var label = new LabelControl
            {
                Text = "Jump:",
                Bounds =
                    new UniRectangle(FirstRowLabelX, _strafeRightButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _jumpButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.Jump,
                Bounds = new UniRectangle(FirstRowButtonX, _strafeRightButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _jumpButton.Pressed += (sender, args) => SetJump();

            pressableControls.Add(_jumpButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_jumpButton);
        }

        private void AddCameraViewButton()
        {
            var label = new LabelControl
            {
                Text = "Change view:",
                Bounds =
                    new UniRectangle(FirstRowLabelX, _jumpButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _changeCameraViewButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.ChangeCameraView,
                Bounds = new UniRectangle(FirstRowButtonX, _jumpButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _changeCameraViewButton.Pressed += (sender, args) => SetChangeView();

            pressableControls.Add(_changeCameraViewButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_changeCameraViewButton);
        }

        private void AddPrimaryActionButton()
        {
            var label = new LabelControl
            {
                Text = "Primary action:",
                Bounds =
                    new UniRectangle(SecondRowLabelX, ButtonDistanceFromTop, LabelWidth,
                                        ButtonHeight)
            };

            _primaryActionButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.PrimaryAction,
                Bounds = new UniRectangle(SecondRowButtonX, ButtonDistanceFromTop, ButtonWidth, ButtonHeight)
            };

            _primaryActionButton.Pressed += (sender, args) => SetPrimaryAction();

            pressableControls.Add(_primaryActionButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_primaryActionButton);
        }

        private void AddSecondaryActionButton()
        {
            var label = new LabelControl
            {
                Text = "Secondary action:",
                Bounds =
                    new UniRectangle(SecondRowLabelX, _primaryActionButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _secondaryActionButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.SecondaryAction,
                Bounds = new UniRectangle(SecondRowButtonX, _primaryActionButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _secondaryActionButton.Pressed += (sender, args) => SetSecondaryAction();

            pressableControls.Add(_secondaryActionButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_secondaryActionButton);
        }

        private void AddSayButton()
        {
            var label = new LabelControl
            {
                Text = "Say:",
                Bounds =
                    new UniRectangle(SecondRowLabelX, _secondaryActionButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _sayButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.Say,
                Bounds = new UniRectangle(SecondRowButtonX, _secondaryActionButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _sayButton.Pressed += (sender, args) => SetSay();

            pressableControls.Add(_sayButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_sayButton);
        }

        private void AddInventoryButton()
        {
            var label = new LabelControl
            {
                Text = "Inventory:",
                Bounds =
                    new UniRectangle(SecondRowLabelX, _sayButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            _inventoryButton = new ButtonControl
            {
                Text = SettingsManager.ControlSettings.Inventory,
                Bounds = new UniRectangle(SecondRowButtonX, _sayButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight)
            };

            _inventoryButton.Pressed += (sender, args) => SetInventory();

            pressableControls.Add(_inventoryButton);
            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_inventoryButton);
        }

        private void AddInventoryNumButton()
        {
            var label = new LabelControl
            {
                Text = "Item selection:",
                Bounds =
                    new UniRectangle(SecondRowLabelX, _inventoryButton.Bounds.Bottom + 10, LabelWidth,
                                        ButtonHeight)
            };

            var inventoryNumButton = new ButtonControl
            {
                Text = "0-9",
                Bounds = new UniRectangle(SecondRowButtonX, _inventoryButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
                Enabled = false
            };

            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(inventoryNumButton);
        }

        private void SetMoveForward()
        {
            _moveForwardButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.MoveForward;
        }

        private void SetMoveBack()
        {
            _moveBackButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.MoveBack;
        }

        private void SetStrafeLeft()
        {
            _strafeLeftButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.StrafeLeft;
        }

        private void SetStrafeRight()
        {
            _strafeRightButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.StrafeRight;
        }

        private void SetJump()
        {
            _jumpButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.Jump;
        }

        private void SetChangeView()
        {
            _changeCameraViewButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.ChangeView;
        }

        private void SetPrimaryAction()
        {
            _primaryActionButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.PrimaryAction;
        }

        private void SetSecondaryAction()
        {
            _secondaryActionButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.SecondaryAction;
        }

        private void SetSay()
        {
            _sayButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.Say;
        }

        private void SetInventory()
        {
            _inventoryButton.Text = ONCLICK_TEXT;
            _isWaitingForKeyPress = true;
            _awaitingAction = Actions.Inventory;
        }

        internal override void Update(GameTime gameTime)
        {
            WaitingForKeyLoop();
        }

        private void WaitingForKeyLoop()
        {
            if (!_isWaitingForKeyPress)
            {
                return;
            }

            KeyboardState currentState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            if ((currentState.GetPressedKeys().Length > 0 && _previousState.GetPressedKeys().Length == 0) ||
                (IsMouseButtonPressed(currentMouseState) && !IsMouseButtonPressed(_previousMouseState)))
            {
                if (IsMouseButtonPressed(currentMouseState))
                {
                    if (currentMouseState.LeftButton == ButtonState.Pressed)
                    {
                        SetPressedKey(Keys.F22);
                    }
                    else if (currentMouseState.RightButton == ButtonState.Pressed)
                    {
                        SetPressedKey(Keys.F23);
                    }
                    else if (currentMouseState.MiddleButton == ButtonState.Pressed)
                    {
                        SetPressedKey(Keys.F24);
                    }
                }
                else
                {
                    Keys pressedKey = currentState.GetPressedKeys().First();

                    if (pressedKey == Keys.Escape)
                    {
                        Cancel();
                        return;
                    }

                    if (_excludedKeys.Contains(pressedKey))
                    {
                        _previousState = currentState;
                        _previousMouseState = currentMouseState;
                        return;
                    }

                    SetPressedKey(pressedKey);
                }
            }
            else
            {
                _previousState = currentState;
                _previousMouseState = currentMouseState;
            }
        }

        private static bool IsMouseButtonPressed(MouseState currentMouseState)
        {
            return currentMouseState.LeftButton == ButtonState.Pressed || currentMouseState.RightButton == ButtonState.Pressed || currentMouseState.MiddleButton == ButtonState.Pressed;
        }

        private void SetPressedKey(Keys pressedKey)
        {
            _previousState = new KeyboardState();
            _previousMouseState = new MouseState();
            _isWaitingForKeyPress = false;

            string keyName = ControlSettings.ReverseKeyMap[pressedKey];

            switch (_awaitingAction)
            {
                case Actions.MoveForward:
                    _moveForwardButton.Text = keyName;
                    SettingsManager.ControlSettings.MoveForward = keyName;
                    break;

                case Actions.MoveBack:
                    _moveBackButton.Text = keyName;
                    SettingsManager.ControlSettings.MoveBack = keyName;
                    break;

                case Actions.StrafeLeft:
                    _strafeLeftButton.Text = keyName;
                    SettingsManager.ControlSettings.StrafeLeft = keyName;
                    break;

                case Actions.StrafeRight:
                    _strafeRightButton.Text = keyName;
                    SettingsManager.ControlSettings.StrafeRight = keyName;
                    break;

                case Actions.Jump:
                    _jumpButton.Text = keyName;
                    SettingsManager.ControlSettings.Jump = keyName;
                    break;

                case Actions.ChangeView:
                    _changeCameraViewButton.Text = keyName;
                    SettingsManager.ControlSettings.ChangeCameraView = keyName;
                    break;

                case Actions.PrimaryAction:
                    _primaryActionButton.Text = keyName;
                    SettingsManager.ControlSettings.PrimaryAction = keyName;
                    break;

                case Actions.SecondaryAction:
                    _secondaryActionButton.Text = keyName;
                    SettingsManager.ControlSettings.SecondaryAction = keyName;
                    break;

                case Actions.Say:
                    _sayButton.Text = keyName;
                    SettingsManager.ControlSettings.Say = keyName;
                    break;

                case Actions.Inventory:
                    _inventoryButton.Text = keyName;
                    SettingsManager.ControlSettings.Inventory = keyName;
                    break;
            }

            _awaitingAction = Actions.None;

            SettingsManager.SaveControlSettings();
        }

        private void Cancel()
        {
            _previousState = new KeyboardState();
            _previousMouseState = new MouseState();
            _isWaitingForKeyPress = false;

            switch (_awaitingAction)
            {
                case Actions.MoveForward:
                    _moveForwardButton.Text = SettingsManager.ControlSettings.MoveForward;
                    break;

                case Actions.MoveBack:
                    _moveBackButton.Text = SettingsManager.ControlSettings.MoveBack;
                    break;

                case Actions.StrafeLeft:
                    _strafeLeftButton.Text = SettingsManager.ControlSettings.StrafeLeft;
                    break;

                case Actions.StrafeRight:
                    _strafeRightButton.Text = SettingsManager.ControlSettings.StrafeRight;
                    break;

                case Actions.Jump:
                    _jumpButton.Text = SettingsManager.ControlSettings.Jump;
                    break;

                case Actions.ChangeView:
                    _changeCameraViewButton.Text = SettingsManager.ControlSettings.ChangeCameraView;
                    break;

                case Actions.PrimaryAction:
                    _primaryActionButton.Text = SettingsManager.ControlSettings.PrimaryAction;
                    break;

                case Actions.SecondaryAction:
                    _secondaryActionButton.Text = SettingsManager.ControlSettings.SecondaryAction;
                    break;

                case Actions.Say:
                    _sayButton.Text = SettingsManager.ControlSettings.Say;
                    break;

                case Actions.Inventory:
                    _inventoryButton.Text = SettingsManager.ControlSettings.Inventory;
                    break;
            }
        }

        // Some help
        // http://gamedev.stackexchange.com/questions/51822/changing-game-controls-using-txt-file
        // http://stackoverflow.com/questions/15933025/xna-controls-settings/15935732#15935732
        protected override void Back()
        {
            var settingsGUI = new SettingsGUI(Game);
            MenuState.SetGUI(settingsGUI);
        }
    }
}