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
    internal class BlockOptionsPopupGUI : View.GUI.GUI
    {
        private const int BUTTON_WIDTH = 130;
        private const int FIRST_ROW_X = 30;
        private const int FIRST_COL_Y = 15;
        private const int SECOND_ROW_X = FIRST_ROW_X + BUTTON_WIDTH + 30;
        private const int LABEL_HEIGHT = 30;
        private const int LIST_WIDTH = 160;

        private readonly Color[] _emptyColors = Icon.EMPTY_COLORS;

        private readonly EditedModel _editedBlock;
        private readonly BlockEditorState _blockEditorState;
        private readonly WorldSettings _worldSettings;
        private readonly ModelBlockEditorGUI _parentGUI;

        internal PanelControl ItemAdditionPanel { get; private set; }

        internal PanelControl Panel { get; private set; }

        private LabelControl _titleLabel;
        private OptionControl _isCubicalBlockCheckbox;
        private OptionControl _isUnbreakableCheckbox;
        private OptionControl _isLiquidCheckbox;

        private NumberInputControl _blockHealthControl;

        private ButtonControl _generalOptionsButton;
        private ButtonControl _itemDropsButton;
        private ButtonControl _soundsButton;

        private List<Control> _generalControls;
        private List<Control> _itemDropControls;
        private List<Control> _soundControls;

        private List<IItemLike> _spawnRules;
        private List<IItemLike> _items;
        private List<string> _footstepSoundNames;

        private ItemListControl _itemDropsListControl;
        private ButtonControl _addItemButton;
        private Texture2D _trashCanIcon;
        private IconCellControl _trashCanCell;

        private ItemMousePicker _itemMousePicker;
        private Texture2D _itemToAddIconTexture;
        private ItemListControl _itemAdditionList;
        private IconCellControl _itemToAddIcon;
        private NumberIntervalInputControl _quantityInput;
        private NumberInputControl _itemProbabilityInput;
        private BaseWorldsTextControl _itemSelectionFilterInput;

        private ListControl _footstepSoundsList;
        private ButtonControl _addFootstepSoundButton;
        private AddSoundPanel _soundPanelConstructor;

        internal BlockOptionsPopupGUI(
            WorldsGame game, WorldSettings worldSettings, ModelBlockEditorGUI parentGUI,
            EditedModel editedBlock, BlockEditorState blockEditorState)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;
            _worldSettings = worldSettings;
            _parentGUI = parentGUI;
            _editedBlock = editedBlock;
            _blockEditorState = blockEditorState;

            _itemMousePicker = new ItemMousePicker(Game.GraphicsDevice, Game.Content);

            _itemToAddIconTexture = new Texture2D(Game.GraphicsDevice, Icon.SIZE, Icon.SIZE);
        }

        internal void GenerateControls()
        {
            AddMainPanel();
            AddGeneralControls();
            AddItemDropsControls();
            AddItemAdditionPanel();

            DepressEverything();
            ClearBoard();

            _generalOptionsButton.ConstantlyPressed = true;

            AddGeneralControls();
        }

        internal override void Start()
        {
            GenerateControls();
            LoadData();
        }

        protected override void LoadData()
        {
        }

        private void LoadItems()
        {
            _items = new List<IItemLike>();

            _items.AddRange(_worldSettings.Items);
        }

        private void AddMainPanel()
        {
            const int Y = 15;
            const int X = 30;
            const int width = 450;
            const int height = 350;

            Panel = new PanelControl
            {
                Bounds = new UniRectangle(_parentGUI.Screen.Width / 2 - width / 2, _parentGUI.Screen.Height / 2 - height / 2, width, height)
            };

            _titleLabel = new LabelControl
            {
                Text = "Block options:",
                Bounds = new UniRectangle(X, Y, 110, _parentGUI.LabelHeight),
                IsHeader = true
            };

            _generalOptionsButton = new ButtonControl
            {
                Text = "General",
                Bounds = new UniRectangle(FIRST_ROW_X, _titleLabel.Bounds.Bottom + 30, BUTTON_WIDTH, 30)
            };
            _generalOptionsButton.Pressed += GeneralOptionsPressed;

            _itemDropsButton = new ButtonControl
            {
                Text = "Item Drops",
                Bounds = new UniRectangle(FIRST_ROW_X, _generalOptionsButton.Bounds.Bottom + 10, BUTTON_WIDTH, 30)
            };
            _itemDropsButton.Pressed += ItemDropsPressed;

            _soundsButton = new ButtonControl
            {
                Text = "Sounds",
                Bounds = new UniRectangle(FIRST_ROW_X, _itemDropsButton.Bounds.Bottom + 10, BUTTON_WIDTH, 30)
            };
            _soundsButton.Pressed += SoundsPressed;

            var panelBackButton = new ButtonControl
            {
                Text = "Back",
                Bounds =
                    new UniRectangle(new UniScalar(1f, -100), new UniScalar(1f, -60), 70, 30)
            };
            panelBackButton.Pressed += (sender, args) => Back();

            Panel.Children.Add(_titleLabel);
            Panel.Children.Add(_generalOptionsButton);
            Panel.Children.Add(_itemDropsButton);
            Panel.Children.Add(_soundsButton);

            Panel.Children.Add(panelBackButton);
        }

        private void ClearBoard()
        {
            if (_generalControls != null)
            {
                foreach (Control ñontrol in _generalControls)
                {
                    Panel.Children.Remove(ñontrol);
                }
            }
            if (_itemDropControls != null)
            {
                foreach (Control ñontrol in _itemDropControls)
                {
                    Panel.Children.Remove(ñontrol);
                }
            }
            if (_soundControls != null)
            {
                foreach (Control ñontrol in _soundControls)
                {
                    Panel.Children.Remove(ñontrol);
                }
            }
        }

        private void DepressEverything()
        {
            _generalOptionsButton.ConstantlyPressed = false;
            _itemDropsButton.ConstantlyPressed = false;
            _soundsButton.ConstantlyPressed = false;
        }

        private void GeneralOptionsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
            AddGeneralControls();
        }

        private void ItemDropsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
            AddItemDropsControls();
        }

        private void SoundsPressed(object sender, EventArgs eventArgs)
        {
            DepressEverything();
            ClearBoard();
            ((ButtonControl)sender).ConstantlyPressed = true;
            AddSoundControls();
        }

        private void AddGeneralControls()
        {
            if (_generalControls == null)
            {
                _generalControls = new List<Control>();

                _isCubicalBlockCheckbox = new OptionControl
                {
                    Bounds = new UniRectangle(SECOND_ROW_X, _titleLabel.Bounds.Bottom + 30, 16, 16),
                    Selected = _editedBlock.IsFullBlock
                };
                _isCubicalBlockCheckbox.Changed += (sender, args) => ToggleFullBlock();

                var cubicalBlockCheckboxLabel = new LabelControl
                {
                    Text = "Cubical block",
                    Bounds =
                        new UniRectangle(_isCubicalBlockCheckbox.Bounds.Right + 10,
                                            _isCubicalBlockCheckbox.Bounds.Top, 110, 16)
                };

                _isUnbreakableCheckbox = new OptionControl
                {
                    Bounds =
                        new UniRectangle(SECOND_ROW_X, _isCubicalBlockCheckbox.Bounds.Bottom + 10, 16, 16),
                    Selected = _blockEditorState.EditedBlock.IsUnbreakable
                };
                _isUnbreakableCheckbox.Changed += (sender, args) => ToggleUnbreakable();

                var unbreakableLabel = new LabelControl
                {
                    Text = "Unbreakable",
                    Bounds =
                        new UniRectangle(_isUnbreakableCheckbox.Bounds.Right + 10,
                                        _isUnbreakableCheckbox.Bounds.Top, 110, 16)
                };

                _isLiquidCheckbox = new OptionControl
                {
                    Bounds =
                        new UniRectangle(SECOND_ROW_X, _isUnbreakableCheckbox.Bounds.Bottom + 10, 16, 16),
                    Selected = _blockEditorState.EditedBlock.IsLiquid
                };
                _isLiquidCheckbox.Changed += (sender, args) => ToggleLiquid();

                var liquidLabel = new LabelControl
                {
                    Text = "Liquid",
                    Bounds =
                        new UniRectangle(_isLiquidCheckbox.Bounds.Right + 10,
                                         _isLiquidCheckbox.Bounds.Top, 110, 16)
                };

                var blockHealthLabel = new LabelControl
                                       {
                                           Text = "Block HP:",
                                           Bounds =
                                               new UniRectangle(SECOND_ROW_X, _isLiquidCheckbox.Bounds.Bottom + 10, 60, 30)
                                       };

                _blockHealthControl = new NumberInputControl
                {
                    Bounds =
                        new UniRectangle(blockHealthLabel.Bounds.Right + 10,
                                        blockHealthLabel.Bounds.Top, 55, 30),
                    MinValue = 1,
                    MaxValue = 99999,
                    IsPositiveOnly = true,
                    Text = _blockEditorState.EditedBlock.Health.ToString()
                };

                _generalControls.Add(cubicalBlockCheckboxLabel);                
                _generalControls.Add(_isCubicalBlockCheckbox);
                _generalControls.Add(unbreakableLabel);
                _generalControls.Add(_isUnbreakableCheckbox);
                _generalControls.Add(liquidLabel);
                _generalControls.Add(_isLiquidCheckbox);
                _generalControls.Add(blockHealthLabel);
                _generalControls.Add(_blockHealthControl);
            }

            foreach (Control generalControl in _generalControls)
            {
                Panel.Children.Add(generalControl);
            }
        }

        private void AddItemDropsControls()
        {
            if (_itemDropControls == null)
            {
                _itemDropControls = new List<Control>();

                LoadItems();

                _spawnRules = new List<IItemLike>();
                _spawnRules.AddRange(_blockEditorState.EditedBlock.ItemDropRules);

                _itemDropsListControl = new ItemListControl(
                    _worldSettings, _parentGUI, SECOND_ROW_X, FIRST_COL_Y + 60, IconCellControl.ICON_CELL_SIZE * 2,
                    IconCellControl.ICON_CELL_SIZE * 2, _spawnRules,
                    control: Panel, containerList: _itemDropControls);

                _itemDropsListControl.Initialize();
                _itemDropsListControl.OnItemLeftClick += OnItemInventoryListClick;

                _addItemButton = new ButtonControl
                                 {
                                     Bounds =
                                         new UniRectangle(SECOND_ROW_X + IconCellControl.ICON_CELL_SIZE * 2 + 10,
                                                          FIRST_COL_Y + 60, 100, ButtonHeight),

                                     Text = "Add item"
                                 };
                _addItemButton.Pressed += OnAddItem;

                _trashCanIcon = Game.Content.Load<Texture2D>("Textures/trash");

                _trashCanCell = new IconCellControl
                                {
                                    Bounds = new UniRectangle(
                                        _addItemButton.Bounds.Left,
                                        _addItemButton.Bounds.Top + IconCellControl.ICON_CELL_SIZE,
                                        IconCellControl.ICON_CELL_SIZE, IconCellControl.ICON_CELL_SIZE),
                                    Icon = _trashCanIcon
                                };
                _trashCanCell.OnLeftMouseClick += OnTrashClick;

                _itemDropControls.Add(_addItemButton);
                _itemDropControls.Add(_trashCanCell);
            }

            foreach (Control control in _itemDropControls)
            {
                Panel.Children.Add(control);
            }
        }

        private void AddSoundControls()
        {
            if (_soundControls == null)
            {
                _soundControls = new List<Control>();

                _footstepSoundNames = new List<string>();

                var footstepsLabel = new LabelControl
                {
                    Text = "Footsteps:",
                    Bounds =
                        new UniRectangle(SECOND_ROW_X, FIRST_COL_Y + 60, 60, 30)
                };

                _footstepSoundsList = new ListControl
                {
                    Bounds = new UniRectangle(SECOND_ROW_X, footstepsLabel.Bounds.Bottom + 10, LIST_WIDTH, 170),
                    SelectionMode = ListSelectionMode.Single
                };

                _addFootstepSoundButton = new ButtonControl
                {
                    Text = "Add",
                    Bounds =
                        new UniRectangle(_footstepSoundsList.Bounds.Right + 10, _footstepSoundsList.Bounds.Top, ButtonWidth, ButtonHeight),
                };
                _addFootstepSoundButton.Pressed += (sender, args) => OnAddFootstepSound();

                _soundControls.Add(footstepsLabel);
                _soundControls.Add(_footstepSoundsList);
                _soundControls.Add(_addFootstepSoundButton);
            }

            foreach (Control generalControl in _soundControls)
            {
                Panel.Children.Add(generalControl);
            }
        }

        private void OnAddFootstepSound()
        {
            if (_soundPanelConstructor == null)
            {
                _soundPanelConstructor = new AddSoundPanel(_parentGUI, _worldSettings);
                _soundPanelConstructor.Start();

                _soundPanelConstructor.OnGoingBack += OnEffectPanelClosed;
                _soundPanelConstructor.OnAddition += OnEffectAddition;
            }

            _soundPanelConstructor.SoundTypeNumber = 1;

            _parentGUI.Screen.Desktop.Children.Remove(Panel);

            _parentGUI.Screen.Desktop.Children.Add(_soundPanelConstructor.SoundPanel);
        }

        private void OnEffectPanelClosed()
        {
            _parentGUI.Screen.Desktop.Children.Remove(_soundPanelConstructor.SoundPanel);

            _parentGUI.Screen.Desktop.Children.Add(Panel);
        }

        private void OnEffectAddition(string soundName, int soundTypeNumber)
        {
            if (soundTypeNumber == 1)
            {
                _footstepSoundNames.Add(soundName);
            }
        }

        private void OnTrashClick(string itemName)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                _itemMousePicker.PickedItem = null;
                _itemDropsListControl.UnclickEverything();
            }
        }

        private void OnAddItem(object sender, EventArgs eventArgs)
        {
            if (_spawnRules.Count >= 4)
            {
                _parentGUI.ShowAlertBox("Block item drops are already full.");
                return;
            }

            _parentGUI.Screen.Desktop.Children.Remove(Panel);
            _parentGUI.Screen.Desktop.Children.Add(ItemAdditionPanel);
        }

        private void ItemAdditionListClick(string itemName)
        {
            _itemMousePicker.PickedItem = _itemMousePicker.PickedItem == null ? _items.Find(item => item.Name == itemName) : null;

            _itemAdditionList.UnclickEverything();
        }

        private void OnItemInventoryListClick(string itemName)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                _spawnRules.Add(_itemMousePicker.PickedItem);
                _blockEditorState.EditedBlock.ItemDropRules.Add((SpawnedItemRule)_itemMousePicker.PickedItem);
                _itemMousePicker.PickedItem = null;
                _itemDropsListControl.UpdateItems(_spawnRules);
                _itemDropsListControl.UnclickEverything();
            }
            else
            {
                int index = _itemDropsListControl.GetClickedCellIndex();

                if (index != -1 && _blockEditorState.EditedBlock.ItemDropRules.Count > index)
                {
                    SpawnedItemRule rule = _blockEditorState.EditedBlock.ItemDropRules[index];

                    _itemMousePicker.PickedItem = rule;

                    _blockEditorState.EditedBlock.ItemDropRules.RemoveAt(index);
                    _spawnRules.Remove(_itemMousePicker.PickedItem);

                    _itemDropsListControl.UnclickEverything();
                    _itemDropsListControl.UpdateItems(_spawnRules);
                }
            }
        }

        private void BackToInventory()
        {
            _itemMousePicker.PickedItem = null;

            _parentGUI.Screen.Desktop.Children.Remove(ItemAdditionPanel);
            _parentGUI.Screen.Desktop.Children.Add(Panel);
        }

        private void AddItemAdditionPanel()
        {
            const int width = 700;
            const int height = 470;

            var panelTopX = (int)_parentGUI.Screen.Width / 2 - width / 2;
            var panelTopY = (int)_parentGUI.Screen.Height / 2 - height / 2;

            const int secondRowX = FIRST_ROW_X + IconCellControl.ICON_CELL_SIZE * 5 + 100;

            ItemAdditionPanel = new PanelControl
            {
                Bounds = new UniRectangle(panelTopX, panelTopY, width, height)
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

            _itemSelectionFilterInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(filterLabel.Bounds.Right + 10, FIRST_COL_Y + 50, 100, LabelHeight)
            };

            _itemSelectionFilterInput.OnTextChanged += OnItemSelectionFilterChanged;

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

            ItemAdditionPanel.Children.Add(filterLabel);
            ItemAdditionPanel.Children.Add(_itemSelectionFilterInput);

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

            _spawnRules.Add(spawnedItem);
            _blockEditorState.EditedBlock.ItemDropRules.Add(spawnedItem);
            _itemDropsListControl.UpdateItems(_spawnRules);

            BackToInventory();
        }

        private void ToggleFullBlock()
        {
            _isLiquidCheckbox.Selected = false;
            _blockEditorState.ToggleFullBlock(_isCubicalBlockCheckbox.Selected);
        }

        private void ToggleUnbreakable()
        {
            _blockEditorState.EditedBlock.IsUnbreakable = _isUnbreakableCheckbox.Selected;
        }

        private void ToggleLiquid()
        {
            _blockEditorState.ToggleLiquid(_isLiquidCheckbox.Selected);
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _itemMousePicker.Draw(gameTime, spriteBatch);
        }

        protected override void Back()
        {
            _blockEditorState.EditedBlock.Health = (short)_blockHealthControl.GetInt();

            if (_blockEditorState.EditedBlock.Health == 0)
            {
                _blockEditorState.EditedBlock.Health = 1;
            }

            _parentGUI.HideBlockOptionsPanel();
        }

        private void Unsubscribe()
        {
            if (_itemAdditionList != null)
            {
                _itemAdditionList.OnItemLeftClick -= ItemAdditionListClick;
            }
            if (_itemSelectionFilterInput != null)
            {
                _itemSelectionFilterInput.OnTextChanged -= OnItemSelectionFilterChanged;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Unsubscribe();

            _trashCanIcon.Dispose();

            _itemDropsListControl.Dispose();
            _trashCanCell.Dispose();
        }
    }
}