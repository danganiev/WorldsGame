using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.GUI.ModelEditor;
using WorldsGame.Saving;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.GUI.TexturePackEditor
{
    internal class TextureListPopupGUI : View.GUI.GUI
    {
        private readonly EditedModel _editedBlock;
        private readonly WorldSettings _worldSettings;
        private readonly ModelEditorGUIBase _parentGUI;

        internal PanelControl Panel { get; private set; }

        private LabelControl _titleLabel;
        private ListControl _textureList;
        private BaseWorldsTextControl _filterInput;
        //        private GraphicsDevice _graphicsDevice;
        //        private BaseWorldSublistGUI<Texture> _internalGUI;

        private List<string> _fullList;

        private bool IsModelEditor { get { return _editedBlock != null; } }

        private bool IsTextureSelected
        {
            get { return _textureList.SelectedItems.Count != 0; }
        }

        private string SelectedName { get { return _textureList.SelectedName(); } }

        internal TextureListPopupGUI(WorldsGame game)
        {
            Game = game;
            //            _graphicsDevice = Game.GraphicsDevice;
            viewport = Game.GraphicsDevice.Viewport;
        }

        private TextureListPopupGUI(WorldsGame game, WorldSettings worldSettings, ModelEditorGUIBase parentGUI)
            : this(game)
        {
            _worldSettings = worldSettings;
            _parentGUI = parentGUI;
            //            _internalGUI = Texture.GetListGUI(game, worldSettings);
            //            internalGUI.CreateControlsFromOutside();
        }

        internal TextureListPopupGUI(WorldsGame game, WorldSettings worldSettings, ModelEditorGUIBase parentGUI, EditedModel editedBlock)
            : this(game, worldSettings, parentGUI)
        {
            _editedBlock = editedBlock;
        }

        internal void GenerateControls()
        {
            AddLabelPanel();
        }

        protected override void LoadData()
        {
            //            _internalGUI.LoadDataFromOutside();

            LoadList(_textureList, Texture.SaverHelper(_worldSettings.Name));

            _fullList = new List<string>(_textureList.Items);
        }

        private void AddLabelPanel()
        {
            const int Y = 15;
            const int X = 30;
            const int elementListWidth = 230;
            const int elementListHeight = 370;
            const int width = 550;

            Panel = new PanelControl
            {
                Bounds = new UniRectangle(_parentGUI.Screen.Width / 2 - width / 2, 100, width, _parentGUI.Screen.Height - 200)
            };

            _titleLabel = new LabelControl
            {
                Text = "Select texture:",
                Bounds = new UniRectangle(X, Y, 110, _parentGUI.LabelHeight)
            };

            var filterLabel = new LabelControl
            {
                Bounds = new UniRectangle(X, _titleLabel.Bounds.Bottom + 10, 40, _parentGUI.LabelHeight),
                Text = "Filter:"
            };

            _filterInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(filterLabel.Bounds.Right + 10, _titleLabel.Bounds.Bottom + 10, elementListWidth - (filterLabel.Bounds.Right - filterLabel.Bounds.Left) - 10, 30)
            };
            _filterInput.OnTextChanged += FilterData;

            _textureList = new ListControl
            {
                Bounds = new UniRectangle(30, _filterInput.Bounds.Bottom + 10, elementListWidth, elementListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            var createTextureButton = new ButtonControl
            {
                Text = "Create",
                Bounds = new UniRectangle(_textureList.Bounds.Right + 10, _textureList.Bounds.Top, 70, 30)
                //                Bounds = new UniRectangle(new UniScalar(Panel.Bounds.Right - 30, Panel.Bounds.Bottom - 20, 100, 30)
            };
            createTextureButton.Pressed += (sender, args) => CreateTexture();

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 70)), new UniScalar(1f, -(X + 30)), 70, 30)
                //                Bounds = new UniRectangle(new UniScalar(Panel.Bounds.Right - 30, Panel.Bounds.Bottom - 20, 100, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            var selectButton = new ButtonControl
            {
                Text = "Select",
                Bounds = new UniRectangle(backButton.Bounds.Left - 110, new UniScalar(1f, -(X + 30)), 100, 30)
                //                Bounds = new UniRectangle(new UniScalar(Panel.Bounds.Right - 30, Panel.Bounds.Bottom - 20, 100, 30)
            };
            selectButton.Pressed += (sender, args) => SelectTexture();

            Panel.Children.Add(_titleLabel);
            Panel.Children.Add(filterLabel);
            Panel.Children.Add(_filterInput);
            Panel.Children.Add(_textureList);
            Panel.Children.Add(createTextureButton);
            Panel.Children.Add(backButton);
            Panel.Children.Add(selectButton);
            //            _labelPanel.Children.Add(helpLabel);
        }

        private Texture GetSelectedTexture()
        {
            return Texture.Load(_worldSettings.Name, SelectedName);//.GetTexture(_graphicsDevice);
        }

        private void CreateTexture()
        {
            ModelEditorType editorType = ModelEditorType.None;

            if (Game.GameStateManager.ActiveState.GetType() == typeof(ItemEditorState))
            {
                editorType = ModelEditorType.Item;
            }
            else if (Game.GameStateManager.ActiveState.GetType() == typeof(BlockEditorState))
            {
                editorType = ModelEditorType.Block;
            }
            else if (Game.GameStateManager.ActiveState.GetType() == typeof(CharacterEditorState))
            {
                editorType = ModelEditorType.Character;
            }

            Game.GameStateManager.Push(new TextureDrawingState(Game, _worldSettings, editorType));
        }

        private void SelectTexture()
        {
            if (!IsTextureSelected)
            {
                _parentGUI.ShowAlertBox("Please select a texture first.");
                return;
            }

            if (IsModelEditor)
            {
                _editedBlock.SetTextureToSelectedSide(GetSelectedTexture());
                Back();
            }
        }

        internal override void Start()
        {
            GenerateControls();
            LoadData();
        }

        private void FilterData(string filterText)
        {
            _textureList.Items.Clear();
            IEnumerable<string> newList = from element in _fullList where element.ToLowerInvariant().Contains(filterText.ToLowerInvariant()) select element;

            foreach (string s in newList)
            {
                _textureList.Items.Add(s);
            }
        }

        protected override void Back()
        {
            if (_editedBlock != null)
            {
                _parentGUI.HideTextureSelectionPanel();
            }
        }
    }
}