using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.Editors.Textures;
using WorldsGame.Gamestates;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Utils.UIControls;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.GUI.TexturePackEditor
{
    internal class TextureEditorGUI : View.GUI.GUI
    {
        private readonly TextureEditor _textureEditor;
        private readonly bool _isIcon;
        private readonly SpriteFont _defaultFont;

        private BaseWorldsTextControl _nameInput;
        private BorderlessPanelControl _mainPanel;
        private BorderlessPanelControl _bottomPanel;
        private LabelControl _nameLabel;
        private LabelControl _sizeLabel;
        private ButtonControl _helpButton;
        private ButtonControl _increaseBrushButton;
        private InputControl _widthInput;
        private InputControl _heightInput;
        private ButtonControl _increaseHeightButton;
        private LabelControl _opacityLabel;
        private LabelControl _colorLabel;
        private ButtonControl _saveButton;
        private NumberInputControl _opacityInput;
        private LabelControl _titleLabel;
        private ButtonControl _previousFrameButton;
        private ButtonControl _nextFrameButton;
        private OptionControl _isAnimatedCheckbox;
        private LabelControl _framingLabel;

        internal float Opacity
        {
            get
            {
                if (_opacityInput.Text == "")
                {
                    _opacityInput.Text = "100";
                }

                return _opacityInput.GetFloat();
            }
        }

        internal WorldsColorInputControl ColorInput { get; private set; }

        internal TextureEditorGUI(WorldsGame game, TextureEditor textureEditor, bool isIcon = false)
            : base(game)
        {
            _textureEditor = textureEditor;
            _isIcon = isIcon;
            _defaultFont = game.Content.Load<SpriteFont>("Fonts/DefaultFont");
        }

        protected override void CreateControls()
        {
            base.CreateControls();
            AddLabelPanel();
            AddSecondLineControls();
        }

        private void AddLabelPanel()
        {
            var Y = 0;

            _mainPanel = new BorderlessPanelControl
            {
                Bounds = new UniRectangle(0, 0, Screen.Width, _isIcon ? 100 : 160)
            };

            _bottomPanel = new BorderlessPanelControl
            {
                Bounds = new UniRectangle(0, Screen.Height - 100, Screen.Width, 100)
            };

            _titleLabel = new LabelControl
            {
                Text = _isIcon ? "Icon editor" : "Texture editor",
                IsHeader = true,
                Bounds = new UniRectangle(TextureEditor.BACKGROUND_X_POSITION, 30, 110, LabelHeight)
            };

            _mainPanel.Children.Add(_titleLabel);

            Screen.Desktop.Children.Add(_mainPanel);
            Screen.Desktop.Children.Add(_bottomPanel);

            var backButton = new ButtonControl
            {
                Text = _isIcon ? "Save" : "Back",
                Bounds = new UniRectangle(ButtonDistanceFromRight, 0, 100, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            _bottomPanel.Children.Add(backButton);

            _saveButton = new ButtonControl
            {
                Text = "Save",
                Bounds = new UniRectangle(backButton.Bounds.Left - 110, 0, 100, 30)
            };
            _saveButton.Pressed += (sender, args) => SaveTexture();

            if (!_isIcon)
            {
                _bottomPanel.Children.Add(_saveButton);
            }

            if (!_isIcon)
            {
                _nameLabel = new LabelControl
                {
                    Bounds = new UniRectangle(_saveButton.Bounds.Left - 180 - 50 - 20, Y, 50, LabelHeight),
                    Text = "Name:"
                };

                _nameInput = new BaseWorldsTextControl
                {
                    Bounds = new UniRectangle(_nameLabel.Bounds.Right + 10, Y, 180, 30),
                };
                if (_textureEditor.Texture != null)
                    _nameInput.Text = _textureEditor.Texture.Name;

                _bottomPanel.Children.Add(_nameLabel);
                _bottomPanel.Children.Add(_nameInput);
            }
        }

        private void AddSecondLineControls()
        {
            UniScalar Y = TitleY + 10;

            AddHelpButton();

            AddBrushSizeControls(Y);

            if (!_isIcon)
            {
                AddWidthHeightControls(Y);
            }

            AddRGBHexBaseWorldsTextControl(Y);

            AddOpacityControls(Y);

            AddAnimationCheckbox();
            AddFrameControls();
        }

        private void AddAnimationCheckbox()
        {
            int X = (int)ColorInput.Bounds.Right.ToOffset(Screen.Width) + 30;

            _isAnimatedCheckbox = new OptionControl
            {
                Bounds =
                    new UniRectangle(X, ColorInput.Bounds.Top + 6, 16, 16),
                Selected = _textureEditor.Texture != null && _textureEditor.Texture.IsAnimated
            };
            _isAnimatedCheckbox.Changed += (sender, args) => ToggleAnimated();

            var isAnimatedLabel = new LabelControl
            {
                Text = "Animated",
                Bounds =
                    new UniRectangle(_isAnimatedCheckbox.Bounds.Right + 10,
                                    _isAnimatedCheckbox.Bounds.Top, 110, 16)
            };

            _mainPanel.Children.Add(_isAnimatedCheckbox);
            _mainPanel.Children.Add(isAnimatedLabel);
        }

        private void ToggleAnimated()
        {
            _textureEditor.ToggleAnimated();

            if (_textureEditor.Texture.IsAnimated)
            {
                _mainPanel.Children.Add(_previousFrameButton);
                _mainPanel.Children.Add(_framingLabel);
                _mainPanel.Children.Add(_nextFrameButton);
            }
            else
            {
                _mainPanel.Children.Remove(_previousFrameButton);
                _mainPanel.Children.Remove(_framingLabel);
                _mainPanel.Children.Remove(_nextFrameButton);
            }
        }

        private void AddFrameControls()
        {
            var Y = _colorLabel.Bounds.Bottom + 10;

            string frameString = string.Format("Frame {0}/{1}", 1, CompiledTexture.ANIMATED_FRAME_COUNT);
            int labelWidth = (int)(_defaultFont.MeasureString(frameString).X);

            //            int pagingWidth = 30 + 30 + labelWidth * 2;
            int X = (int)ColorInput.Bounds.Right.ToOffset(Screen.Width) + 30;// -pagingWidth / 2;

            _previousFrameButton = new ButtonControl
            {
                Text = "<",
                Bounds = new UniRectangle(X, Y, 30, 30)
            };
            _previousFrameButton.Pressed += OnPreviousFrame;

            _framingLabel = new LabelControl
            {
                Text = string.Format("Frame {0}/{1}", 1, CompiledTexture.ANIMATED_FRAME_COUNT),
                Bounds = new UniRectangle(_previousFrameButton.Bounds.Right + labelWidth / 2, Y, labelWidth, 30)
            };

            _nextFrameButton = new ButtonControl
            {
                Text = ">",
                Bounds = new UniRectangle(_framingLabel.Bounds.Right + labelWidth / 2, Y, 30, 30)
            };
            _nextFrameButton.Pressed += OnNextFrame;

            if (_textureEditor.Texture != null && _textureEditor.Texture.IsAnimated)
            {
                _mainPanel.Children.Add(_previousFrameButton);
                _mainPanel.Children.Add(_framingLabel);
                _mainPanel.Children.Add(_nextFrameButton);
            }
        }

        private void OnNextFrame(object sender, EventArgs eventArgs)
        {
            _textureEditor.OnNextFrame();
            _framingLabel.Text = string.Format("Frame {0}/{1}", _textureEditor.CurrentFrame + 1, CompiledTexture.ANIMATED_FRAME_COUNT);
        }

        private void OnPreviousFrame(object sender, EventArgs eventArgs)
        {
            _textureEditor.OnPreviousFrame();
            _framingLabel.Text = string.Format("Frame {0}/{1}", _textureEditor.CurrentFrame + 1, CompiledTexture.ANIMATED_FRAME_COUNT);
        }

        private void AddOpacityControls(UniScalar Y)
        {
            _opacityLabel = new LabelControl
            {
                Bounds = new UniRectangle(_colorLabel.Bounds.Left, _colorLabel.Bounds.Bottom + 10, 90, LabelHeight),
                Text = "Opacity (%):"
            };

            _opacityInput = new NumberInputControl
            {
                Bounds = new UniRectangle(_opacityLabel.Bounds.Right + 10, _opacityLabel.Bounds.Top, 60, 30),
                MinValue = 0,
                MaxValue = 100
            };

            _mainPanel.Children.Add(_opacityLabel);
            _mainPanel.Children.Add(_opacityInput);
        }

        private void AddHelpButton()
        {
            _helpButton = new ButtonControl
            {
                Text = "Help",
                Bounds = new UniRectangle(TextureEditor.BACKGROUND_X_POSITION, 0, ButtonWidth, 30)
            };
            _helpButton.Pressed += (sender, args) => ShowAlertBox(
                "Left click to draw or select color from palette.",
                "Right click to erase, shift to pick color from canvas.");

            _bottomPanel.Children.Add(_helpButton);
        }

        private void AddBrushSizeControls(UniScalar Y)
        {
            //            var buttonWidth = (int)((_nameInput.Bounds.Size.X.Offset - 30) / 2);

            var decreaseBrushButton = new ButtonControl
            {
                Text = "-",
                //                Bounds = _isIcon ? new UniRectangle(_helpButton.Bounds.Right + 30, _helpButton.Bounds.Top, 30, 30) : new UniRectangle(_nameLabel.Bounds.Left, Y, 30, 30)
                Bounds = new UniRectangle(TextureEditor.BACKGROUND_X_POSITION, Y, 30, 30)
            };
            decreaseBrushButton.Pressed += (sender, args) => DecreaseBrushSize();

            var sizeTextLabel = new LabelControl
            {
                Text = "Brush size:",
                Bounds = new UniRectangle(decreaseBrushButton.Bounds.Right + 10, decreaseBrushButton.Bounds.Top, 70, 30)
            };

            _sizeLabel = new LabelControl
            {
                Text = _textureEditor.BrushSize.ToString(),
                Bounds = new UniRectangle(sizeTextLabel.Bounds.Right + 10, decreaseBrushButton.Bounds.Top, 10, 30)
            };

            _increaseBrushButton = new ButtonControl
            {
                Text = "+",
                Bounds = new UniRectangle(_sizeLabel.Bounds.Right + 10, decreaseBrushButton.Bounds.Top, 30, 30)
            };
            _increaseBrushButton.Pressed += (sender, args) => IncreaseBrushSize();

            _mainPanel.Children.Add(decreaseBrushButton);
            _mainPanel.Children.Add(sizeTextLabel);
            _mainPanel.Children.Add(_sizeLabel);
            _mainPanel.Children.Add(_increaseBrushButton);
        }

        private void AddWidthHeightControls(UniScalar Y)
        {
            var decreaseWidthButton = new ButtonControl
            {
                Text = "-",
                Bounds = new UniRectangle(_increaseBrushButton.Bounds.Right + 25, Y, 30, 30)
            };
            decreaseWidthButton.Pressed += (sender, args) => DecreaseWidth();

            var widthTextLabel = new LabelControl
            {
                Text = "Width(px):",
                Bounds = new UniRectangle(decreaseWidthButton.Bounds.Right + 10, Y, 70, 30)
            };

            _widthInput = new InputControl
            {
                Text = _textureEditor.Width.ToString(),
                Bounds = new UniRectangle(widthTextLabel.Bounds.Right + 10, Y, 30, 30)
            };

            var increaseWidthButton = new ButtonControl
            {
                Text = "+",
                Bounds = new UniRectangle(_widthInput.Bounds.Right + 10, Y, 30, 30)
            };
            increaseWidthButton.Pressed += (sender, args) => IncreaseWidth();

            Y = decreaseWidthButton.Bounds.Bottom + 10;

            var decreaseHeightButton = new ButtonControl
            {
                Text = "-",
                Bounds = new UniRectangle(decreaseWidthButton.Bounds.Left, Y, 30, 30)
            };
            decreaseHeightButton.Pressed += (sender, args) => DecreaseHeight();

            var heightTextLabel = new LabelControl
            {
                Text = "Height(px):",
                Bounds = new UniRectangle(decreaseHeightButton.Bounds.Right + 10, Y, 70, 30)
            };

            _heightInput = new InputControl
            {
                Text = _textureEditor.Height.ToString(),
                Bounds = new UniRectangle(heightTextLabel.Bounds.Right + 10, Y, 30, 30)
            };

            _increaseHeightButton = new ButtonControl
            {
                Text = "+",
                Bounds = new UniRectangle(_heightInput.Bounds.Right + 10, Y, 30, 30)
            };
            _increaseHeightButton.Pressed += (sender, args) => IncreaseHeight();

            _mainPanel.Children.Add(decreaseWidthButton);
            _mainPanel.Children.Add(widthTextLabel);
            _mainPanel.Children.Add(_widthInput);
            _mainPanel.Children.Add(increaseWidthButton);

            _mainPanel.Children.Add(decreaseHeightButton);
            _mainPanel.Children.Add(heightTextLabel);
            _mainPanel.Children.Add(_heightInput);
            _mainPanel.Children.Add(_increaseHeightButton);
        }

        private void DecreaseBrushSize()
        {
            _textureEditor.DecreaseBrushSize();
            _sizeLabel.Text = _textureEditor.BrushSize.ToString();
        }

        private void IncreaseBrushSize()
        {
            _textureEditor.IncreaseBrushSize();
            _sizeLabel.Text = _textureEditor.BrushSize.ToString();
        }

        private void IncreaseWidth()
        {
            _textureEditor.IncreaseWidth();
            _widthInput.Text = _textureEditor.Width.ToString();
        }

        private void DecreaseWidth()
        {
            _textureEditor.DecreaseWidth();
            _widthInput.Text = _textureEditor.Width.ToString();
        }

        private void IncreaseHeight()
        {
            _textureEditor.IncreaseHeight();
            _heightInput.Text = _textureEditor.Height.ToString();
        }

        private void DecreaseHeight()
        {
            _textureEditor.DecreaseHeight();
            _heightInput.Text = _textureEditor.Height.ToString();
        }

        private void AddRGBHexBaseWorldsTextControl(UniScalar Y)
        {
            //            UniScalar X = _isIcon ? _increaseBrushButton.Bounds.Right + 40 : _increaseHeightButton.Bounds.Right + 40;
            UniScalar X = new UniScalar(TextureEditor.COLORPICKER_X_POSITION);
            _colorLabel = new LabelControl
            {
                Bounds = new UniRectangle(X, Y, 90, LabelHeight),
                Text = "Color:"
            };

            ColorInput = new WorldsColorInputControl
            {
                Bounds = new UniRectangle(_colorLabel.Bounds.Right + 10, Y, 80, 30)
            };

            _mainPanel.Children.Add(_colorLabel);
            _mainPanel.Children.Add(ColorInput);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Texture needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in texture name.");
                return false;
            }

            return true;
        }

        private void SaveTexture()
        {
            if (!_isIcon && !IsNameInputOK())
            {
                return;
            }

            _textureEditor.SaveTexture();

            Back();
        }

        private WorldSettings WorldSettings { get { return _textureEditor.WorldSettings; } }

        internal new void Back()
        {
            _textureEditor.SaveIcon();

            TextureDrawingState textureState = (TextureDrawingState)Game.GameStateManager.Pop();
            Screen.Desktop.Children.Clear();

            if (textureState.EditorType != ModelEditorType.None)
            {
                if (textureState.EditorType == ModelEditorType.Item)
                {
                    //                    var state = (MenuState)Game.GameStateManager.ActiveState;
                    //                    var textureListGUI = Texture.GetListGUI(Game, WorldSettings, texture: _textureEditor.Texture);
                    //                    state.SetGUI(textureListGUI);
                }
            }
            else if (!_isIcon)
            {
                var state = (MenuState)Game.GameStateManager.ActiveState;
                var textureListGUI = Texture.GetListGUI(Game, WorldSettings, texture: _textureEditor.Texture);
                state.SetGUI(textureListGUI);
            }
            else
            {
                if (_textureEditor.CharacterAttributeEditorGUI != null)
                {
                    var state = (MenuState)Game.GameStateManager.ActiveState;
                    state.SetGUI(_textureEditor.CharacterAttributeEditorGUI);
                }
                else if (_textureEditor.ItemOptionsPopupGUI != null)
                {
                    var state = (ItemEditorState)Game.GameStateManager.ActiveState;
                    state.OpenItemOptions();
                }
            }
        }

        internal string GetName()
        {
            return _nameInput.Text;
        }
    }
}