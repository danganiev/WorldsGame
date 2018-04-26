using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.GUI;
using WorldsGame.GUI.ModelEditor;
using WorldsGame.GUI.TexturePackEditor;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.ExtensionMethods;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.Editors.Textures
{
    internal class TextureEditor : IDisposable
    {
        internal static int COLORPICKER_X_POSITION = 640;
        private const int COLORPICKER_Y_POSITION = 200;
        private const int COLORPICKER_HEIGHT = 259;

        internal const int BACKGROUND_X_POSITION = 90;
        private const int BACKGROUND_Y_POSITION = 200;
        private const int BACKGROUND_PIXEL_LENGTH = 12;

        private const int SELECTED_COLOR_WIDTH = 384;
        private const int SELECTED_COLOR_HEIGHT = 30;        

        private SpriteBatch _spriteBatch;
        private Texture2D _selectedColorTexture;
        private Texture2D _colorPicker; // the image we use to pick colors
        private Texture2D _backgroundTexture; // our grid guide texture
        private Texture2D _workedTexture; // will store the data you edit on the grid

        //        private Texture2D _scaledWorkedTexture;
        private int _backgroundHeight = 384;

        private int _backgroundWidth = 384;

        private Color _cellColor;
        private Color[] _colorPickerData; // an array that holds the color of every pixel of our color picker image
        private Color[] _pixelData; // an array that contains the color of each pixel in our sprite

        private readonly ContentManager _content;
        private readonly WorldsGame _game;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly TextureEditorInputController _inputController;
        private TextureEditorGUI _textureEditorGUI;

        private readonly Color[] _startingColors;
        private readonly bool _isIcon;

        private Stack<Color[]> _undoStack;

        private int _pixelAmount;

        private int _currentFrame;
        private Color[] _oldPixelData;
        private List<Color[]> _frameData;

        internal Texture Texture { get; private set; }

        internal WorldSettings WorldSettings { get; private set; }

        internal int BrushSize { get; private set; }

        internal int ColorPickerBottom { get { return COLORPICKER_Y_POSITION + COLORPICKER_HEIGHT + SELECTED_COLOR_HEIGHT; } }

        internal int Width { get; private set; }

        internal int Height { get; private set; }

        internal CharacterAttributeEditorGUI CharacterAttributeEditorGUI { get; private set; }

        internal ItemOptionsPopupGUI ItemOptionsPopupGUI { get; private set; }

        internal int CurrentFrame
        {
            get { return _currentFrame; }
        }

        internal TextureEditor(
            WorldsGame game, GraphicsDevice graphicsDevice,
            WorldSettings worldSettings, Texture texture = null)
        {
            COLORPICKER_X_POSITION = game.GraphicsDevice.Viewport.Width - BACKGROUND_X_POSITION - SELECTED_COLOR_WIDTH;

            _content = new ContentManager(game.Services, "Content");
            _game = game;
            _graphicsDevice = graphicsDevice;

            Texture = texture ?? new Texture();

            BrushSize = 2;
            _inputController = new TextureEditorInputController();

            if (texture == null)
            {
                Width = 32;
                Height = 32;
            }
            else
            {
                Width = texture.Width;
                Height = texture.Height;
            }

            WorldSettings = worldSettings;

            _undoStack = new Stack<Color[]>();

            _frameData = new List<Color[]>();
        }

        internal TextureEditor(
            WorldsGame game, GraphicsDevice graphicsDevice,
            WorldSettings worldSettings, Texture texture, int xSize, int ySize)
            : this(game, graphicsDevice, worldSettings, texture)
        {
            Width = xSize;
            Height = ySize;
        }

        internal TextureEditor(
            WorldsGame game, GraphicsDevice graphicsDevice,
            WorldSettings worldSettings, Color[] startingColors, CharacterAttributeEditorGUI characterAttributeEditorGUI)
            : this(game, graphicsDevice, worldSettings, null)
        {
            _startingColors = startingColors;
            _isIcon = true;
            CharacterAttributeEditorGUI = characterAttributeEditorGUI;
            Width = 16;
            Height = 16;
        }

        internal TextureEditor(
           WorldsGame game, GraphicsDevice graphicsDevice,
           WorldSettings worldSettings, Color[] startingColors, ItemOptionsPopupGUI itemOptionsPopupGUI)
            : this(game, graphicsDevice, worldSettings, null)
        {
            _startingColors = startingColors;
            ItemOptionsPopupGUI = itemOptionsPopupGUI;
            _isIcon = true;

            Width = Icon.SIZE;
            Height = Icon.SIZE;
        }

        internal void Initialize()
        {
            _textureEditorGUI = new TextureEditorGUI(_game, this, _isIcon);
            _textureEditorGUI.Start();

            _textureEditorGUI.ColorInput.OnColorEntered += ParseStringColor;
        }

        internal void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _selectedColorTexture = _content.Load<Texture2D>(@"Textures/TextureEditor/nullTexture");
            _colorPicker = _content.Load<Texture2D>(@"Textures/TextureEditor/color_picker");
            _backgroundTexture = _content.Load<Texture2D>(@"Textures/TextureEditor/32field");

            CutBackgroundTexture();
            InitializeWorkedTexture();

            _colorPickerData = GetPixelData(_colorPicker);
            _pixelData = GetPixelData(_workedTexture);

            Subscribe();

            Reload(firstReload: true);
        }

        private void Reload(bool firstReload = false)
        {
            _backgroundTexture.Dispose();
            _backgroundTexture = _content.Load<Texture2D>(@"Textures/TextureEditor/32field");
            CutBackgroundTexture();

            _pixelData = GetPixelData(_workedTexture, isReloading: true);
            _oldPixelData = _pixelData;

            if (!firstReload)
            {
                _frameData.Clear();
                for (int i = 0; i < CompiledTexture.ANIMATED_FRAME_COUNT; i++)
                {
                    _frameData.Add((Color[])_pixelData.Clone());
                }
            }

            InitializeWorkedTexture(isReloading: true);
            _workedTexture.SetData(_pixelData);
        }

        private void Subscribe()
        {
            Messenger.On("EscapeKeyPressed", BackToMainMenu);

            _inputController.Undo += OnUndo;
        }

        private void OnUndo()
        {
            if (_undoStack.Count > 0)
            {
                _pixelData = _undoStack.Pop();
                _workedTexture.SetData(_pixelData);
            }
        }

        private void InitializeWorkedTexture(bool isReloading = false)
        {
            _workedTexture = new Texture2D(_graphicsDevice, Width, Height);

            if (Texture != null && !isReloading && Texture.Colors != null)
            {
                if (Texture.IsAnimated)
                {
                    _frameData.AddRange(Texture.FrameColors);
                    _workedTexture.SetData(Texture.FrameColors[0]);
                }
                else
                {
                    _workedTexture.SetData(Texture.Colors);
                }
                return;
            }

            if (_startingColors != null)
            {
                _workedTexture.SetData(_startingColors);
            }
        }

        internal void UnloadContent()
        {
            Unsubscribe();
            _content.Unload();
            _inputController.Undo -= OnUndo;
        }

        private void Unsubscribe()
        {
            Messenger.Off("EscapeKeyPressed", BackToMainMenu);
        }

        internal void Update(GameTime gameTime)
        {
            _inputController.Update();

            PickColor();
        }

        private void PickColor()
        {
            for (int i = 0; i < _colorPickerData.Length; i++)
            {
                int x = i % _colorPicker.Width;
                int y = i / _colorPicker.Width;
                var colorRect = new Rectangle(x + COLORPICKER_X_POSITION, y + COLORPICKER_Y_POSITION, 1, 1);
                if (colorRect.Contains(_inputController.MouseState.X, _inputController.MouseState.Y))
                {
                    if (_inputController.LeftMouseButtonPressed)
                    {
                        ChangePickerColor(_colorPickerData[i]);
                    }
                }
            }
        }

        internal void Draw(GameTime gameTime)
        {
            // The color of gui
            _graphicsDevice.Clear(new Color(230, 221, 209));

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            DrawColorPicker();
            DrawSelectedColor();
            DrawBackgroundTexture();

            PixelOperation();

            DrawWorkedTexture();
            //            DrawScaledWorkedTexture();

            _spriteBatch.End();
        }

        private void PushPreviousStateToUndoStack()
        {
            Color[] previousState = (Color[])_pixelData.Clone();

            if (_undoStack.Count > 0)
            {
                Color[] peekState = _undoStack.Peek();

                if (!peekState.SequenceEqual(previousState))
                {
                    _undoStack.Push(previousState);
                }
            }
            else
            {
                _undoStack.Push(previousState);
            }
        }

        private void PixelOperation()
        {
            if (_inputController.LeftMouseButtonPressed || _inputController.RightMouseButtonPressed ||
                _inputController.LeftShiftPressed)
            {
                for (int i = 0; i < _pixelData.Length; i++)
                {
                    var x = i % _workedTexture.Width * BACKGROUND_PIXEL_LENGTH + BACKGROUND_X_POSITION;
                    var y = (i % _pixelAmount) / _workedTexture.Width * BACKGROUND_PIXEL_LENGTH + BACKGROUND_Y_POSITION;
                    var pixelRect = new Rectangle(x, y, BACKGROUND_PIXEL_LENGTH, BACKGROUND_PIXEL_LENGTH);
                    var canvasRectangle = new Rectangle(BACKGROUND_X_POSITION, BACKGROUND_Y_POSITION, _backgroundWidth, _backgroundHeight);
                    if (canvasRectangle.Contains(_inputController.MouseState.X, _inputController.MouseState.Y))
                    {
                        if (pixelRect.Contains(_inputController.MouseState.X, _inputController.MouseState.Y))
                        {
                            if (_inputController.LeftMouseButtonPressed)
                            {
                                PushPreviousStateToUndoStack();
                                DrawPixel(i);
                            }
                            else if (_inputController.RightMouseButtonPressed)
                            {
                                PushPreviousStateToUndoStack();
                                ErasePixel(i);
                            }
                            else if (_inputController.LeftShiftPressed)
                            {
                                PickPixel(i);
                            }
                        }
                    }
                }
            }
        }

        private void DrawWorkedTexture()
        {
            _spriteBatch.Draw(_workedTexture,
                new Rectangle(BACKGROUND_X_POSITION, BACKGROUND_Y_POSITION, _backgroundWidth, _backgroundHeight),
                Color.White);
        }

        //        private void DrawScaledWorkedTexture()
        //        {
        //            // Draws little version of current texture
        //            _spriteBatch.Draw(_scaledWorkedTexture,
        //                new Rectangle(COLORPICKER_X_POSITION + 100, COLORPICKER_Y_POSITION + _colorPicker.Height + SELECTED_COLOR_HEIGHT + 50,
        //                    128, 128),
        //                Color.White);
        //        }

        private void DrawSelectedColor()
        {
            _spriteBatch.Draw(_selectedColorTexture,
                new Rectangle(COLORPICKER_X_POSITION, COLORPICKER_Y_POSITION + _colorPicker.Height,
                    SELECTED_COLOR_WIDTH, SELECTED_COLOR_HEIGHT),
                _cellColor);
        }

        private void DrawBackgroundTexture()
        {
            _spriteBatch.Draw(_backgroundTexture,
                new Rectangle(BACKGROUND_X_POSITION, BACKGROUND_Y_POSITION, _backgroundWidth, _backgroundHeight),
                Color.White);
        }

        private void DrawColorPicker()
        {
            _spriteBatch.Draw(_colorPicker, new Vector2(COLORPICKER_X_POSITION, COLORPICKER_Y_POSITION), Color.White);
        }

        private void PickPixel(int pixelIndex)
        {
            ChangePickerColor(_pixelData[pixelIndex]);
        }

        private void ChangePickerColor(Color color)
        {
            _cellColor = color;
            UpdateOpacity();
            System.Drawing.Color col = System.Drawing.Color.FromArgb(_cellColor.A, _cellColor.R, _cellColor.G, _cellColor.B);

            _textureEditorGUI.ColorInput.Text = col.ToArgb().ToString("X8").Substring(2, 6);
        }

        private void UpdateOpacity()
        {
            _cellColor.A = (byte)(255 * (_textureEditorGUI.Opacity / 100));
        }

        private void DrawPixel(int pixelIndex, bool isErasing = false)
        {
            _graphicsDevice.Textures[0] = null;
            UpdateOpacity();
            Color color = !isErasing ? _cellColor : new Color(0, 0, 0, 0);

            if (pixelIndex < _pixelAmount)
            {
                if (BrushSize > 1)
                {
                    for (int i = pixelIndex - (BrushSize - 1); i <= pixelIndex + (BrushSize - 1); i++)
                    {
                        if (i < _pixelAmount && i >= 0)
                        {
                            if (i / Width == pixelIndex / Width)
                            {
                                _pixelData[i] = color;
                            }
                        }
                    }
                    for (int i = BrushSize - 1; i >= 1; i--)
                    {
                        int minus = pixelIndex - Width * i;
                        for (int j = minus - (BrushSize - 1); j <= minus + (BrushSize - 1); j++)
                        {
                            if (j >= 0)
                            {
                                if (j / Width == minus / Width)
                                {
                                    _pixelData[j] = color;
                                }
                            }
                        }

                        int plus = pixelIndex + Width * i;
                        for (int j = plus - (BrushSize - 1); j <= plus + (BrushSize - 1); j++)
                        {
                            if (j < _pixelAmount)
                            {
                                if (j / Width == plus / Width)
                                {
                                    _pixelData[j] = color;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _pixelData[pixelIndex] = color;
                }

                _workedTexture.SetData(_pixelData);
            }
        }

        private void ErasePixel(int pixelIndex)
        {
            DrawPixel(pixelIndex, true);
        }

        private Color[] GetPixelData(Texture2D texture, bool isReloading = false)
        {
            Color[] pixels;
            if (!isReloading)
            {
                pixels = new Color[texture.Width * texture.Height];
                texture.GetData(pixels);
            }
            else
            {
                var oldPixels = new Color[texture.Width * texture.Height];
                pixels = new Color[Width * Height];
                if (oldPixels.Length >= pixels.Length)
                {
                    // Enshrinking of the texture
                    texture.GetData(0, new Rectangle(0, 0, Width, Height), pixels, 0, pixels.Length);
                }
                else
                {
                    // Enlarging of the texture
                    texture.GetData(0, new Rectangle(0, 0, texture.Width, texture.Height), oldPixels, 0, oldPixels.Length);

                    for (int i = 0; i < texture.Width; i++)
                    {
                        for (int j = 0; j < texture.Height; j++)
                        {
                            pixels[j * Width + i] = oldPixels[j * texture.Width + i];
                        }
                    }
                }
            }

            return pixels;
        }

        internal void SaveTexture()
        {
            if (!_isIcon)
            {
                if (Texture != null && !Texture.IsNew)
                {
                    Texture.Delete();
                }

                if (Texture == null)
                {
                    Texture = new Texture();
                }

                Texture.Name = _textureEditorGUI.GetName();
                Texture.WorldSettingsName = WorldSettings.Name;
                Texture.Colors = _pixelData;
                Texture.Width = Width;
                Texture.Height = Height;

                if (Texture.IsAnimated)
                {
                    Texture.FrameColors = _frameData;
                }

                Texture.Save();
            }

            //            Make10000RandomTextures();
        }

        internal void SaveIcon()
        {
            if (_isIcon)
            {
                if (CharacterAttributeEditorGUI != null)
                {
                    CharacterAttributeEditorGUI.SetIconColors(_pixelData);
                }
                else if (ItemOptionsPopupGUI != null)
                {
                    ItemOptionsPopupGUI.SetIconColors(_pixelData);
                }
            }
        }

        private void Make10000RandomTextures()
        {
            var r = new Random();
            for (int i = 0; i < 10000; i++)
            {
                int width = r.Next(32);
                int height = r.Next(32);

                if (width == 0 || height == 0)
                {
                    continue;
                }

                var color = new Color(r.Next(255), r.Next(255), r.Next(255));
                var pixels = new Color[width * height];
                for (int j = 0; j < width * height; j++)
                {
                    pixels[j] = color;
                }

                var texture = new Texture
                {
                    Name = i.ToString(),
                    WorldSettingsName = WorldSettings.Name,
                    Colors = pixels,
                    Width = width,
                    Height = height
                };

                texture.Save();
            }
        }

        internal void BackToMainMenu()
        {
            _textureEditorGUI.Back();
        }

        internal void DecreaseBrushSize()
        {
            if (BrushSize > 1)
            {
                BrushSize--;
            }
        }

        internal void IncreaseBrushSize()
        {
            if (BrushSize < 8)
            {
                BrushSize++;
            }
        }

        internal void IncreaseWidth()
        {
            if (Width < CompiledTexture.MAX_TEXTURE_SIZE)
            {
                Width++;
            }
            Reload();
        }

        internal void DecreaseWidth()
        {
            if (Width > 1)
            {
                Width--;
            }
            Reload();
        }

        internal void IncreaseHeight()
        {
            if (Height < CompiledTexture.MAX_TEXTURE_SIZE)
            {
                Height++;
            }
            Reload();
        }

        internal void DecreaseHeight()
        {
            if (Height > 1)
            {
                Height--;
            }
            Reload();
        }

        private void ParseStringColor(string input)
        {
            System.Drawing.Color col = System.Drawing.ColorTranslator.FromHtml(string.Format("#{0}", input));

            _cellColor = new Color(col.R, col.G, col.B, col.A);
        }

        private void CutBackgroundTexture()
        {
            _pixelAmount = Width * Height;

            _backgroundWidth = Width * BACKGROUND_PIXEL_LENGTH;
            _backgroundHeight = Height * BACKGROUND_PIXEL_LENGTH;
            int backgroundTextureTruePerPixelLength = _backgroundTexture.Width / 32;
            _backgroundTexture = _backgroundTexture.Crop(new Rectangle(0, 0, Width * backgroundTextureTruePerPixelLength, Height * backgroundTextureTruePerPixelLength));
        }

        internal void OnPreviousFrame()
        {
            if (CurrentFrame > 0)
            {
                _frameData[CurrentFrame] = _pixelData;
                _currentFrame = CurrentFrame - 1;

                if (_frameData[CurrentFrame] != null)
                {
                    _pixelData = _frameData[CurrentFrame];
                }
                _workedTexture.SetData(_pixelData);
            }
        }

        internal void OnNextFrame()
        {
            if (CurrentFrame < CompiledTexture.ANIMATED_FRAME_COUNT - 1)
            {
                _frameData[CurrentFrame] = _pixelData;
                _currentFrame = CurrentFrame + 1;

                if (_frameData[CurrentFrame] != null)
                {
                    _pixelData = _frameData[CurrentFrame];
                }
                _workedTexture.SetData(_pixelData);
            }
        }

        internal void ToggleAnimated()
        {
            Texture.IsAnimated = !Texture.IsAnimated;

            if (Texture.IsAnimated)
            {
                _oldPixelData = _pixelData;

                _frameData.Clear();

                _frameData.Add((Color[]) _pixelData.Clone());
                _frameData.Add((Color[]) _pixelData.Clone());
                _frameData.Add((Color[]) _pixelData.Clone());
                _frameData.Add((Color[]) _pixelData.Clone());
                
                _pixelData = _frameData[0];
            }
            else
            {
                _pixelData = _oldPixelData;
            }
        }

        public void Dispose()
        {
            _content.Unload();

            _spriteBatch.Dispose();
            //            _scaledWorkedTexture.Dispose();
            _workedTexture.Dispose();
            _inputController.Dispose();
        }
    }
}