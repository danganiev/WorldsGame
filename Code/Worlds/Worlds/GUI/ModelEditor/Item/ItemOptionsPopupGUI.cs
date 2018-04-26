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
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.DataClasses.Items;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;
using Effect = WorldsGame.Saving.DataClasses.Effect;

namespace WorldsGame.GUI.ModelEditor
{
    internal class ItemOptionsPopupGUI : View.GUI.GUI
    {
        private const int PANEL_WIDTH = 900;
        private const int PANEL_HEIGHT = 500;
        private const int LIST_WIDTH = 160;

        private const int LABEL_MARGIN = 80;

        private readonly EditedModel _editedItemModel;
        private readonly ItemEditorState _itemEditorState;
        private readonly WorldSettings _worldSettings;
        private readonly ModelEditorGUIBase _parentGUI;

        private Texture2D _iconTexture;
        private Color[] _iconColors;
        private bool _isIconEdited;

        // Spritebatch is defined here because I didn't wanted to bother
        private readonly SpriteBatch _spriteBatch;

        private LabelControl _titleLabel;
        private LabelControl _iconLabel;
        private ButtonControl _iconEditButton;
        private int _panelStartX;
        private int _panelStartY;
        private NumberInputControl _stackQuantityInput;

        //        private ListControl _itemTypesList;
        private ListControl _itemQualitiesList;

        private ListControl _action1EffectsList;

        private List<Effect> _action1Effects;

        private ListControl _action2EffectsList;
        private List<Effect> _action2Effects;

        private ButtonControl _addAction1EffectButton;
        private ButtonControl _addAction2EffectButton;

        private ButtonControl _removeAction1EffectButton;
        private ButtonControl _removeAction2EffectButton;

        private ListControl _action1TypesList;
        private ListControl _action2TypesList;

        private LabelControl _effectTitleLabel;

        private AddEffectPanel _effectPanelConstructor;

        protected override int ListHeight { get { return 70; } }

        internal PanelControl MainPanel { get; private set; }

        internal PanelControl EffectPanel { get; private set; }

        internal string IconLabelText
        {
            get
            {
                if (_iconTexture != null)
                {
                    return "Icon:";
                }
                return "Icon: None";
            }
        }

        private bool IsNewItem { get { return _itemEditorState.IsNewItem; } }

        internal ItemOptionsPopupGUI(
            WorldsGame game, WorldSettings worldSettings, ModelEditorGUIBase parentGUI,
            EditedModel editedItemModel, ItemEditorState itemEditorState)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;
            _worldSettings = worldSettings;
            _parentGUI = parentGUI;
            _editedItemModel = editedItemModel;
            _itemEditorState = itemEditorState;
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            _iconColors = _editedItemModel.IconTextureColors;

            _action1Effects = new List<Effect>();
            _action2Effects = new List<Effect>();

            if (_iconColors != null)
            {
                _iconTexture = new Texture2D(game.GraphicsDevice, Icon.SIZE, Icon.SIZE);
                _iconTexture.SetData(_iconColors);
            }
        }

        internal void GenerateControls()
        {
            AddMainPanel();
            AddIconControls();
            AddQuantity();
            AddQuality();
            //            AddItemType();
            AddActionsAndEffects();

            AddEffectPanel();
            AddEffectControls();
        }

        protected override void LoadData()
        {
            LoadPopupList(_itemQualitiesList, ItemEnumNaming.GetItemQualityNamesWithPopups());            

            LoadList(_action1TypesList, from actionType in ItemUtils.GetSelectableItemActionTypes() select actionType.ToString());
            LoadList(_action2TypesList, from actionType in ItemUtils.GetSelectableItemActionTypes() select actionType.ToString());

            _action1Effects.AddRange(_itemEditorState.Action1Effects);
            _action2Effects.AddRange(_itemEditorState.Action2Effects);

            foreach (Effect action1Effect in _action1Effects)
            {
                _action1EffectsList.Items.Add(string.Format("{0} '{1}' ({2})",
                    EffectNaming.GetEffectNames()[(int)action1Effect.EffectType], action1Effect.ApplicantName, action1Effect.Value));
            }

            foreach (Effect action2Effect in _action2Effects)
            {
                _action2EffectsList.Items.Add(string.Format("{0} '{1}' ({2})",
                    EffectNaming.GetEffectNames()[(int)action2Effect.EffectType], action2Effect.ApplicantName, action2Effect.Value));
            }
        }

