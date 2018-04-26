using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.GUI.RecipeEditor
{
    /// <summary>
    /// Absolutely non-reusable recipe control for use only in recipe editor
    /// </summary>
    internal class RecipeMakerSourcePlayerControl : IDisposable
    {
        private const int ITEM_AMOUNT = 9;
        private const int ITEM_AMOUNT_PER_ROW = 3;

        private readonly Color[] _emptyColors = Icon.EMPTY_COLORS;

        private readonly RecipeEditorGUI _gui;

        private List<IconCellControl> _cells;
        private IconCellControl _resultCell;

        // This is every item in the world, not only the 9 here.
        private List<IItemLike> _allItems;

        private Screen _screen;
        private int _x;
        private int _y;
        private GraphicsDevice _graphicsDevice;
        private Texture2D _guiTexture;
        private Texture2D _arrowTexture;
        private ButtonControl _resultQuantityUpButton;
        private ButtonControl _resultQuantityDownButton;

        internal event Action<string> OnItemLeftClick = name => { };

        internal int XSize
        {
            get
            {
                Dictionary<int, string> items = GetAllItems();

                if (items.Count == 0)
                {
                    return 0;
                }

                int minX = 2;
                int maxX = 0;

                foreach (KeyValuePair<int, string> item in items)
                {
                    int x = item.Key % ITEM_AMOUNT_PER_ROW;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                }

                return Math.Abs(maxX - minX) + 1;
            }
        }

        internal int YSize
        {
            get
            {
                var items = GetAllItems();

                if (items.Count == 0)
                {
                    return 0;
                }

                int minY = 2;
                int maxY = 0;

                foreach (KeyValuePair<int, string> item in items)
                {
                    int x = item.Key / ITEM_AMOUNT_PER_ROW;
                    minY = Math.Min(minY, x);
                    maxY = Math.Max(maxY, x);
                }

                return Math.Abs(maxY - minY) + 1;
            }
        }

        internal RecipeMakerSourcePlayerControl(RecipeEditorGUI gui, int x, int y, List<IItemLike> items)
        {
            _gui = gui;
            _graphicsDevice = _gui.Game.GraphicsDevice;
            _screen = _gui.Screen;
            _cells = new List<IconCellControl>();
            //            _itemIcons = new List<Texture2D>(ITEM_AMOUNT);
            _allItems = items;
            _x = x;
            _y = y;

            _guiTexture = _gui.Game.Content.Load<Texture2D>("Skin//Suave//SuaveSheet");
            _arrowTexture = _gui.Game.Content.Load<Texture2D>("Textures//blackArrow");
        }

        internal void Initialize()
        {
            InitializeCells();
            InitializeIcons();

            for (int i = 0; i < Icon.SIZE * Icon.SIZE; i++)
            {
                _emptyColors[i] = new Color(0, 0, 0, 0);
            }
        }

        private void InitializeCells()
        {
            for (int i = 0; i < ITEM_AMOUNT; i++)
            {
                var control = new IconCellControl
                              {
                                  Bounds =
                                      new UniRectangle(
                                      _x + IconCellControl.ICON_CELL_SIZE * (i % ITEM_AMOUNT_PER_ROW),
                                      _y + IconCellControl.ICON_CELL_SIZE * (i / ITEM_AMOUNT_PER_ROW),
                                      IconCellControl.ICON_CELL_SIZE,
                                      IconCellControl.ICON_CELL_SIZE)
                              };
                control.OnLeftMouseClick += OnItemClick;
                _cells.Add(control);
                _screen.Desktop.Children.Add(control);
            }

            _resultCell = new IconCellControl(hasQuantity: true)
            {
                Bounds =
                    new UniRectangle(
                    _x + IconCellControl.ICON_CELL_SIZE * 3 + 64 + 20 + IconCellControl.ICON_CELL_SIZE,
                    _y + IconCellControl.ICON_CELL_SIZE,
                    IconCellControl.ICON_CELL_SIZE,
                    IconCellControl.ICON_CELL_SIZE)
            };
            _resultCell.OnLeftMouseClick += OnItemClick;

            _resultQuantityUpButton = new ButtonControl
            {
                Bounds = new UniRectangle(_resultCell.Bounds.Right + 5, _resultCell.Bounds.Top,
                    20, IconCellControl.ICON_CELL_SIZE / 2 - 4),
                Text = "+"
            };
            _resultQuantityUpButton.Pressed += OnQuantityUp;

            _resultQuantityDownButton = new ButtonControl
            {
                Bounds = new UniRectangle(_resultCell.Bounds.Right + 5, _resultQuantityUpButton.Bounds.Bottom + 8,
                    20, IconCellControl.ICON_CELL_SIZE / 2 - 4),
                Text = "-"
            };
            _resultQuantityDownButton.Pressed += OnQuantityDown;

            _screen.Desktop.Children.Add(_resultCell);
            _screen.Desktop.Children.Add(_resultQuantityUpButton);
            _screen.Desktop.Children.Add(_resultQuantityDownButton);
        }

        private void InitializeIcons()
        {
            for (int i = 0; i < ITEM_AMOUNT; i++)
            {
                _cells[i].Icon = new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE);
            }

            _resultCell.Icon = new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE);
        }

        internal void SetItem(int cell, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _cells[cell].ObjectName = name;
                _cells[cell].Icon.SetData(_allItems.Find(item => item.Name == name).IconColors);
            }
            else
            {
                _cells[cell].ObjectName = "";
            }
        }

        internal void SetResultItem(string name, int quantity)
        {
            _resultCell.ObjectName = name;
            _resultCell.Quantity = quantity;
            _resultCell.Icon.SetData(_allItems.Find(item => item.Name == name).IconColors);
        }

        internal void SetClickedItem(string name)
        {
            if (!_resultCell.IsClicked)
            {
                int index = _cells.FindIndex(control => control.IsClicked);

                IconCellControl clickedControl = _cells[index];
                clickedControl.ObjectName = name;

                if (_graphicsDevice.Textures[0] == clickedControl.Icon)
                {
                    _graphicsDevice.Textures[0] = null;
                }
                clickedControl.Icon.SetData(_allItems.Find(item => item.Name == name).IconColors);
            }
            else
            {
                _resultCell.ObjectName = name;
                _resultCell.Quantity = 1;
                if (_graphicsDevice.Textures[0] == _resultCell.Icon)
                {
                    _graphicsDevice.Textures[0] = null;
                }
                _resultCell.Icon.SetData(_allItems.Find(item => item.Name == name).IconColors);
            }
        }

        internal void UnclickEverything()
        {
            foreach (IconCellControl iconCellControl in _cells)
            {
                iconCellControl.IsClicked = false;
            }
            _resultCell.IsClicked = false;
        }

        internal void UnsetSelected()
        {
            if (!_resultCell.IsClicked)
            {
                int index = _cells.FindIndex(control => control.IsClicked);
                IconCellControl clickedControl = _cells[index];
                clickedControl.ObjectName = null;

                if (_graphicsDevice.Textures[0] == clickedControl.Icon)
                {
                    _graphicsDevice.Textures[0] = null;
                }

                clickedControl.Icon.SetData(_emptyColors);
            }
            else
            {
                if (_graphicsDevice.Textures[0] == _resultCell.Icon)
                {
                    _graphicsDevice.Textures[0] = null;
                }

                _resultCell.Icon.SetData(_emptyColors);
            }
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                DepthStencilState.Default, RasterizerState.CullNone);

            spriteBatch.Draw(
                _guiTexture,
                new Rectangle(_x - 1, _y - 1,
                    3 * IconCellControl.ICON_CELL_SIZE + 2,
                    3 * IconCellControl.ICON_CELL_SIZE + 2),
                new Rectangle(157, 60, 1, 1), Color.White);

            spriteBatch.Draw(_arrowTexture, new Rectangle(_x + IconCellControl.ICON_CELL_SIZE * 3 + 4 + 32, _y + 40, 64, 64), Color.White);

            spriteBatch.End();
        }

        internal Dictionary<int, string> GetRelativeItems()
        {
            var preResult = new Dictionary<int, string>();
            var result = new Dictionary<int, string>();

            int firstItemIndex = FindFirstFilledItemIndex();

            if (firstItemIndex == -1)
            {
                return preResult;
            }

            int ySize = YSize;
            int xSize = XSize;

            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    preResult[i * ySize + j] = _cells[firstItemIndex + j + i * 3].ObjectName;
                }
            }

            int maxSize = xSize * ySize;

            for (int i = 0; i < maxSize; i++)
            {
                var item = preResult.First();
                preResult.Remove(item.Key);
                result[i] = item.Value;
            }

            return result;
        }

        internal Dictionary<int, string> GetAllItems()
        {
            var result = new Dictionary<int, string>();

            for (int i = 0; i < _cells.Count; i++)
            {
                IconCellControl iconCellControl = _cells[i];

                if (!string.IsNullOrEmpty(iconCellControl.ObjectName))
                {
                    result.Add(i, iconCellControl.ObjectName);
                }
            }

            return result;
        }

        internal string GetResultItem()
        {
            return _resultCell != null ? _resultCell.ObjectName : "";
        }

        internal int GetResultQuantity()
        {
            return _resultCell != null ? _resultCell.Quantity : 0;
        }

        private int FindFirstFilledItemIndex()
        {
            int minX = 3;
            int minY = 3;

            for (int i = 0; i < _cells.Count; i++)
            {
                IconCellControl iconCellControl = _cells[i];
                if (!string.IsNullOrEmpty(iconCellControl.ObjectName))
                {
                    int x = i % ITEM_AMOUNT_PER_ROW;
                    int y = i / ITEM_AMOUNT_PER_ROW;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                }
            }

            return minX + minY * 3;
        }

        // Due to multiple event identities we would need an additional method
        private void OnItemClick(string name)
        {
            OnItemLeftClick(name);
        }

        private void OnQuantityUp(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_resultCell.ObjectName) && _resultCell.Quantity < 64)
            {
                _resultCell.Quantity = _resultCell.Quantity + 1;
            }
        }

        private void OnQuantityDown(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_resultCell.ObjectName) && _resultCell.Quantity > 1)
            {
                _resultCell.Quantity = _resultCell.Quantity - 1;
            }
        }

        internal void Clear()
        {
            UnclickEverything();

            foreach (IconCellControl iconCellControl in _cells)
            {
                iconCellControl.ObjectName = "";
                iconCellControl.Icon.SetData(_emptyColors);
                iconCellControl.Quantity = 0;
            }

            _resultCell.ObjectName = "";
            _resultCell.Icon.SetData(_emptyColors);
            _resultCell.Quantity = 0;
        }

        public void Dispose()
        {
            OnItemLeftClick = null;

            _guiTexture.Dispose();
            _arrowTexture.Dispose();

            foreach (IconCellControl iconCellControl in _cells)
            {
                iconCellControl.Dispose();
            }

            _resultCell.Dispose();
        }
    }
}