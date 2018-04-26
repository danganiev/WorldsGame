using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class CharacterAttributeEditorGUI : View.GUI.GUI
    {
        private Color[] _fullIconColors;
        private Color[] _halfIconColors;
        private Texture2D _fullIconTexture;
        private Texture2D _halfIconTexture;
        private LabelControl _fullIconLabel;
        private LabelControl _halfIconLabel;
        private ButtonControl _fullIconEditButton;
        private ButtonControl _halfIconEditButton;
        private bool _isFullIconEdited;
        private bool _isHalfIconEdited;

        private LabelControl _nameLabel;

        private InputControl _nameInput;

        //        private LabelControl _minValueLabel;
        //        private NumberInputControl _minValueControl;
        private LabelControl _defaultValueLabel;

        private NumberInputControl _defaultValueControl;
        private LabelControl _maxValueLabel;
        private NumberInputControl _maxValueControl;

        private float _minValue;
        private float _defaultValue;
        private float _maxValue;

        internal CharacterAttribute CharacterAttribute { get; set; }

        internal WorldSettings WorldSettings { get; set; }

        protected override string LabelText { get { return IsNew ? "Create character attribute" : "Edit character attribute"; } }

        protected override bool IsSaveable { get { return true; } }

        protected override bool IsBackable { get { return true; } }

        protected override int LabelWidth { get { return 150; } }

        internal bool IsNew
        {
            get { return CharacterAttribute == null; }
        }

        internal string FullIconLabelText
        {
            get
            {
                if (_fullIconColors != null)
                {
                    return "Full icon:";
                }
                return "Full icon: None";
            }
        }

        internal string HalfIconLabelText
        {
            get
            {
                if (_halfIconColors != null)
                {
                    return "Half icon:";
                }
                return "Half icon: None";
            }
        }

        private bool AreIconsFilled
        {
            get { return _halfIconColors != null && _fullIconColors != null; }
        }

        private bool AreValuesFilled
        {
            get { return _defaultValueControl.Text != "" && /*_minValueControl.Text != "" &&*/ _maxValueControl.Text != ""; }
        }

        internal CharacterAttributeEditorGUI(WorldsGame game, WorldSettings world)
            : base(game)
        {
            WorldSettings = world;
        }

        internal CharacterAttributeEditorGUI(WorldsGame game, WorldSettings world, CharacterAttribute characterAttribute)
            : this(game, world)
        {
            CharacterAttribute = characterAttribute;
            _isFullIconEdited = true;
            SetIconColors(CharacterAttribute.IconFull);
            _isHalfIconEdited = true;
            SetIconColors(CharacterAttribute.IconHalf);
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            RefillIconControls();
            RefillValueControls();
        }

        private void RefillIconControls()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            _nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Name:"
            };

            _nameInput = new InputControl
            {
                Bounds = new UniRectangle(_nameLabel.Bounds.Right + 30, Y, 95, 30),
                Text = IsNew ? "" : CharacterAttribute.Name
            };

            Y += 50;

            _fullIconLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = FullIconLabelText
            };

            _fullIconEditButton = new ButtonControl
            {
                Bounds = new UniRectangle(_fullIconLabel.Bounds.Right + 30 + 30, Y, ButtonWidth, ButtonHeight),
                Text = "Edit"
            };
            _fullIconEditButton.Pressed += (sender, args) => EditFullIcon();

            Y += 50;

            _halfIconLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = HalfIconLabelText
            };

            _halfIconEditButton = new ButtonControl
            {
                Bounds = new UniRectangle(_halfIconLabel.Bounds.Right + 30 + 30, Y, ButtonWidth, ButtonHeight),
                Text = "Edit"
            };
            _halfIconEditButton.Pressed += (sender, args) => EditHalfIcon();

            Screen.Desktop.Children.Add(_nameLabel);
            Screen.Desktop.Children.Add(_nameInput);

            Screen.Desktop.Children.Add(_fullIconLabel);
            Screen.Desktop.Children.Add(_fullIconEditButton);

            Screen.Desktop.Children.Add(_halfIconLabel);
            Screen.Desktop.Children.Add(_halfIconEditButton);
        }

        private void RefillValueControls()
        {
            var Y = _halfIconEditButton.Bounds.Bottom + 20;
            //            _minValueLabel = new LabelControl
            //            {
            //                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
            //                Text = "Default minimum value:"
            //            };

            //            _minValueControl = new NumberInputControl
            //            {
            //                Bounds = new UniRectangle(_minValueLabel.Bounds.Right + 30, Y, 50, 30),
            //                Text = IsNew ? "0" : CharacterAttribute.MinValue.ToString(),
            //                MinValue = -1000,
            //                MaxValue = 1000
            //            };

            _defaultValueLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Default value:"
            };
            _defaultValueControl = new NumberInputControl
            {
                Bounds = new UniRectangle(_defaultValueLabel.Bounds.Right + 30, Y, 50, 30),
                Text = IsNew ? "100" : CharacterAttribute.DefaultValue.ToString(),
                MinValue = 0,
                MaxValue = 1000
            };

            _maxValueLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, _defaultValueControl.Bounds.Bottom + 20, LabelWidth, LabelHeight),
                Text = "Default maximum value:"
            };
            _maxValueControl = new NumberInputControl
            {
                Bounds = new UniRectangle(_defaultValueLabel.Bounds.Right + 30, _defaultValueControl.Bounds.Bottom + 20, 50, 30),
                Text = IsNew ? "100" : CharacterAttribute.DefaultMaxValue.ToString(),
                MinValue = 0,
                MaxValue = 1000
            };

            //            Screen.Desktop.Children.Add(_minValueLabel);
            //            Screen.Desktop.Children.Add(_minValueControl);
            Screen.Desktop.Children.Add(_defaultValueLabel);
            Screen.Desktop.Children.Add(_defaultValueControl);
            Screen.Desktop.Children.Add(_maxValueLabel);
            Screen.Desktop.Children.Add(_maxValueControl);
        }

        private void EditFullIcon()
        {
            FillAvailableValues();
            _isFullIconEdited = true;
            Game.GameStateManager.Push(new TextureDrawingState(Game, WorldSettings, _fullIconColors, this));
        }

        private void EditHalfIcon()
        {
            FillAvailableValues();
            _isHalfIconEdited = true;
            Game.GameStateManager.Push(new TextureDrawingState(Game, WorldSettings, _halfIconColors, this));
        }

        private void FillAvailableValues()
        {
            if (CharacterAttribute == null)
            {
                CharacterAttribute = new CharacterAttribute();
            }
            AreValuesOK();
            CharacterAttribute.DefaultValue = _defaultValue;
            //            CharacterAttribute.DefaultMinValue = _minValue;
            CharacterAttribute.DefaultMaxValue = _maxValue;
            CharacterAttribute.Name = _nameInput.Text;
        }

        internal void SetIconColors(Color[] colors)
        {
            if (_isFullIconEdited)
            {
                _fullIconColors = colors;
                _fullIconTexture = new Texture2D(Game.GraphicsDevice, 16, 16);
                _fullIconTexture.SetData(_fullIconColors);
            }
            else if (_isHalfIconEdited)
            {
                _halfIconColors = colors;
                _halfIconTexture = new Texture2D(Game.GraphicsDevice, 16, 16);
                _halfIconTexture.SetData(_halfIconColors);
            }

            _isFullIconEdited = false;
            _isHalfIconEdited = false;
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.DrawAfterGUI(gameTime, spriteBatch);

            if (!_isFullIconEdited && !_isHalfIconEdited)
            {
                //                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                //                                   DepthStencilState.Default, RasterizerState.CullNone);

                if (_fullIconTexture != null)
                {
                    spriteBatch.Draw(_fullIconTexture,
                                      new Rectangle(FirstRowLabelX + LabelWidth + 30, TitleY + 80 + 50, 16, 16), Color.White);
                }
                if (_halfIconTexture != null)
                {
                    spriteBatch.Draw(_halfIconTexture,
                                      new Rectangle(FirstRowLabelX + LabelWidth + 30, TitleY + 80 + 50 + 50, 16, 16),
                                      Color.White);
                }

                //                spriteBatch.End();
            }
        }

        protected override void Save()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            if (!AreIconsFilled)
            {
                ShowAlertBox("Both full and half value icons should be filled.");
                return;
            }

            if (!AreValuesFilled)
            {
                ShowAlertBox("Minimum, default and maximum values should be filled");
                return;
            }

            if (!AreValuesOK())
            {
                ShowAlertBox("Minimum, default or maximum value is wrong.");
                return;
            }

            if (_minValue > _maxValue)
            {
                ShowAlertBox("Minimum value can't be greater than maximum value");
                return;
            }

            if (_defaultValue < _minValue)
            {
                ShowAlertBox("Default value can't be less than minimum value");
                return;
            }

            if (_defaultValue > _maxValue)
            {
                ShowAlertBox("Default value can't be greater than maximum value");
                return;
            }

            if (!IsNew)
            {
                CharacterAttribute.Delete();
            }

            CharacterAttribute = new CharacterAttribute
            {
                Name = _nameInput.Text,
                WorldSettingsName = WorldSettings.Name,
                IconFull = _fullIconColors,
                IconHalf = _halfIconColors,
                DefaultValue = _defaultValue,
                //                MinValue = _minValue,
                DefaultMaxValue = _maxValue
            };
            CharacterAttribute.Save();

            Back();
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Character attribute needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in a name.");
                return false;
            }

            return true;
        }

        private bool AreValuesOK()
        {
            //            bool result = float.TryParse(_minValueControl.Text, out _minValue);
            bool result = float.TryParse(_defaultValueControl.Text, out _defaultValue);
            result = result && float.TryParse(_maxValueControl.Text, out _maxValue);

            return result;
        }

        protected override void Back()
        {
            var characterAttributeGUI = CharacterAttribute.GetListGUI(Game, WorldSettings, MenuState, CharacterAttribute);
            MenuState.SetGUI(characterAttributeGUI);
        }

        public override void Dispose()
        {
            //            _spriteBatch.Dispose();

            if (_fullIconTexture != null)
            {
                _fullIconTexture.Dispose();
            }
            if (_halfIconTexture != null)
            {
                _halfIconTexture.Dispose();
            }

            base.Dispose();
        }
    }
}