        private void AddMainPanel()
        {
            const int Y = 15;
            const int X = 30;

            _panelStartX = (int)_parentGUI.Screen.Width / 2 - PANEL_WIDTH / 2;
            _panelStartY = (int)_parentGUI.Screen.Height / 2 - PANEL_HEIGHT / 2;
            MainPanel = new PanelControl
            {
                Bounds = new UniRectangle(_panelStartX, _panelStartY, PANEL_WIDTH, PANEL_HEIGHT)
            };

            _titleLabel = new LabelControl
            {
                Text = "Item options",
                Bounds = new UniRectangle(X, Y, 130, _parentGUI.LabelHeight),
                IsHeader = true
            };
            var saveButton = new ButtonControl
            {
                Text = "Save",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 150)), new UniScalar(1f, -(X + 30)), 70, 30)
                //                Bounds = new UniRectangle(new UniScalar(Panel.Bounds.Right - 30, Panel.Bounds.Bottom - 20, 100, 30)
            };
            saveButton.Pressed += (sender, args) => Save();

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 70)), new UniScalar(1f, -(X + 30)), 70, 30)
                //                Bounds = new UniRectangle(new UniScalar(Panel.Bounds.Right - 30, Panel.Bounds.Bottom - 20, 100, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            MainPanel.Children.Add(_titleLabel);
            MainPanel.Children.Add(saveButton);
            MainPanel.Children.Add(backButton);

            MainPanel.BringToFront();
        }

        private void AddIconControls()
        {
            var Y = _titleLabel.Bounds.Bottom + 30;
            _iconLabel = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 70, LabelHeight),
                Text = IconLabelText
            };

            _iconEditButton = new ButtonControl
            {
                Bounds = new UniRectangle(_iconLabel.Bounds.Right + LABEL_MARGIN, Y, ButtonWidth, ButtonHeight),
                Text = "Edit"
            };
            _iconEditButton.Pressed += (sender, args) => EditIcon();

            MainPanel.Children.Add(_iconLabel);
            MainPanel.Children.Add(_iconEditButton);
        }

        private void EditIcon()
        {
            _isIconEdited = true;
            Game.GameStateManager.Push(new TextureDrawingState(Game, _worldSettings, _iconColors, this));
        }

        private void AddQuantity()
        {
            var Y = _iconLabel.Bounds.Bottom + 10;

            var stackQuantityLabel = new LabelControl
            {
                Bounds = new UniRectangle(_iconLabel.Bounds.Left, Y, 70, LabelHeight),
                Text = "Stack quantity:"
            };

            _stackQuantityInput = new NumberInputControl
            {
                Bounds = new UniRectangle(stackQuantityLabel.Bounds.Right + LABEL_MARGIN, Y, ButtonWidth, ButtonHeight),
                Text = IsNewItem ? "64" : _itemEditorState.MaxStackCount.ToString(),
                MaxValue = 64,
                MinValue = 0
            };

            MainPanel.Children.Add(stackQuantityLabel);
            MainPanel.Children.Add(_stackQuantityInput);
        }

        private void AddQuality()
        {
            var Y = _stackQuantityInput.Bounds.Bottom + 10;

            var label = new LabelControl
            {
                Bounds = new UniRectangle(_iconLabel.Bounds.Left, Y, 70, LabelHeight),
                Text = "Item quality:"
            };

            _itemQualitiesList = new ListControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };
            _itemQualitiesList.SelectedItems.Add((int)_itemEditorState.ItemQuality);

            MainPanel.Children.Add(label);
            MainPanel.Children.Add(_itemQualitiesList);
        }

        private void AddActionsAndEffects()
        {
            var Y = _itemQualitiesList.Bounds.Bottom + 10;

            var action1Label = new LabelControl
            {
                Bounds = new UniRectangle(_iconLabel.Bounds.Left, Y, 70, 20),
                Text = "Action 1 (Left Click):"
            };

            _action1TypesList = new ListControl
            {
                Bounds = new UniRectangle(action1Label.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            var action1EffectsLabel = new LabelControl
            {
                Bounds = new UniRectangle(_iconLabel.Bounds.Left, _action1TypesList.Bounds.Bottom + 10, 70, 20),
                Text = "Action 1 effects:"
            };

            _action1EffectsList = new ListControl
            {
                Bounds = new UniRectangle(action1EffectsLabel.Bounds.Right + LABEL_MARGIN, _action1TypesList.Bounds.Bottom + 10, LIST_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            _addAction1EffectButton = new ButtonControl
            {
                Text = "Add",
                Bounds =
                    new UniRectangle(_action1EffectsList.Bounds.Right + 10, _action1EffectsList.Bounds.Top, ButtonWidth, ButtonHeight),
            };
            _addAction1EffectButton.Pressed += (sender, args) => OnAddActionEffect(1);

            _removeAction1EffectButton = new ButtonControl
            {
                Text = "Remove",
                Bounds =
                    new UniRectangle(_action1EffectsList.Bounds.Right + 10, _addAction1EffectButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
            };

            var action2Label = new LabelControl
            {
                Bounds = new UniRectangle(_addAction1EffectButton.Bounds.Right + 30, Y, 70, 20),
                Text = "Action 2 (Right Click):"
            };

            _action2TypesList = new ListControl
            {
                Bounds = new UniRectangle(action2Label.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            var action2EffectsLabel = new LabelControl
            {
                Bounds = new UniRectangle(_addAction1EffectButton.Bounds.Right + 30, _action2TypesList.Bounds.Bottom + 10, 70, 20),
                Text = "Action 2 effects:"
            };

            _action2EffectsList = new ListControl
            {
                Bounds = new UniRectangle(action2EffectsLabel.Bounds.Right + LABEL_MARGIN, _action2TypesList.Bounds.Bottom + 10, LIST_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            _addAction2EffectButton = new ButtonControl
            {
                Text = "Add",
                Bounds =
                    new UniRectangle(_action2EffectsList.Bounds.Right + 10, _action2EffectsList.Bounds.Top, ButtonWidth, ButtonHeight),
            };
            _addAction2EffectButton.Pressed += (sender, args) => OnAddActionEffect(2);

            _removeAction2EffectButton = new ButtonControl
            {
                Text = "Remove",
                Bounds =
                    new UniRectangle(_action2EffectsList.Bounds.Right + 10, _addAction2EffectButton.Bounds.Bottom + 10, ButtonWidth, ButtonHeight),
            };

            MainPanel.Children.Add(action1Label);
            MainPanel.Children.Add(_action1TypesList);
            MainPanel.Children.Add(action1EffectsLabel);
            MainPanel.Children.Add(_action1EffectsList);
            MainPanel.Children.Add(_addAction1EffectButton);
            MainPanel.Children.Add(_removeAction1EffectButton);

            MainPanel.Children.Add(action2Label);
            MainPanel.Children.Add(_action2TypesList);
            MainPanel.Children.Add(action2EffectsLabel);
            MainPanel.Children.Add(_action2EffectsList);
            MainPanel.Children.Add(_addAction2EffectButton);
            MainPanel.Children.Add(_removeAction2EffectButton);
        }

        private void AddEffectPanel()
        {
            const int Y = 15;
            const int X = 30;

            var panelStartX = (int)_parentGUI.Screen.Width / 2 - PANEL_WIDTH / 2;
            var panelStartY = (int)_parentGUI.Screen.Height / 2 - PANEL_HEIGHT / 2;

            EffectPanel = new PanelControl
            {
                Bounds = new UniRectangle(panelStartX, panelStartY, PANEL_WIDTH, PANEL_HEIGHT)
            };

            _effectTitleLabel = new LabelControl
            {
                Text = "Add effect",
                Bounds = new UniRectangle(X, Y, 130, _parentGUI.LabelHeight),
                IsHeader = true
            };

            EffectPanel.Children.Add(_effectTitleLabel);
        }

        private void AddEffectControls()
        {
            var Y = _effectTitleLabel.Bounds.Bottom + 30;
            var label = new LabelControl
            {
                Text = "Add effect",
                Bounds = new UniRectangle(_effectTitleLabel.Bounds.Left, Y, 130, _parentGUI.LabelHeight),
            };

            EffectPanel.Children.Add(label);
            //            EffectPanel.Children.Add(_iconEditButton);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="actionNumber">Can only be 1 or 2</param>
        private void OnAddActionEffect(int actionNumber)
        {
            if (_effectPanelConstructor == null)
            {
                _effectPanelConstructor = new AddEffectPanel(_parentGUI, _worldSettings);
                _effectPanelConstructor.Start();

                _effectPanelConstructor.OnGoingBack += OnEffectPanelClosed;
                _effectPanelConstructor.OnAddition += OnEffectAddition;
            }

            _effectPanelConstructor.ActionNumber = actionNumber;

            _parentGUI.Screen.Desktop.Children.Remove(MainPanel);

            _parentGUI.Screen.Desktop.Children.Add(_effectPanelConstructor.EffectPanel);
        }

        private void OnEffectPanelClosed()
        {
            _parentGUI.Screen.Desktop.Children.Remove(_effectPanelConstructor.EffectPanel);

            _parentGUI.Screen.Desktop.Children.Add(MainPanel);
        }

        private void OnEffectAddition(Effect effect, int actionNumber)
        {
            if (actionNumber == 1)
            {
                _action1Effects.Add(effect);

                _action1EffectsList.Items.Add(string.Format("{0} '{1}' ({2})",
                    EffectNaming.GetEffectNames()[(int)effect.EffectType], effect.ApplicantName, effect.Value));
            }
            else if (actionNumber == 2)
            {
                _action2Effects.Add(effect);

                _action2EffectsList.Items.Add(string.Format("{0} '{1}' ({2})",
                    EffectNaming.GetEffectNames()[(int)effect.EffectType], effect.ApplicantName, effect.Value));
            }
        }

        internal void SetIconColors(Color[] colors)
        {
            _editedItemModel.SetIconColors(colors);
            _isIconEdited = false;
        }

        internal void Draw(GameTime gameTime)
        {
            if (!_isIconEdited && _iconTexture != null &&
                (_effectPanelConstructor == null || !_parentGUI.Screen.Desktop.Children.Contains(_effectPanelConstructor.EffectPanel)))
            {
                _spriteBatch.Begin(
                    SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                    DepthStencilState.Default, RasterizerState.CullNone);

                _spriteBatch.Draw(
                    _iconTexture, new Rectangle(_panelStartX + 30 + 50, _panelStartY + 30 + 30 + 20, Icon.SIZE, Icon.SIZE), Color.White);

                _spriteBatch.End();
            }
        }

        internal override void Start()
        {
            GenerateControls();
            LoadData();
        }

        internal void SetOptionsData()
        {
            if (!_itemQualitiesList.IsSelected())
            {
                _parentGUI.ShowAlertBox("Item quality has to be selected");
            }

            _itemEditorState.MaxStackCount = _stackQuantityInput.GetInt();
            _itemEditorState.Action1Effects.Clear();
            _itemEditorState.Action1Effects.AddRange(_action1Effects);
            _itemEditorState.Action2Effects.Clear();
            _itemEditorState.Action2Effects.AddRange(_action2Effects);
            _itemEditorState.ItemQuality = (ItemQuality)_itemQualitiesList.SelectedItems[0];
            
        }

        protected override void Save()
        {
            SetOptionsData();

            Back();
        }

        protected override void Back()
        {
            _parentGUI.HideOptionsPanel();
        }

        public override void Dispose()
        {
            _spriteBatch.Dispose();

            if (_iconTexture != null)
            {
                _iconTexture.Dispose();
            }

            base.Dispose();
        }
    }
}