using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI.RecipeEditor
{
    internal class ItemListControl : IDisposable
    {
        private readonly Color[] _emptyColors = Icon.EMPTY_COLORS;

        private readonly View.GUI.GUI _gui;

        private readonly int _x;
        private readonly int _y;
        private readonly int _height;
        private readonly int _width;
        private readonly WorldSettings _worldSettings;
        private readonly Screen _screen;
        private Texture2D _guiTexture;
        private Control _parentControl;
        private readonly int _pagingYOffset;
        private readonly int _pagingXIndentation;
        private readonly List<Control> _containerList;

        private List<IItemLike> _items;

        private List<IconCellControl> _cells;

        private readonly int _cellAmount;
        private readonly int _rowAmount;
        private BaseWorldsTextControl _filterInput;
        private readonly GraphicsDevice _graphicsDevice;
        private ButtonControl _previousPageButton;
        private ButtonControl _nextPageButton;
        private readonly SpriteFont _defaultFont;

        private int ItemAmountPerPage
        {
            get { return _cellAmount * _rowAmount; }
        }

        internal bool HasFilterInput { get; set; }

        internal bool HasPaging { get; set; }

        internal event Action<string> OnItemLeftClick = name => { };

        internal event Action<string> OnFilterInput = name => { };

        internal ItemListControl(
            WorldSettings worldSettings, View.GUI.GUI gui, int x, int y, int width, int height, List<IItemLike> items,
            Control control = null, int pagingYOffset = 24, int pagingXIndentation = 48, List<Control> containerList = null)
        {
            _worldSettings = worldSettings;
            _gui = gui;
            _screen = _gui.Screen;
            _x = x;
            _y = y;
            _height = height;
            _width = width;
            _items = new List<IItemLike>(items);
            _cells = new List<IconCellControl>();
            _graphicsDevice = gui.Game.GraphicsDevice;
            _parentControl = control;
            _pagingYOffset = pagingYOffset;
            _pagingXIndentation = pagingXIndentation;
            _containerList = containerList;

            _defaultFont = _gui.Game.Content.Load<SpriteFont>("Fonts/DefaultFont");
            _guiTexture = _gui.Game.Content.Load<Texture2D>("Skin//Suave//SuaveSheet");

            GetIconAmount(_width, _height, out _cellAmount, out _rowAmount);

            _width = _cellAmount * IconCellControl.ICON_CELL_SIZE;
            _height = _rowAmount * IconCellControl.ICON_CELL_SIZE;
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                DepthStencilState.Default, RasterizerState.CullNone);

            spriteBatch.Draw(
                _guiTexture,
                new Rectangle(_x - 1, _y - 1,
                    _cellAmount * IconCellControl.ICON_CELL_SIZE + 2,
                    _rowAmount * IconCellControl.ICON_CELL_SIZE + 2),
                new Rectangle(157, 60, 1, 1), Color.White);

            spriteBatch.End();
        }

        internal void Initialize()
        {
            AddCells();

            if (HasFilterInput)
            {
                AddFilterInput();
            }

            if (HasPaging)
            {
                AddPaging();
            }

            LoadData();
        }

        private void LoadData()
        {
            for (int i = 0; i < ItemAmountPerPage; i++)
            {
                if (i == _items.Count)
                {
                    break;
                }

                var icon = new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE);
                icon.SetData(_items[i].IconColors ?? Icon.EMPTY_COLORS);

                _cells[i].ObjectName = _items[i].Name;

                if (_items[i].Description != null)
                {
                    _cells[i].AdditionalText = _items[i].Description;
                }

                _cells[i].Icon = icon;
            }
        }

        private void ClearCells()
        {
            foreach (IconCellControl cellControl in _cells)
            {
                cellControl.ObjectName = "";

                if (cellControl.Icon != null)
                {
                    cellControl.Icon.SetData(_emptyColors);
                }
            }
        }

        internal void UpdateItems(IEnumerable<IItemLike> items)
        {
            _items.Clear();
            _items.AddRange(items);

            ClearCells();

            for (int i = 0; i < ItemAmountPerPage; i++)
            {
                if (i == _items.Count)
                {
                    break;
                }

                if (_cells[i].Icon == null)
                {
                    _cells[i].Icon = new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE);
                }

                _cells[i].ObjectName = _items[i].Name;

                if (_items[i].Description != null)
                {
                    _cells[i].AdditionalText = _items[i].Description;
                }

                _cells[i].Icon.SetData(_items[i].IconColors);
            }
        }

        internal void UnclickEverything()
        {
            foreach (IconCellControl iconCellControl in _cells)
            {
                iconCellControl.IsClicked = false;
            }
        }

        private void AddCells()
        {
            for (int i = 0; i < ItemAmountPerPage; i++)
            {
                var control = new IconCellControl
                              {
                                  Bounds =
                                      new UniRectangle(
                                      _x + IconCellControl.ICON_CELL_SIZE * (i % _cellAmount),
                                      _y + IconCellControl.ICON_CELL_SIZE * (i / _cellAmount),
                                      IconCellControl.ICON_CELL_SIZE, IconCellControl.ICON_CELL_SIZE)
                              };
                control.OnLeftMouseClick += OnItemClick;

                _cells.Add(control);

                if (_containerList != null)
                {
                    _containerList.Add(control);
                }
                else if (_parentControl == null)
                {
                    _screen.Desktop.Children.Add(control);
                }
                else
                {
                    _parentControl.Children.Add(control);
                }
            }
        }

        private void AddFilterInput()
        {
            _filterInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(_x + IconCellControl.ICON_CELL_SIZE * _cellAmount - 100, _y - 50, 100, 30)
            };
            _filterInput.OnTextChanged += FilterInputEventProxy;

            var filterLabel = new LabelControl
            {
                Bounds = new UniRectangle(_filterInput.Bounds.Left - 50, _y - 50, 40, 30),
                Text = "Filter:"
            };

            Control control = _parentControl ?? _screen.Desktop;

            control.Children.Add(_filterInput);
            control.Children.Add(filterLabel);
        }

        private void FilterInputEventProxy(string text)
        {
            OnFilterInput(text);
        }

        private void AddPaging()
        {
            int Y = _y + _height + _pagingYOffset;

            string pageString = string.Format("Page {0}/{1}", 1, 1);
            int labelWidth = (int)(_defaultFont.MeasureString(pageString).X);

            int pagingWidth = 30 + 30 + labelWidth * 2;
            int X = _x + _width / 2 - pagingWidth / 2;

            _previousPageButton = new ButtonControl
            {
                Text = "<",
                Bounds = new UniRectangle(X, Y, 30, 30)
            };

            var pagingLabel = new LabelControl
            {
                Text = string.Format("Page {0}/{1}", 1, 1),
                Bounds = new UniRectangle(_previousPageButton.Bounds.Right + labelWidth / 2, Y, labelWidth, 30)
            };

            _nextPageButton = new ButtonControl
            {
                Text = ">",
                Bounds = new UniRectangle(pagingLabel.Bounds.Right + labelWidth / 2, Y, 30, 30)
            };

            Control control = _parentControl ?? _screen.Desktop;

            control.Children.Add(_previousPageButton);
            control.Children.Add(pagingLabel);
            control.Children.Add(_nextPageButton);
        }

        // Due to multiple event identities we would need additional method
        private void OnItemClick(string name)
        {
            OnItemLeftClick(name);
        }

        internal int GetClickedCellIndex()
        {
            return _cells.FindIndex(control => control.IsClicked);
        }

        internal static void GetIconAmount(int width, int height, out int cellAmount, out int rowAmount)
        {
            cellAmount = width / IconCellControl.ICON_CELL_SIZE;
            rowAmount = height / IconCellControl.ICON_CELL_SIZE;
        }

        public void Dispose()
        {
            _guiTexture.Dispose();

            foreach (IconCellControl iconCellControl in _cells)
            {
                iconCellControl.Dispose();
            }
        }
    }
}