using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class AtmosphereEditorGUI : View.GUI.GUI
    {
        private const int COLORPICKER_WIDTH = 100;
        private const int COLORPICKER_HEIGHT = 60;
        private static int COLORPICKER_X_POSITION = 150 + 30 + 50;
        private const int COLORPICKER_Y_POSITION = 200 + 30 + 30 + 20;
        private readonly WorldSettings _worldSettings;
        
        private ListControl _sunColorList;
        private ButtonControl _createSunColorButton;
        private ButtonControl _editSunColorButton;
        private ButtonControl _removeSunColorButton;
        private WorldsColorInputControl _sunColor;
        private NumberInputControl _timeOfDay;

        private bool _isSunColorEdited;

        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _colorPicker;

        private Color[] _colorPickerData;
        private Color _pickedColor;

        private MouseState _mouseState;

        protected override int LabelWidth { get { return 100; } }

        protected override string LabelText { get { return "Atmosphere & weather"; } }

        protected override bool IsBackable { get { return true; } }

        protected override bool IsSaveable { get { return true; } }

        internal int SecondRowLabelX { get { return 130 + FirstRowLabelX + 100 + 180; } }

        public AtmosphereEditorGUI(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            _worldSettings = worldSettings;
            Game = game;

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            _colorPicker = Game.Content.Load<Texture2D>(@"Textures/TextureEditor/color_picker");
            _colorPickerData = GetPixelData(_colorPicker);
            COLORPICKER_X_POSITION = SecondRowLabelX;
        }

        internal override void Update(GameTime gameTime)
        {
            _mouseState = Mouse.GetState();
            PickColor();
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddSunColorList();
            AddSunColorEditor();
        }

        protected override void LoadData()
        {
        }

        private void AddSunColorList()
        {
            var label = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, titleLabel.Bounds.Bottom + 50, 130, LabelHeight),
                Text = "Sunlight colors:",
                PopupText = "Sunlight colors per time of day. They are interpolated between each other"
            };

            _sunColorList = new ListControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + 10, titleLabel.Bounds.Bottom + 50, 130, ListHeight),
            };

            _createSunColorButton = new ButtonControl
            {
                Text = "Create",
                Bounds =
                    new UniRectangle(_sunColorList.Bounds.Right + 10, _sunColorList.Bounds.Top, ButtonWidth, ButtonHeight),
            };

            _editSunColorButton = new ButtonControl
            {
                Text = "Edit",
                Bounds =
                    new UniRectangle(_sunColorList.Bounds.Right + 10, _createSunColorButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
            };

            _removeSunColorButton = new ButtonControl
            {
                Text = "Remove",
                Bounds =
                    new UniRectangle(_sunColorList.Bounds.Right + 10, _editSunColorButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
            };

            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_sunColorList);
            Screen.Desktop.Children.Add(_createSunColorButton);
            Screen.Desktop.Children.Add(_editSunColorButton);
            Screen.Desktop.Children.Add(_removeSunColorButton);
        }

        private void AddSunColorEditor()
        {
            var timeLabel = new LabelControl
            {
                Bounds = new UniRectangle(SecondRowLabelX, titleLabel.Bounds.Bottom + 50, 130, LabelHeight),
                Text = "Time of day (0-24):",
            };

            _timeOfDay = new NumberInputControl
            {
                Bounds = new UniRectangle(timeLabel.Bounds.Right + 10, titleLabel.Bounds.Bottom + 50, 150, 30),
                IsIntegerOnly = true,
                IsPositiveOnly = true,
                MinValue = 0,
                MaxValue = 24
            };

            var colorLabel = new LabelControl
            {
                Bounds = new UniRectangle(SecondRowLabelX, _timeOfDay.Bounds.Bottom + 10, 130, LabelHeight),
                Text = "Color:",                
            };

            _sunColor = new WorldsColorInputControl
            {
                Bounds = new UniRectangle(colorLabel.Bounds.Right + 10, _timeOfDay.Bounds.Bottom + 10, 150, 30),
            };

            Screen.Desktop.Children.Add(timeLabel);
            Screen.Desktop.Children.Add(_timeOfDay);
            Screen.Desktop.Children.Add(colorLabel);
            Screen.Desktop.Children.Add(_sunColor);
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {        
            if (!_isSunColorEdited)
            {
                _spriteBatch.Begin(
                    SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                    DepthStencilState.Default, RasterizerState.CullNone);

                _spriteBatch.Draw(
                    _colorPicker, new Rectangle(COLORPICKER_X_POSITION, COLORPICKER_Y_POSITION, _colorPicker.Width, _colorPicker.Height), Color.White);

                _spriteBatch.End();
            }
        }

        private Color[] GetPixelData(Texture2D texture)
        {
            Color[] pixels;
//            if (!isReloading)
//            {
            pixels = new Color[texture.Width * texture.Height];
            texture.GetData(pixels);
//            }
//            else
//            {
//                var oldPixels = new Color[texture.Width * texture.Height];
////                pixels = new Color[Width * Height];
//                if (oldPixels.Length >= pixels.Length)
//                {
//                    // Enshrinking of the texture
//            texture.GetData(0, new Rectangle(0, 0, COLORPICKER_WIDTH, COLORPICKER_HEIGHT), pixels, 0, pixels.Length);
//                }
//                else
//                {
//                    // Enlarging of the texture
//                    texture.GetData(0, new Rectangle(0, 0, texture.Width, texture.Height), oldPixels, 0, oldPixels.Length);
//
//                    for (int i = 0; i < texture.Width; i++)
//                    {
//                        for (int j = 0; j < texture.Height; j++)
//                        {
//                            pixels[j * Width + i] = oldPixels[j * texture.Width + i];
//                        }
//                    }
//                }
//            }

            return pixels;
        }

        private void PickColor()
        {
            for (int i = 0; i < _colorPickerData.Length; i++)
            {
                int x = i % _colorPicker.Width;
                int y = i / _colorPicker.Width;
//                int x = i % COLORPICKER_WIDTH;
//                int y = i / COLORPICKER_WIDTH;
                var colorRect = new Rectangle(x + COLORPICKER_X_POSITION, y + COLORPICKER_Y_POSITION, 1, 1);
                if (colorRect.Contains(_mouseState.X, _mouseState.Y))
                {
                    if (_mouseState.LeftButton == ButtonState.Pressed)
                    {
                        ChangePickerColor(_colorPickerData[i]);
                    }
                }
            }
        }

        private void ChangePickerColor(Color color)
        {
            _pickedColor = color;
            _pickedColor.A = 255;
            System.Drawing.Color col = System.Drawing.Color.FromArgb(_pickedColor.A, _pickedColor.R, _pickedColor.G, _pickedColor.B);

            _sunColor.Text = col.ToArgb().ToString("X8").Substring(2, 6);
        }

        protected override void Back()
        {
            var worldEditorGUI = new WorldEditorGUI(Game, _worldSettings);
            MenuState.SetGUI(worldEditorGUI);
        }

        public override void Dispose()
        {
            _spriteBatch.Dispose();

            if (_colorPicker != null)
            {
                _colorPicker.Dispose();
            }            

            base.Dispose();
        }
    }
}