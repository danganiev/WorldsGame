using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.GUI.TexturePackEditor;
using WorldsGame.Saving;

namespace WorldsGame.GUI.ModelEditor
{
    internal class ModelBlockEditorGUI : ModelEditorGUIBase
    {
        private readonly WorldSettings _worldSettings;
        private readonly BlockEditorState _blockEditorState;

        //        private ButtonControl _blockOptionsButton;

        private BlockOptionsPopupGUI _blockOptionsPopupGUI;

        internal bool IsMenuOpen { get { return textureListPopupGUI != null || _blockOptionsPopupGUI != null; } }

        internal EditedModel EditedBlock { get; set; }

        internal ModelBlockEditorGUI(WorldsGame game, EditedModel editedBlock, WorldSettings worldSettings, BlockEditorState blockEditorState)
            : base(game)
        {
            EditedBlock = editedBlock;
            _worldSettings = worldSettings;
            _blockEditorState = blockEditorState;
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddBlockOptionsButton();
        }

        private void AddBlockOptionsButton()
        {
            var buttonX = Screen.Desktop.Bounds.Left + 100;
            var buttonY = Screen.Desktop.Bounds.Top + 30;

            var blockOptionsButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, buttonY, 120, ButtonHeight),
                Text = "Block options"
            };
            blockOptionsButton.Pressed += (sender, args) => ShowBlockOptions();
            pressableControls.Add(blockOptionsButton);

            var selectTextureButton = new ButtonControl
            {
                Bounds = new UniRectangle(blockOptionsButton.Bounds.Right + 10, buttonY, 120, ButtonHeight),
                Text = "Select texture"
            };
            selectTextureButton.Pressed += (sender, args) => SelectTexture();
            pressableControls.Add(selectTextureButton);

            var rotateTextureButton = new ButtonControl
            {
                Bounds = new UniRectangle(selectTextureButton.Bounds.Right + 10, buttonY, 120, ButtonHeight),
                Text = "Rotate texture"
            };
            rotateTextureButton.Pressed += (sender, args) => RotateTexture();
            pressableControls.Add(rotateTextureButton);

            var addPartButton = new ButtonControl
            {
                Bounds = new UniRectangle(rotateTextureButton.Bounds.Right + 30, buttonY, 80, ButtonHeight),
                Text = "Add part"
            };
            addPartButton.Pressed += (sender, args) => AddPart();
            pressableControls.Add(addPartButton);

            var removePartButton = new ButtonControl
            {
                Bounds = new UniRectangle(addPartButton.Bounds.Right + 10, buttonY, 100, ButtonHeight),
                Text = "Remove part"
            };
            removePartButton.Pressed += (sender, args) => RemovePart();
            pressableControls.Add(removePartButton);

            var copyPartButton = new ButtonControl
            {
                Bounds = new UniRectangle(removePartButton.Bounds.Right + 10, buttonY, 100, ButtonHeight),
                Text = "Copy part"
            };
            copyPartButton.Pressed += (sender, args) => CopyPart();
            pressableControls.Add(copyPartButton);

            Screen.Desktop.Children.Add(blockOptionsButton);
            Screen.Desktop.Children.Add(selectTextureButton);
            Screen.Desktop.Children.Add(rotateTextureButton);
            Screen.Desktop.Children.Add(addPartButton);
            Screen.Desktop.Children.Add(removePartButton);
            Screen.Desktop.Children.Add(copyPartButton);
        }

        private void ShowBlockOptions()
        {
            DisableControls();
            _blockOptionsPopupGUI = new BlockOptionsPopupGUI(Game, _worldSettings, this, EditedBlock, _blockEditorState);
            _blockOptionsPopupGUI.Start();
            Screen.Desktop.Children.Add(_blockOptionsPopupGUI.Panel);
        }

        private void SelectTexture()
        {
            if (!EditedBlock.IsSideSelected)
            {
                ShowAlertBox("To set a texture to the side, you need to select a side first.");
            }
            else
            {
                DisableControls();
                textureListPopupGUI = new TextureListPopupGUI(Game, _worldSettings, this, EditedBlock);
                textureListPopupGUI.Start();
                Screen.Desktop.Children.Add(textureListPopupGUI.Panel);
            }
        }

        private void RotateTexture()
        {
            EditedBlock.RotateSelectedTexture();
        }

        private void AddPart()
        {
            EditedBlock.AddNewCuboid();
        }

        private void RemovePart()
        {
            EditedBlock.RemoveSelectedCuboid();
        }

        private void CopyPart()
        {
            EditedBlock.CopySelectedCuboid();
        }

        public void OnEscape()
        {
            if (textureListPopupGUI != null)
            {
                HideTextureSelectionPanel();
            }
            if (_blockOptionsPopupGUI != null)
            {
                HideBlockOptionsPanel();
            }
        }

        internal void HideBlockOptionsPanel()
        {
            Screen.Desktop.Children.Remove(_blockOptionsPopupGUI.Panel);
            _blockOptionsPopupGUI = null;
            EnableControls();
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.DrawAfterGUI(gameTime, spriteBatch);

            if (_blockOptionsPopupGUI != null)
            {
                _blockOptionsPopupGUI.DrawAfterGUI(gameTime, spriteBatch);
            }
        }
    }
}