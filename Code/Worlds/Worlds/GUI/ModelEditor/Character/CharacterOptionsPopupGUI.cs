using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.GUI.RecipeEditor;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI.ModelEditor
{
    internal class CharacterOptionsPopupGUI : View.GUI.GUI
    {
        private const int BUTTON_WIDTH = 130;
        private const int FIRST_ROW_X = 30;
        private const int FIRST_COL_Y = 35;
        private const int SECOND_ROW_X = FIRST_ROW_X + BUTTON_WIDTH + 30;
        private const int THIRD_ATTRIBUTE_ROW_X = SECOND_ROW_X + 100 + 30;
        private const int LABEL_HEIGHT = 30;

        private readonly Color[] _emptyColors = Icon.EMPTY_COLORS;

        private static readonly List<Vector2> ATTRIBUTE_POSITIONS = new List<Vector2>
        {
            new Vector2(SECOND_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30),
            new Vector2(SECOND_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30 + 30 + 10),
            new Vector2(SECOND_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30 + 60 + 20),
            new Vector2(SECOND_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30 + 90 + 30),
            new Vector2(THIRD_ATTRIBUTE_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30),
            new Vector2(THIRD_ATTRIBUTE_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30 + 30 + 10),
            new Vector2(THIRD_ATTRIBUTE_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30 + 60 + 20),
            new Vector2(THIRD_ATTRIBUTE_ROW_X, FIRST_COL_Y + LABEL_HEIGHT + 30 + 90 + 30),
        };

        private int _panelTopX;
        private int _panelTopY;

        private readonly EditedModel _editedCharacter;
        private readonly CharacterEditorState _characterEditorState;
        private readonly WorldSettings _worldSettings;
        private readonly ModelCharacterEditorGUI _parentGUI;

        //        private ButtonControl _increaseHeightButton;
        //        private ButtonControl _increaseLengthButton;
        //        private ButtonControl _increaseWidthButton;

        private LabelControl _titleLabel;
        private LabelControl _heightValueLabel;
        private LabelControl _lengthValueLabel;
        private LabelControl _widthValueLabel;

        private ButtonControl _generalOptionsButton;
        private ButtonControl _attributesOptionsButton;
        private ButtonControl _modelOptionsButton;
        private ButtonControl _inventoryOptionsButton;

        private List<SpriteControl> _attributeIconSprites;
        private List<LabelControl> _attributeLabels;
        private List<NumberInputControl> _attributeValueInputs;

        private ItemListControl _inventoryItemsControl;

        private List<Texture2D> _attributeIcons;
        private List<IItemLike> _items;

        private ButtonControl _addItemButton;
        private Texture2D _trashCanIcon;
        private IconCellControl _trashCanCell;
        private ItemListControl _itemAdditionList;

        private ItemMousePicker _itemMousePicker;
        private IconCellControl _itemToAddIcon;
        private Texture2D _itemToAddIconTexture;
        private NumberIntervalInputControl _quantityInput;
        private NumberInputControl _itemProbabilityInput;
        private List<IItemLike> _spawnRules;

        private OptionControl _isPlayerCharacterCheckbox;

        private List<Control> _generalControls;
        private List<Control> _attributeControls;
        private List<Control> _modelControls;
        private List<Control> _inventoryControls;
        private BaseWorldsTextControl _itemAdditionFilterInput;

        internal PanelControl PropertiesPanel { get; private set; }

        internal PanelControl ItemAdditionPanel { get; private set; }

        internal CharacterOptionsPopupGUI(
            WorldsGame game, WorldSettings worldSettings, ModelCharacterEditorGUI parentGUI,
            EditedModel editedCharacter, CharacterEditorState characterEditorState)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;
            _worldSettings = worldSettings;
            _parentGUI = parentGUI;
            _editedCharacter = editedCharacter;
            _characterEditorState = characterEditorState;
            _attributeIcons = new List<Texture2D>();

            _attributeLabels = new List<LabelControl>();
            _attributeValueInputs = new List<NumberInputControl>();
            _attributeIconSprites = new List<SpriteControl>();

            _itemMousePicker = new ItemMousePicker(Game.GraphicsDevice, Game.Content);

            _itemToAddIconTexture = new Texture2D(Game.GraphicsDevice, Icon.SIZE, Icon.SIZE);
        }

        internal void GenerateControls()
        {
            AddMainPanel();
            //            AddSizeControls();

            //            AddAttributeControls();
            AddGeneralControls();
            AddInventoryControls();
            AddItemAdditionPanel();

            DepressEverything();
            ClearBoard();

            _generalOptionsButton.ConstantlyPressed = true;

            AddGeneralControls();
        }

        protected override void LoadData()
        {
            //            _internalGUI.LoadDataFromOutside();

            //            LoadList(_elementList, Texture.SaverHelper(_worldSettings.Name));
        }

        private void LoadItems()
        {
            _items = new List<IItemLike>();

            _items.AddRange(_worldSettings.Items);
        }

        private void AddMainPanel()
        {
            //            const int elementListWidth = 230;
            //            const int elementListHeight = 370;
            const int elementListWidth = 200;
            const int elementListHeight = 330;
            const int width = 880;
            const int height = 520;

            _panelTopX = (int)_parentGUI.Screen.Width / 2 - width / 2;
            _panelTopY = (int)_parentGUI.Screen.Height / 2 - height / 2;

            PropertiesPanel = new PanelControl
            {
                Bounds = new UniRectangle(_panelTopX, _panelTopY, width, height)
            };

            _titleLabel = new LabelControl
            {
                Text = "Character properties",
                Bounds = new UniRectangle(FIRST_ROW_X, FIRST_COL_Y, 110, LABEL_HEIGHT),
                IsHeader = true
            };

            _generalOptionsButton = new ButtonControl
            {
                Text = "General",
                Bounds = new UniRectangle(FIRST_ROW_X, _titleLabel.Bounds.Bottom + 30, BUTTON_WIDTH, 30)
            };
            _generalOptionsButton.Pressed += GeneralOptionsPressed;

            _attributesOptionsButton = new ButtonControl
            {
                Text = "Attributes",
                Bounds = new UniRectangle(FIRST_ROW_X, _generalOptionsButton.Bounds.Bottom + 10, BUTTON_WIDTH, 30)
            };
            _attributesOptionsButton.Pressed += AttributesOptionsPressed;

            _modelOptionsButton = new ButtonControl
            {
                Text = "Model",
                Bounds = new UniRectangle(FIRST_ROW_X, _attributesOptionsButton.Bounds.Bottom + 10, BUTTON_WIDTH, 30),
                ConstantlyPressed = true
            };
            _modelOptionsButton.PopupText = "Edit 3D-model properties, such as maximum height, width and length in block size";
            _modelOptionsButton.Pressed += ModelOptionsPressed;

            _inventoryOptionsButton = new ButtonControl
            {
                Text = "Inventory",
                Bounds = new UniRectangle(FIRST_ROW_X, _modelOptionsButton.Bounds.Bottom + 10, BUTTON_WIDTH, 30)
            };
            _inventoryOptionsButton.Pressed += InventoryOptionsPressed;

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(FIRST_ROW_X + 70)), new UniScalar(1f, -(FIRST_ROW_X + 30)), 70, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            PropertiesPanel.Children.Add(_titleLabel);
            PropertiesPanel.Children.Add(_generalOptionsButton);
            PropertiesPanel.Children.Add(_attributesOptionsButton);
            PropertiesPanel.Children.Add(_modelOptionsButton);
            PropertiesPanel.Children.Add(_inventoryOptionsButton);
            PropertiesPanel.Children.Add(backButton);
        }

        private void DepressEverything()
        {
            _generalOptionsButton.ConstantlyPressed = false;
            _attributesOptionsButton.ConstantlyPressed = false;
            _modelOptionsButton.ConstantlyPressed = false;
            _inventoryOptionsButton.ConstantlyPressed = false;
        }

        private void ClearBoard()
        {
            if (_generalControls != null)
            {
                foreach (Control ñontrol in _generalControls)
                {
                    PropertiesPanel.Children.Remove(ñontrol);
                }
            }
            if (_attributeControls != null)
            {
                foreach (Control ñontrol in _attributeControls)
                {
                    PropertiesPanel.Children.Remove(ñontrol);
                }
            }
            if (_modelControls != null)
            {
                foreach (Control ñontrol in _modelControls)
                {
                    PropertiesPanel.Children.Remove(ñontrol);
                }
            }
            if (_inventoryControls != null)
            {
                foreach (Control ñontrol in _inventoryControls)
                {
                    PropertiesPanel.Children.Remove(ñontrol);
                }
            }
        }

        private void GeneralOptionsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
            AddGeneralControls();
        }

        private void AttributesOptionsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
        }

        private void ModelOptionsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
        }

        private void InventoryOptionsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
            AddInventoryControls();
        }

        private void AddGeneralControls()
        {
            if (_generalControls == null)
            {
                _generalControls = new List<Control>();

                var Y = _titleLabel.Bounds.Bottom + 30;

                _isPlayerCharacterCheckbox = new OptionControl
                {
                    Bounds = new UniRectangle(SECOND_ROW_X, Y, 16, 16)
                };
                _isPlayerCharacterCheckbox.Changed += (sender, args) => ToggleIsPlayer();

                var isPlayerLabel = new LabelControl
                                    {
                                        Text = "Player character",
                                        Bounds =
                                            new UniRectangle(_isPlayerCharacterCheckbox.Bounds.Right + 10,
                                                             _isPlayerCharacterCheckbox.Bounds.Top, 110, 16)
                                    };

                _generalControls.Add(_isPlayerCharacterCheckbox);
                _generalControls.Add(isPlayerLabel);
            }

            _isPlayerCharacterCheckbox.Selected = _characterEditorState.EditedCharacter != null && _characterEditorState.EditedCharacter.IsPlayerCharacter;

            foreach (Control generalControl in _generalControls)
            {
                PropertiesPanel.Children.Add(generalControl);
            }
        }

        private void ToggleIsPlayer()
        {
            _characterEditorState.EditedCharacter.IsPlayerCharacter = _isPlayerCharacterCheckbox.Selected;
        }

        private void AddSizeControls()
        {
            var Y = _titleLabel.Bounds.Bottom + 30;

            var decreaseHeightButton = new ButtonControl
            {
                Text = "-",
                Bounds = new UniRectangle(_titleLabel.Bounds.Right + 50, Y, 30, 30)
            };
            decreaseHeightButton.Pressed += (sender, args) => DecreaseHeight();

            var heightTextLabel = new LabelControl
            {
                Text = "Height:",
                Bounds = new UniRectangle(decreaseHeightButton.Bounds.Right + 10, Y, 70, 30)
            };

            _heightValueLabel = new LabelControl
            {
                Text = _editedCharacter.HeightInBlocks.ToString(),
                Bounds = new UniRectangle(heightTextLabel.Bounds.Right + 10, Y, 10, 30)
            };

            var increaseHeightButton = new ButtonControl
            {
                Text = "+",
                Bounds = new UniRectangle(_heightValueLabel.Bounds.Right + 10, Y, 30, 30)
            };
            increaseHeightButton.Pressed += (sender, args) => IncreaseHeight();

            PropertiesPanel.Children.Add(decreaseHeightButton);
            PropertiesPanel.Children.Add(heightTextLabel);
            PropertiesPanel.Children.Add(_heightValueLabel);
            PropertiesPanel.Children.Add(increaseHeightButton);
        }

        private void AddAttributeControls()
        {
            _attributeValueInputs.Clear();
            _attributeLabels.Clear();
            _attributeIconSprites.Clear();

            for (int index = 0; index < _worldSettings.CharacterAttributes.Count; index++)
            {
                CharacterAttribute attribute = _worldSettings.CharacterAttributes[index];

                var icon = new Texture2D(Game.GraphicsDevice, 16, 16);
                icon.SetData(attribute.IconFull);
                _attributeIcons.Add(icon);

                Vector2 position = ATTRIBUTE_POSITIONS[index];

                var sprite = new SpriteControl
                {
                    Bounds = new UniRectangle(position.X, position.Y, 16, 16),
                    Sprite = icon,
                    Size = 16
                };

                _attributeIconSprites.Add(sprite);
                PropertiesPanel.Children.Add(sprite);

                var label = new LabelControl
                {
                    Bounds = new UniRectangle(position.X + 32, position.Y, 100, _parentGUI.LabelHeight),
                    Text = attribute.Name + ":"
                };

                _attributeLabels.Add(label);
                PropertiesPanel.Children.Add(label);

                var input = new NumberInputControl
                {
                    IsPositiveOnly = true,
                    Bounds = new UniRectangle(label.Bounds.Right + 10, position.Y, 100, _parentGUI.LabelHeight),
                    MaxValue = 9999
                };

                _attributeValueInputs.Add(input);
                PropertiesPanel.Children.Add(input);
            }
        }

        private void AddInventoryControls()
        {
            if (_inventoryControls == null)
            {
                _inventoryControls = new List<Control>();

                LoadItems();

                _spawnRules = new List<IItemLike>();
                _spawnRules.AddRange(_characterEditorState.InventorySpawnRules);

                _inventoryItemsControl = new ItemListControl(
                    _worldSettings, _parentGUI, SECOND_ROW_X, FIRST_COL_Y + 60, IconCellControl.ICON_CELL_SIZE * 10,
                    IconCellControl.ICON_CELL_SIZE * 5, _spawnRules,
                    control: PropertiesPanel, containerList: _inventoryControls);

                _inventoryItemsControl.Initialize();
                _inventoryItemsControl.OnItemLeftClick += OnItemInventoryListClick;

                _addItemButton = new ButtonControl
                                 {
                                     Bounds =
                                         new UniRectangle(SECOND_ROW_X + IconCellControl.ICON_CELL_SIZE * 10 + 10,
                                                          FIRST_COL_Y + 60, 100, ButtonHeight),

                                     Text = "Add item"
                                 };
                _addItemButton.Pressed += OnAddItem;

                _trashCanIcon = Game.Content.Load<Texture2D>("Textures/trash");

                _trashCanCell = new IconCellControl
                                {
                                    Bounds = new UniRectangle(
                                        _addItemButton.Bounds.Left,
                                        _addItemButton.Bounds.Top + IconCellControl.ICON_CELL_SIZE * 4,
                                        IconCellControl.ICON_CELL_SIZE, IconCellControl.ICON_CELL_SIZE),
                                    Icon = _trashCanIcon
                                };
                _trashCanCell.OnLeftMouseClick += OnTrashClick;

                _inventoryControls.Add(_addItemButton);
                _inventoryControls.Add(_trashCanCell);
            }

            foreach (Control generalControl in _inventoryControls)
            {
                PropertiesPanel.Children.Add(generalControl);
            }
        }

        private void OnAddItem(object sender, EventArgs eventArgs)
        {
            if (_spawnRules.Count >= 50)
            {
                _parentGUI.ShowAlertBox("Inventory is full.");
                return;
            }

            _parentGUI.Screen.Desktop.Children.Remove(PropertiesPanel);
            _parentGUI.Screen.Desktop.Children.Add(ItemAdditionPanel);
        }

        private void AddItemAdditionPanel()
        {
            const int width = 700;
            const int height = 470;

            _panelTopX = (int)_parentGUI.Screen.Width / 2 - width / 2;
            _panelTopY = (int)_parentGUI.Screen.Height / 2 - height / 2;

            const int secondRowX = FIRST_ROW_X + IconCellControl.ICON_CELL_SIZE * 5 + 100;

            ItemAdditionPanel = new PanelControl
            {
                Bounds = new UniRectangle(_panelTopX, _panelTopY, width, height)
            };

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(FIRST_ROW_X + 70)), new UniScalar(1f, -(FIRST_ROW_X + 30)), 70, 30)
            };
            backButton.Pressed += (sender, args) => BackToInventory();

            var addButton = new ButtonControl
            {
                Text = "Add",
                Bounds = new UniRectangle(new UniScalar(1f, -(FIRST_ROW_X + 170)), new UniScalar(1f, -(FIRST_ROW_X + 30)), 70, 30)
            };
            addButton.Pressed += (sender, args) => SaveAddedItem();

            var itemListLabel = new LabelControl
            {
                Text = "Item list",
                IsHeader = true,
                Bounds = new UniRectangle(FIRST_ROW_X, FIRST_COL_Y, 110, LABEL_HEIGHT)
            };

            var filterLabel = new LabelControl
            {
                Bounds = new UniRectangle(FIRST_ROW_X/* + IconCellControl.ICON_CELL_SIZE * 5 - 100 - 40 - 10*/, FIRST_COL_Y + 50, 40, LabelHeight),
                Text = "Filter:"
            };

            _itemAdditionFilterInput = new BaseWorldsTextControl
                                       {
                                           Bounds = new UniRectangle(filterLabel.Bounds.Right + 10, FIRST_COL_Y + 50, 100, LabelHeight)
                                       };

            _itemAdditionFilterInput.OnTextChanged += OnItemSelectionFilterChanged;

            _itemAdditionList = new ItemListControl(
                _worldSettings, _parentGUI, FIRST_ROW_X, FIRST_COL_Y + 50 + 10 + LabelHeight, IconCellControl.ICON_CELL_SIZE * 5,
                IconCellControl.ICON_CELL_SIZE * 5, _items,
                control: ItemAdditionPanel)
                                {
                                    HasPaging = true
                                };

            _itemAdditionList.OnItemLeftClick += ItemAdditionListClick;

            _itemAdditionList.Initialize();

            var itemOptionsLabel = new LabelControl
            {
                Text = "Item to add",
                IsHeader = true,
                Bounds = new UniRectangle(secondRowX, FIRST_COL_Y, 110, LABEL_HEIGHT)
            };

            var itemIconLabel = new LabelControl
            {
                Text = "Item:",
                Bounds = new UniRectangle(secondRowX, FIRST_COL_Y + 50 + 10 + LabelHeight, 130, IconCellControl.ICON_CELL_SIZE)
            };

            _itemToAddIcon = new IconCellControl
            {
                Bounds =
                    new UniRectangle(secondRowX + 140, FIRST_COL_Y + 50 + 10 + LabelHeight,
                                    IconCellControl.ICON_CELL_SIZE, IconCellControl.ICON_CELL_SIZE),
                Icon = _itemToAddIconTexture
            };

            _itemToAddIcon.OnLeftMouseClick += ItemToAddClick;

            var itemProbabilityInputLabel = new LabelControl
            {
                Text = "Spawn probability:",
                Bounds = new UniRectangle(secondRowX, itemIconLabel.Bounds.Bottom + 10, 130, LABEL_HEIGHT)
            };

            _itemProbabilityInput = new NumberInputControl
            {
                IsPositiveOnly = true,
                Bounds = new UniRectangle(secondRowX + 140, itemIconLabel.Bounds.Bottom + 10, 48, LABEL_HEIGHT),
                MinValue = 0,
                MaxValue = 100
            };

            var quantityInputLabel = new LabelControl
            {
                Text = "Quantity:",
                PopupText = "You can use intervals in the form '1-64'",
                Bounds = new UniRectangle(secondRowX, itemProbabilityInputLabel.Bounds.Bottom + 10, 110, LABEL_HEIGHT)
            };

            _quantityInput = new NumberIntervalInputControl
            {
                Bounds = new UniRectangle(secondRowX + 140, itemProbabilityInputLabel.Bounds.Bottom + 10, 48, LABEL_HEIGHT),
                MinValue = 1,
                MaxValue = 64
            };

            //            ItemAdditionPanel.Children.Add(titleLabel);
            ItemAdditionPanel.Children.Add(filterLabel);
            ItemAdditionPanel.Children.Add(_itemAdditionFilterInput);

            ItemAdditionPanel.Children.Add(itemListLabel);

            ItemAdditionPanel.Children.Add(itemOptionsLabel);

            ItemAdditionPanel.Children.Add(itemIconLabel);
            ItemAdditionPanel.Children.Add(_itemToAddIcon);
            ItemAdditionPanel.Children.Add(itemProbabilityInputLabel);
            ItemAdditionPanel.Children.Add(_itemProbabilityInput);
            ItemAdditionPanel.Children.Add(quantityInputLabel);
            ItemAdditionPanel.Children.Add(_quantityInput);

            ItemAdditionPanel.Children.Add(backButton);
            ItemAdditionPanel.Children.Add(addButton);
        }

        private void ItemToAddClick(string itemName)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                _itemToAddIcon.ObjectName = _itemMousePicker.PickedItem.Name;
                _itemToAddIcon.Icon.SetData(_itemMousePicker.PickedItem.IconColors);

                _itemMousePicker.PickedItem = null;
            }
            else if (itemName != null)
            {
                _itemMousePicker.PickedItem = _items.Find(item => item.Name == itemName);

                _itemToAddIcon.ObjectName = null;
                _itemToAddIcon.Icon.SetData(_emptyColors);
            }

            _itemToAddIcon.IsClicked = false;
        }

        private void OnTrashClick(string itemName)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                _itemMousePicker.PickedItem = null;
                _inventoryItemsControl.UnclickEverything();
            }
        }

        private void ItemAdditionListClick(string itemName)
        {
            _itemMousePicker.PickedItem = _itemMousePicker.PickedItem == null ? _items.Find(item => item.Name == itemName) : null;

            _itemAdditionList.UnclickEverything();
        }

        private void OnItemSelectionFilterChanged(string text)
        {
            if (text == "")
            {
                _itemAdditionList.UpdateItems(_items);
                return;
            }

            IEnumerable<IItemLike> filteredItems = from itemLike in _items
                                                   where itemLike.Name.Contains(text)
                                                   select itemLike;

            _itemAdditionList.UpdateItems(filteredItems);
        }

        private void OnItemInventoryListClick(string itemName)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                _spawnRules.Add(_itemMousePicker.PickedItem);
                _itemMousePicker.PickedItem = null;
                _inventoryItemsControl.UpdateItems(_spawnRules);
            }
            else
            {
                _itemMousePicker.PickedItem = _itemMousePicker.PickedItem == null
                                                  ? _items.Find(item => item.Name == itemName)
                                                  : null;

                var index = _spawnRules.IndexOf(_itemMousePicker.PickedItem);
                if (index != -1)
                {
                    _characterEditorState.InventorySpawnRules.RemoveAt(index);
                    _spawnRules.Remove(_itemMousePicker.PickedItem);

                    _inventoryItemsControl.UnclickEverything();
                    _inventoryItemsControl.UpdateItems(_spawnRules);
                }
            }
        }

        private void BackToInventory()
        {
            _itemMousePicker.PickedItem = null;

            _parentGUI.Screen.Desktop.Children.Remove(ItemAdditionPanel);
            _parentGUI.Screen.Desktop.Children.Add(PropertiesPanel);
        }

        private void SaveAddedItem()
        {
            if (_itemProbabilityInput.Text == "")
            {
                _parentGUI.ShowAlertBox("Please enter the probability of item spawning.");
                return;
            }
            if (_quantityInput.Text == "")
            {
                _parentGUI.ShowAlertBox("Please enter the quantity of items spawning.");
                return;
            }

            IItemLike addedItem = _items.Find(item => item.Name == _itemToAddIcon.ObjectName);

            int minQuantity, maxQuantity;
            _quantityInput.GetInterval(out minQuantity, out maxQuantity);

            var spawnedItem = new SpawnedItemRule
            {
                Name = addedItem.Name,
                Probability = (int)_itemProbabilityInput.GetFloat(),
                MinQuantity = minQuantity,
                MaxQuantity = maxQuantity,
                IconColors = addedItem.IconColors
            };

            _spawnRules.Add(_items.Find(item => item.Name == _itemToAddIcon.ObjectName));
            _characterEditorState.InventorySpawnRules.Add(spawnedItem);
            _inventoryItemsControl.UpdateItems(_spawnRules);

            BackToInventory();
        }

        internal override void Start()
        {
            GenerateControls();
            LoadData();
        }

        private void DecreaseHeight()
        {
        }

        private void IncreaseHeight()
        {
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _itemMousePicker.Draw(gameTime, spriteBatch);
        }

        protected override void Back()
        {
            _parentGUI.HideOptionsPanel();
        }

        private void Unsubscribe()
        {
            if (_itemAdditionList != null)
            {
                _itemAdditionList.OnItemLeftClick -= ItemAdditionListClick;
            }

            if (_itemToAddIcon != null)
            {
                _itemToAddIcon.OnLeftMouseClick -= ItemToAddClick;
            }

            if (_itemAdditionFilterInput != null)
            {
                _itemAdditionFilterInput.OnTextChanged -= OnItemSelectionFilterChanged;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Unsubscribe();

            _attributeIcons.Clear();
            _trashCanIcon.Dispose();
            _trashCanCell.Dispose();

            foreach (SpriteControl attributeIconSprite in _attributeIconSprites)
            {
                attributeIconSprite.Dispose();
            }

            _inventoryItemsControl.Dispose();
        }
    }
}