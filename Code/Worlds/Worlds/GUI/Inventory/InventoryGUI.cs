using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Source.Controls;
using Nuclex.UserInterface.Source.Controls.Worlds;
using WorldsGame.GUI.RecipeEditor;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.Players;
using WorldsGame.Saving;
using WorldsGame.Terrain;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.GUI.Inventory
{
    internal class InventoryGUI : View.GUI.GUI
    {
        private readonly Player _player;
        private readonly World _world;
        private readonly Color[] _emptyColors = Icon.EMPTY_COLORS;

        private const int INVENTORY_CELL_AMOUNT = 10;

        private const int INVENTORY_ROW_AMOUNT = 4;
        private const int INVENTORY_PANEL_WIDTH = 340;
        private const int RECIPE_CELLS_HEIGHT = 32 * 3 + 10;
        private const int INVENTORY_PANEL_HEIGHT = 160 + RECIPE_CELLS_HEIGHT;

        private const int RECIPE_ITEM_AMOUNT_PER_ROW = 3;

        private readonly GraphicsDevice _graphicsDevice;
        private InventoryControl _inventoryControl;
        private List<InventoryCellControl> _cells;
        private ItemMousePicker _itemMousePicker;

        private int _xDrawStartPoint;
        private int _yDrawStartPoint;

        private Playing.Items.Inventory.Inventory _inventory;
        private List<InventoryCellControl> _recipeCells;
        private InventoryCellControl _recipeResultCell;
        private Texture2D _arrowTexture;

        internal int RecipeXSize
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
                    int x = item.Key % RECIPE_ITEM_AMOUNT_PER_ROW;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                }

                return Math.Abs(maxX - minX) + 1;
            }
        }

        internal int RecipeYSize
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
                    int x = item.Key / RECIPE_ITEM_AMOUNT_PER_ROW;
                    minY = Math.Min(minY, x);
                    maxY = Math.Max(maxY, x);
                }

                return Math.Abs(maxY - minY) + 1;
            }
        }

        public InventoryGUI(WorldsGame game, Screen screen, Player player, World world)
            : base(game, screen)
        {
            // Implied as a client player
            _player = player;
            _world = world;
            _graphicsDevice = game.GraphicsDevice;

            _xDrawStartPoint = (_graphicsDevice.Viewport.Width - INVENTORY_PANEL_WIDTH) / 2;
            _yDrawStartPoint = (_graphicsDevice.Viewport.Height - INVENTORY_PANEL_HEIGHT) / 2;

            _inventory = _player.Inventory;
            _itemMousePicker = new ItemMousePicker(_graphicsDevice, game.Content, Icon.SIZE);
            LoadContent();
            InitializeCells();
        }

        private void LoadContent()
        {
            _arrowTexture = Game.Content.Load<Texture2D>("Textures//blackArrow");
        }

        private void InitializeCells()
        {
            _cells = new List<InventoryCellControl>();
            _recipeCells = new List<InventoryCellControl>();
            _recipeResultCell = null;

            for (int i = 0; i < Playing.Items.Inventory.Inventory.MAX_ITEMS; i++)
            {
                _cells.Add(null);
            }

            for (int i = 0; i < 9; i++)
            {
                _recipeCells.Add(null);
            }
        }

        protected override void LoadData()
        {
            LoadCellsFromInventory();

            Messenger.On<int>("PlayerInventoryUpdate", UpdateInventoryCell);
        }

        protected override void CreateControls()
        {
            AddInventoryPanel();
            AddInventoryCells();
            AddRecipeCells();
        }

        private void AddInventoryPanel()
        {
            _inventoryControl = new InventoryControl
            {
                Bounds = new UniRectangle(_xDrawStartPoint, _yDrawStartPoint, INVENTORY_PANEL_WIDTH, INVENTORY_PANEL_HEIGHT)
            };
            Screen.Desktop.Children.Add(_inventoryControl);

            Messenger.On("MouseLeftButtonClick", OnMouseLeftClick);
        }

        private void OnMouseLeftClick()
        {
            var mouseState = Game.InputController.CurrentMouseState;

            if ((mouseState.X < _xDrawStartPoint || mouseState.X > _xDrawStartPoint + INVENTORY_PANEL_WIDTH) ||
                (mouseState.Y < _yDrawStartPoint || mouseState.Y > _yDrawStartPoint + INVENTORY_PANEL_HEIGHT))
            {
                ThrowItemFromInventory();
            }
        }

        private void ThrowItemFromInventory()
        {
            if (_itemMousePicker.PickedItem != null)
            {
                ItemEntityTemplate.Instance.BuildEntity(
                    _world.EntityWorld, _itemMousePicker.PickedItem.Name, _itemMousePicker.Quantity,
                    _player.Position + _player.LookVector, new Vector3(_player.LookVector.X, 0, _player.LookVector.Z) * 10);

                _itemMousePicker.Clear();
            }
        }

        private void SetupCell(InventoryCellControl cell, int index, Action<string, int> leftClickCallback = null,
            Action<string, int> rightClickCallback = null)
        {
            var icon = new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE);
            icon.SetData(_emptyColors);
            cell.Icon = icon;
            cell.IsEmpty = true;
            cell.Index = index;
            if (leftClickCallback == null)
            {
                cell.OnLeftMouseClick += OnItemClick;
                cell.OnRightMouseClick += OnItemRightClick;
            }
            else
            {
                cell.OnLeftMouseClick += leftClickCallback;
                cell.OnRightMouseClick += rightClickCallback;
            }

            _inventoryControl.Children.Add(cell);
        }

        private void AddInventoryCells()
        {
            for (int i = 0; i < INVENTORY_ROW_AMOUNT - 1; i++)
            {
                for (int j = 0; j < INVENTORY_CELL_AMOUNT; j++)
                {
                    var cell = new InventoryCellControl
                    {
                        Bounds = new UniRectangle(10 + j * 32, 10 + i * 32 + RECIPE_CELLS_HEIGHT, 32, 32)
                    };

                    int index = (i + 1) * 10 + j;
                    SetupCell(cell, index);
                    _cells[index] = cell;
                }
            }

            for (int i = 0; i < INVENTORY_CELL_AMOUNT; i++)
            {
                var cell = new InventoryCellControl
                {
                    Bounds = new UniRectangle(10 + i * 32, 20 + (INVENTORY_ROW_AMOUNT - 1) * 32 + RECIPE_CELLS_HEIGHT, 32, 32)
                };

                SetupCell(cell, i);
                _cells[i] = cell;
            }
        }

        private void AddRecipeCells()
        {
            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var cell = new InventoryCellControl
                    {
                        Bounds = new UniRectangle(10 + (j + 4) * 32, 10 + i * 32, 32, 32)
                    };

                    index = i * 3 + j;
                    SetupCell(cell, index, OnRecipeCellClick, OnRecipeCellRightClick);
                    _recipeCells[index] = cell;
                }
            }

            _recipeResultCell = new InventoryCellControl
            {
                Bounds = new UniRectangle(10 + 9 * 32, 10 + 1 * 32, 32, 32)
            };

            SetupCell(_recipeResultCell, 10, OnRecipeResultClick, OnRecipeResultClick);

            var arrowControl = new SpriteControl
            {
                Sprite = _arrowTexture,
                Bounds = new UniRectangle(10 + 7 * 32 + 10, 10 + 26, 32, 32),
                Size = 32
            };

            _inventoryControl.Children.Add(arrowControl);
        }

        // Initial load, for updates there should be another method/event
        private void LoadCellsFromInventory()
        {
            for (int i = 0; i < _inventory.Items.Count; i++)
            {
                InventoryItem item = _inventory.Items[i];
                if (item != null)
                {
                    _cells[i].Icon.SetData(ItemHelper.Get(item.Name).IconColors);
                    _cells[i].Quantity = item.Quantity;
                    _cells[i].ItemName = item.Name;
                    _cells[i].IsEmpty = false;
                }
            }
        }

        private void UpdateInventoryCells()
        {
            for (int i = 0; i < _inventory.Items.Count; i++)
            {
                UpdateInventoryCell(i);
            }
        }

        private void UpdateInventoryCell(int itemSlot)
        {
            InventoryItem item = _inventory.Items[itemSlot];

            if (item != null)
            {
                CompiledItem compiledItem = ItemHelper.Get(item.Name);

                if (compiledItem != null)
                {
                    _cells[itemSlot].Icon.SetData(compiledItem.IconColors);
                    _cells[itemSlot].Quantity = item.Quantity;
                    _cells[itemSlot].ItemName = item.Name;
                    _cells[itemSlot].IsEmpty = false;
                }
            }
            else if (!_cells[itemSlot].IsEmpty)
            {
                _cells[itemSlot].Icon.SetData(_emptyColors);
                _cells[itemSlot].ItemName = "";
                _cells[itemSlot].Quantity = 0;
                _cells[itemSlot].IsEmpty = true;
            }
        }

        private void TryToCraft()
        {
            InventoryItem resultItem = _world.RecipeManager.Craft(RecipeXSize, RecipeYSize, GetRelativeItems());

            if (resultItem != null)
            {
                CompiledItem compiledItem = ItemHelper.Get(resultItem.Name);

                _recipeResultCell.ItemName = resultItem.Name;

                int quantityDiff = ItemHelper.GetQuantityDiff(resultItem.Quantity, resultItem.Name);

                _recipeResultCell.Quantity = quantityDiff > 0 ? resultItem.Quantity : compiledItem.MaxStackCount;
                _recipeResultCell.Icon.SetData(compiledItem.IconColors);
            }
            else
            {
                _recipeResultCell.ItemName = "";
                _recipeResultCell.Quantity = 0;
                _recipeResultCell.Icon.SetData(_emptyColors);
            }
        }

        private void SubtractFromRecipe()
        {
            foreach (InventoryCellControl recipeCell in _recipeCells)
            {
                if (!string.IsNullOrEmpty(recipeCell.ItemName))
                {
                    recipeCell.Quantity -= 1;

                    if (recipeCell.Quantity == 0)
                    {
                        recipeCell.ItemName = "";
                        recipeCell.Icon.SetData(_emptyColors);
                    }
                }
            }

            TryToCraft();
        }

        // Debugger gets this events wrong
        private void OnItemClick(string name, int index)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                // TODO: Also need to stack items here too
                // Putting to empty slot
                if (string.IsNullOrEmpty(name))
                {
                    InventoryItem resultItem = _inventory.AddItemToSlot(index, _itemMousePicker.GetInventoryItem());

                    if (resultItem != null)
                    {
                        _itemMousePicker.PickedItem = ItemHelper.Get(resultItem.Name);
                        _itemMousePicker.Quantity = resultItem.Quantity;
                    }
                    else
                    {
                        _itemMousePicker.Clear();
                    }
                }
                // Putting to filled slot
                else
                {
                    // Same item
                    if (name == _itemMousePicker.PickedItem.Name)
                    {
                        InventoryItem resultItem = _inventory.AddItemToSlot(index, _itemMousePicker.GetInventoryItem());

                        if (resultItem != null)
                        {
                            _itemMousePicker.PickedItem = ItemHelper.Get(resultItem.Name);
                            _itemMousePicker.Quantity = resultItem.Quantity;
                        }
                        else
                        {
                            _itemMousePicker.Clear();
                        }
                    }
                    // Different items
                    else
                    {
                        InventoryItem item = _inventory.TakeItemFromSlot(index);

                        _inventory.AddItemToSlot(index, _itemMousePicker.GetInventoryItem());
                        _itemMousePicker.PickedItem = ItemHelper.Get(item.Name);
                        _itemMousePicker.Quantity = item.Quantity;
                    }
                }

                UpdateInventoryCells();
            }
            // Taking from filled slot
            else if (!string.IsNullOrEmpty(name))
            {
                InventoryItem item = _inventory.TakeItemFromSlot(index);

                _itemMousePicker.PickedItem = ItemHelper.Get(item);
                _itemMousePicker.Quantity = item.Quantity;

                UpdateInventoryCells();
            }

            UnclickEverything();
        }

        private void OnItemRightClick(string name, int index)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                // Putting to empty or same item slot
                if (name == _itemMousePicker.PickedItem.Name || string.IsNullOrEmpty(name))
                {
                    InventoryItem resultItem = _inventory.AddItemToSlot(
                        index, new InventoryItem { Name = _itemMousePicker.PickedItem.Name, Quantity = 1 });

                    if (resultItem == null)
                    {
                        _itemMousePicker.Quantity -= 1;

                        if (_itemMousePicker.Quantity == 0)
                        {
                            _itemMousePicker.Clear();
                        }
                    }
                }
                else
                {
                }

                UpdateInventoryCells();
            }
            // Taking from filled slot
            else if (!string.IsNullOrEmpty(name))
            {
                InventoryItem item = _inventory.TakeItemFromSlot(index);

                int quantity = item.Quantity > 1 ? item.Quantity / 2 : item.Quantity;

                if (item.Quantity > 1)
                {
                    var returnItem = new InventoryItem { Name = item.Name, Quantity = item.Quantity - quantity };
                    _inventory.AddItemToSlot(index, returnItem);
                }

                _itemMousePicker.PickedItem = ItemHelper.Get(item.Name);
                _itemMousePicker.Quantity = quantity;

                UpdateInventoryCells();
            }

            UnclickEverything();
        }

        private void OnRecipeCellClick(string name, int index)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                var cell = _recipeCells[index];

                // Putting to filled with same item cell
                if (_itemMousePicker.PickedItem.Name == cell.ItemName)
                {
                    var item = ItemHelper.Get(_itemMousePicker.PickedItem.Name);

                    int quantityLeft = item.MaxStackCount - _itemMousePicker.Quantity;

                    if (cell.Quantity <= quantityLeft)
                    {
                        cell.Quantity += _itemMousePicker.Quantity;

                        _itemMousePicker.Clear();
                    }
                    else
                    {
                        _itemMousePicker.Quantity -= quantityLeft;

                        _recipeResultCell.Quantity += quantityLeft;
                    }
                }
                // Putting to filled with different item cell
                else
                {
                    CompiledItem takenItem = ItemHelper.Get(_itemMousePicker.PickedItem.Name);
                    bool isEmpty = string.IsNullOrEmpty(cell.ItemName);

                    int takenQuantity = _itemMousePicker.Quantity;

                    if (!isEmpty)
                    {
                        CompiledItem givenItem = ItemHelper.Get((cell.ItemName));

                        _itemMousePicker.PickedItem = givenItem;
                        _itemMousePicker.Quantity = cell.Quantity;
                    }

                    cell.ItemName = takenItem.Name;
                    cell.Quantity = takenQuantity;
                    cell.Icon.SetData(takenItem.IconColors);

                    if (isEmpty)
                    {
                        _itemMousePicker.Clear();
                    }
                }
            }
            // Taking from filled cell
            else if (!string.IsNullOrEmpty(name))
            {
                var cell = _recipeCells[index];

                CompiledItem resultItem = ItemHelper.Get(name);

                _itemMousePicker.PickedItem = resultItem;
                _itemMousePicker.Quantity = cell.Quantity;

                cell.Quantity = 0;
                cell.ItemName = "";
                cell.Icon.SetData(_emptyColors);
            }

            TryToCraft();
            UnclickEverything();
        }

        private void OnRecipeCellRightClick(string name, int index)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                var cell = _recipeCells[index];

                // Putting to filled with same item cell
                if (_itemMousePicker.PickedItem.Name == cell.ItemName || string.IsNullOrEmpty(name))
                {
                    var item = ItemHelper.Get(_itemMousePicker.PickedItem.Name);

                    int quantityLeft = item.MaxStackCount - _itemMousePicker.Quantity;

                    if (string.IsNullOrEmpty(name))
                    {
                        cell.Quantity = 0;
                    }

                    if (cell.Quantity <= quantityLeft)
                    {
                        if (string.IsNullOrEmpty(name))
                        {
                            cell.ItemName = _itemMousePicker.PickedItem.Name;
                            cell.Icon.SetData(item.IconColors);
                        }

                        cell.Quantity += 1;

                        _itemMousePicker.Quantity -= 1;

                        if (_itemMousePicker.Quantity == 0)
                        {
                            _itemMousePicker.Clear();
                        }
                    }
                }
                // Putting to filled with different item cell
                else
                {
                    CompiledItem takenItem = ItemHelper.Get(_itemMousePicker.PickedItem.Name);
                    bool isEmpty = string.IsNullOrEmpty(cell.ItemName);

                    int takenQuantity = _itemMousePicker.Quantity;

                    if (!isEmpty)
                    {
                        CompiledItem givenItem = ItemHelper.Get((cell.ItemName));

                        _itemMousePicker.PickedItem = givenItem;
                        _itemMousePicker.Quantity = cell.Quantity;
                    }

                    cell.ItemName = takenItem.Name;
                    cell.Quantity = takenQuantity;
                    cell.Icon.SetData(takenItem.IconColors);

                    if (isEmpty)
                    {
                        _itemMousePicker.Clear();
                    }
                }
            }
            // Taking from filled cell
            else if (!string.IsNullOrEmpty(name))
            {
                var cell = _recipeCells[index];

                CompiledItem item = ItemHelper.Get(name);

                int quantity = cell.Quantity > 1 ? cell.Quantity / 2 : cell.Quantity;

                if (cell.Quantity > 1)
                {
                    cell.Quantity = cell.Quantity - quantity;
                }
                else
                {
                    cell.Quantity = 0;
                    cell.ItemName = "";
                    cell.Icon.SetData(_emptyColors);
                }

                _itemMousePicker.PickedItem = ItemHelper.Get(item.Name);
                _itemMousePicker.Quantity = quantity;
            }

            TryToCraft();
            UnclickEverything();
        }

        private void OnRecipeResultClick(string name, int index)
        {
            // Taking to filled picker
            if (_itemMousePicker.PickedItem != null)
            {
                if (!string.IsNullOrEmpty(name) && _itemMousePicker.PickedItem.Name == name)
                {
                    CompiledItem resultItem = ItemHelper.Get(name);

                    int quantityLeft = resultItem.MaxStackCount - _itemMousePicker.Quantity;

                    if (_recipeResultCell.Quantity <= quantityLeft)
                    {
                        _itemMousePicker.Quantity += _recipeResultCell.Quantity;

                        _recipeResultCell.Quantity = 0;
                        _recipeResultCell.ItemName = "";
                        _recipeResultCell.Icon.SetData(_emptyColors);
                        SubtractFromRecipe();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(name))
            {
                CompiledItem resultItem = ItemHelper.Get(name);

                _itemMousePicker.PickedItem = resultItem;
                _itemMousePicker.Quantity = _recipeResultCell.Quantity;

                SubtractFromRecipe();
            }

            UnclickEverything();
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

            int ySize = RecipeYSize;
            int xSize = RecipeXSize;

            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    preResult[i * ySize + j] = _cells[firstItemIndex + j + i * 3].ItemName;
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

            for (int i = 0; i < _recipeCells.Count; i++)
            {
                InventoryCellControl iconCellControl = _recipeCells[i];

                if (!string.IsNullOrEmpty(iconCellControl.ItemName))
                {
                    result.Add(i, iconCellControl.ItemName);
                }
            }

            return result;
        }

        private int FindFirstFilledItemIndex()
        {
            return _recipeCells.FindIndex(control => !string.IsNullOrEmpty(control.ItemName));
        }

        private void UnclickEverything()
        {
            foreach (InventoryCellControl cell in _cells)
            {
                cell.IsClicked = false;
            }
            foreach (InventoryCellControl inventoryCellControl in _recipeCells)
            {
                inventoryCellControl.IsClicked = false;
            }
            _recipeResultCell.IsClicked = false;
        }

        internal override void Start()
        {
            Messenger.On<int>("PlayerInventoryUpdate", UpdateInventoryCell);

            if (_inventoryControl == null)
            {
                base.Start();
            }
            else
            {
                UpdateInventoryCells();
                Screen.Desktop.Children.Add(_inventoryControl);
            }
        }

        internal void Stop()
        {
            Messenger.Off<int>("PlayerInventoryUpdate", UpdateInventoryCell);
            Screen.Desktop.Children.Clear();
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _itemMousePicker.Draw(gameTime, spriteBatch);
        }

        public override void Dispose()
        {
            Messenger.Off<int>("PlayerInventoryUpdate", UpdateInventoryCell);
            Messenger.Off("MouseLeftButtonClick", OnMouseLeftClick);

            foreach (var cell in _cells)
            {
                if (cell != null)
                {
                    if (cell.Icon != null)
                    {
                        cell.Icon.Dispose();
                        cell.Icon = null;
                    }
                }
            }
        }
    }
}