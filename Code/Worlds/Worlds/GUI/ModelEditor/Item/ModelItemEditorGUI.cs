using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.GUI.TexturePackEditor;
using WorldsGame.Saving;

namespace WorldsGame.GUI.ModelEditor
{
    internal class ModelItemEditorGUI : ModelEditorGUIBase
    {
        private readonly WorldSettings _worldSettings;
        private readonly ItemEditorState _itemEditorState;

        private ItemOptionsPopupGUI _optionsPopupGUI;
        private AnimationEditorPopupGUI _animationEditorPopupGUI;

        private List<Control> _mainControls;
        private List<Control> _keyframeControls;
        private List<Control> _animationPlayControls;
        private List<Control> _firstPersonModeControls;
        private List<Control> _sideSelectedControls;

        private ButtonControl _playAnimationButton;

        private ButtonControl _saveAndPlayAnimationButton;

        internal bool IsMenuOpen { get { return textureListPopupGUI != null || _optionsPopupGUI != null; } }

        internal EditedModel EditedItem { get { return _itemEditorState.IsFirstPersonModeOn ? _itemEditorState.FPItemModel : _itemEditorState.EditedItemModel; } }

        internal ModelItemEditorGUI(WorldsGame game, WorldSettings worldSettings, ItemEditorState itemEditorState /*EditedModel editedItem,*/)
            : base(game)
        {
            //            EditedItem = editedItem;
            _worldSettings = worldSettings;
            _itemEditorState = itemEditorState;
            _mainControls = new List<Control>();
            _keyframeControls = new List<Control>();
            _animationPlayControls = new List<Control>();
            _firstPersonModeControls = new List<Control>();
            _sideSelectedControls = new List<Control>();
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddButtons();

            ShowMenu();
        }

        private void AddButtons()
        {
            var buttonX = Screen.Desktop.Bounds.Left + 100;
            var buttonY = Screen.Desktop.Bounds.Top + 30;

            var itemOptionsButton = new ButtonControl
                                     {
                                         Bounds = new UniRectangle(buttonX, buttonY, 180, ButtonHeight),
                                         Text = "Item properties"
                                     };
            itemOptionsButton.Pressed += (sender, args) => ShowItemOptions();
            pressableControls.Add(itemOptionsButton);

            var selectTextureButton = new ButtonControl
                                      {
                                          Bounds = new UniRectangle(itemOptionsButton.Bounds.Right + 10, buttonY, 120, ButtonHeight),
                                          Text = "Select texture"
                                      };
            selectTextureButton.Pressed += (sender, args) => SelectTexture();
            pressableControls.Add(selectTextureButton);
            _sideSelectedControls.Add(selectTextureButton);

            var rotateTextureButton = new ButtonControl
                                      {
                                          Bounds = new UniRectangle(selectTextureButton.Bounds.Right + 10, buttonY, 120, ButtonHeight),
                                          Text = "Rotate texture"
                                      };
            rotateTextureButton.Pressed += (sender, args) => RotateTexture();
            pressableControls.Add(rotateTextureButton);
            _sideSelectedControls.Add(rotateTextureButton);

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
            _sideSelectedControls.Add(removePartButton);

            var copyPartButton = new ButtonControl
            {
                Bounds = new UniRectangle(removePartButton.Bounds.Right + 10, buttonY, 100, ButtonHeight),
                Text = "Copy part"
            };
            copyPartButton.Pressed += (sender, args) => CopyPart();
            pressableControls.Add(copyPartButton);
            _sideSelectedControls.Add(copyPartButton);

            var firstPersonModeButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, itemOptionsButton.Bounds.Bottom + 10, 180, ButtonHeight),
                Text = "Toggle 1st person mode"
            };
            firstPersonModeButton.Pressed += (sender, args) => ToggleFirstPersonMode();
            pressableControls.Add(firstPersonModeButton);

            var thirdPersonModeButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, itemOptionsButton.Bounds.Bottom + 10, 180, ButtonHeight),
                Text = "Toggle 3rd person mode"
            };
            thirdPersonModeButton.Pressed += (sender, args) => ToggleThirdPersonMode();
            pressableControls.Add(thirdPersonModeButton);
            _firstPersonModeControls.Add(thirdPersonModeButton);

            var animationPopupButton = new ButtonControl
            {
                Bounds = new UniRectangle(thirdPersonModeButton.Bounds.Right + 10, itemOptionsButton.Bounds.Bottom + 10, 180, ButtonHeight),
                Text = "First person animations"
            };
            animationPopupButton.Pressed += (sender, args) => ShowFPAnimationPopup();
            pressableControls.Add(animationPopupButton);
            _firstPersonModeControls.Add(animationPopupButton);

            var saveDefaultPositionButton = new ButtonControl
            {
                Bounds = new UniRectangle(itemOptionsButton.Bounds.Right + 10, buttonY, 180, ButtonHeight),
                Text = "Save default item"
            };
            saveDefaultPositionButton.Pressed += (sender, args) => SaveDefaultItem();
            pressableControls.Add(saveDefaultPositionButton);
            _firstPersonModeControls.Add(saveDefaultPositionButton);

            _mainControls.Add(itemOptionsButton);
            _mainControls.Add(selectTextureButton);
            _mainControls.Add(rotateTextureButton);
            _mainControls.Add(addPartButton);
            _mainControls.Add(removePartButton);
            _mainControls.Add(copyPartButton);
            _mainControls.Add(firstPersonModeButton);

            _firstPersonModeControls.Add(itemOptionsButton);

            _saveAndPlayAnimationButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, buttonY, 200, ButtonHeight),
                Text = "Save and play animation"
            };
            _saveAndPlayAnimationButton.Pressed += (sender, args) => PlayFullAnimation(true);
            pressableControls.Add(_saveAndPlayAnimationButton);

            var backToAnimationButton = new ButtonControl
            {
                Bounds = new UniRectangle(_saveAndPlayAnimationButton.Bounds.Right + 10, buttonY, 200, ButtonHeight),
                Text = "Save keyframe"
            };
            backToAnimationButton.Pressed += (sender, args) => SaveKeyframe();
            pressableControls.Add(backToAnimationButton);

            _keyframeControls.Add(_saveAndPlayAnimationButton);
            _keyframeControls.Add(backToAnimationButton);

            _playAnimationButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, buttonY, 150, ButtonHeight),
                Text = "Play animation"
            };
            _playAnimationButton.Pressed += (sender, args) => PlayFullAnimation();
            pressableControls.Add(_playAnimationButton);

            var backToAnimationButton2 = new ButtonControl
            {
                Bounds = new UniRectangle(_playAnimationButton.Bounds.Right + 10, buttonY, 80, ButtonHeight),
                Text = "Back"
            };
            backToAnimationButton2.Pressed += (sender, args) => ToggleFirstPersonMode();
            pressableControls.Add(backToAnimationButton2);

            _animationPlayControls.Add(_playAnimationButton);
            _animationPlayControls.Add(backToAnimationButton2);
        }

        private void DisableModelModification()
        {
            EditedItem.DeselectEverything();
            EditedItem.IsSelectable = false;
        }

        private void EnableModelModification()
        {
            //            EditedItem.DeselectEverything();
            EditedItem.IsSelectable = true;
        }

        private void HideAllMenus()
        {
            HideMainMenu();
            HideAnimationPlayMenu();
            HideKeyframeMenu();
            HideFirstPersonModeMenu();
        }

        internal void ShowMenu()
        {
            if (_itemEditorState.IsFirstPersonModeOn)
            {
                ShowFirstPersonModeMenu();
            }
            else
            {
                ShowMainMenu();
            }
        }

        private void ShowMainMenu()
        {
            HideAllMenus();
            EnableModelModification();
            foreach (Control mainControl in _mainControls)
            {
                if (!Screen.Desktop.Children.Contains(mainControl))
                {
                    Screen.Desktop.Children.Add(mainControl);
                }
            }
        }

        private void HideMainMenu()
        {
            foreach (Control mainControl in _mainControls)
            {
                Screen.Desktop.Children.Remove(mainControl);
            }
        }

        internal override void ShowAnimationPlayMenu()
        {
            HideAllMenus();
            DisableModelModification();
            foreach (Control control in _animationPlayControls)
            {
                if (!Screen.Desktop.Children.Contains(control))
                {
                    Screen.Desktop.Children.Add(control);
                }
            }
        }

        private void HideAnimationPlayMenu()
        {
            foreach (Control control in _animationPlayControls)
            {
                Screen.Desktop.Children.Remove(control);
            }
        }

        internal override void ShowKeyframeMenu()
        {
            HideAllMenus();
            EnableModelModification();
            EditedItem.IsBeingTransformedForAnimation = true;
            foreach (Control control in _keyframeControls)
            {
                if (!Screen.Desktop.Children.Contains(control))
                {
                    Screen.Desktop.Children.Add(control);
                }
            }
        }

        private void HideKeyframeMenu()
        {
            EditedItem.IsBeingTransformedForAnimation = false;
            foreach (Control control in _keyframeControls)
            {
                Screen.Desktop.Children.Remove(control);
            }
        }

        private void SaveKeyframe()
        {
            _itemEditorState.SaveCurrentKeyframe();
            ShowFPAnimationPopup(isInitialized: false);
        }

        private void ShowFirstPersonModeMenu()
        {
            HideAllMenus();
            EnableModelModification();
            foreach (Control control in _firstPersonModeControls)
            {
                if (!Screen.Desktop.Children.Contains(control))
                {
                    Screen.Desktop.Children.Add(control);
                }
            }
        }

        private void HideFirstPersonModeMenu()
        {
            foreach (Control control in _firstPersonModeControls)
            {
                Screen.Desktop.Children.Remove(control);
            }
        }

        private void ShowItemOptions()
        {
            DisableControls();
            _optionsPopupGUI = new ItemOptionsPopupGUI(Game, _worldSettings, this, EditedItem, _itemEditorState);
            _optionsPopupGUI.Start();
            Screen.Desktop.Children.Add(_optionsPopupGUI.MainPanel);
        }

        private void SelectTexture()
        {
            if (!EditedItem.IsSideSelected)
            {
                ShowAlertBox("To set a texture to the side, you need to select a side first.");
            }
            else
            {
                DisableControls();
                textureListPopupGUI = new TextureListPopupGUI(Game, _worldSettings, this, EditedItem);
                textureListPopupGUI.Start();
                Screen.Desktop.Children.Add(textureListPopupGUI.Panel);
            }
        }

        private void RotateTexture()
        {
            EditedItem.RotateSelectedTexture();
        }

        private void AddPart()
        {
            EditedItem.AddNewCuboid();
        }

        private void RemovePart()
        {
            EditedItem.RemoveSelectedCuboid();
        }

        private void CopyPart()
        {
            EditedItem.CopySelectedCuboid();
        }

        internal override void HideOptionsPanel(bool isKeyframeEdited = false)
        {
            if (_optionsPopupGUI != null)
            {
                Screen.Desktop.Children.Remove(_optionsPopupGUI.MainPanel);
                _optionsPopupGUI = null;
            }
            if (_animationEditorPopupGUI != null)
            {
                Screen.Desktop.Children.Remove(_animationEditorPopupGUI.Panel);
                _animationEditorPopupGUI = null;

                if (!isKeyframeEdited)
                {
                    _itemEditorState.FPItemModel.LoadOriginalCuboids();
                }
            }

            ShowMenu();
            EnableControls();
        }

        public void OpenItemOptions()
        {
            ShowItemOptions();
        }

        private void ToggleFirstPersonMode()
        {
            ShowFirstPersonModeMenu();

            _itemEditorState.ToggleFirstPersonMode();
        }

        private void ToggleThirdPersonMode()
        {
            _itemEditorState.ToggleFirstPersonMode();
            ShowMainMenu();
        }

        private void ShowFPAnimationPopup(bool isInitialized = true)
        {
            if (isInitialized)
            {
                _itemEditorState.FPItemModel.StoreOriginalCuboids();
            }

            ShowFirstPersonModeMenu();
            DisableControls();

            if (_animationEditorPopupGUI == null)
            {
                _animationEditorPopupGUI = new AnimationEditorPopupGUI(
                    Game, this, _itemEditorState);
                _animationEditorPopupGUI.Start();
            }

            Screen.Desktop.Children.Add(_animationEditorPopupGUI.Panel);
        }

        private void SaveDefaultItem()
        {
            _itemEditorState.SaveDefaultFirstPersonData();

            ShowAlertBox("Default data of first person item saved.");
        }

        public void OnAnimationEnd()
        {
            //            _playKeyframeButton.Enabled = true;
            _saveAndPlayAnimationButton.Enabled = true;
            _playAnimationButton.Enabled = true;

            _saveAndPlayAnimationButton.Text = "Play full animation";
            _playAnimationButton.Text = "Play animation";

            _playAnimationButton.PopupText = "";

            _saveAndPlayAnimationButton.PopupText = "";
        }

        public void OnAnimationComputing()
        {
            _playAnimationButton.PopupText =
                "Computing animation is slow, please wait. But it will be fast and smooth in-game :)";
            _saveAndPlayAnimationButton.PopupText =
                "Computing animation is slow, please wait. But it will be fast and smooth in-game :)";

            _saveAndPlayAnimationButton.Text = "Computing...";
            _playAnimationButton.Text = "Computing...";
        }

        private void PlayFullAnimation(bool saveKeyframe = false)
        {
            _saveAndPlayAnimationButton.Enabled = false;
            _playAnimationButton.Enabled = false;

            if (saveKeyframe)
            {
                _itemEditorState.SaveCurrentKeyframe();
            }

            _itemEditorState.PlayFullAnimation();
        }

        internal void Draw(GameTime gameTime)
        {
            if (_optionsPopupGUI != null)
            {
                _optionsPopupGUI.Draw(gameTime);
            }
        }

        internal void OnEscape()
        {
            if (_optionsPopupGUI != null)
            {
                _optionsPopupGUI.SetOptionsData();
                HideOptionsPanel();
            }
            else if (textureListPopupGUI != null)
            {
                HideTextureSelectionPanel();
            }

            ShowMenu();
            EnableControls();
        }

        public void ShowSideSelectedControls()
        {
            if (!EditedItem.IsBeingTransformedForAnimation)
            {
                foreach (Control sideSelectedControl in _sideSelectedControls)
                {
                    if (!Screen.Desktop.Children.Contains(sideSelectedControl))
                    {
                        Screen.Desktop.Children.Add(sideSelectedControl);
                    }
                }
            }
        }

        public void HideSideSelectedControls()
        {
            foreach (Control sideSelectedControl in _sideSelectedControls)
            {
                Screen.Desktop.Children.Remove(sideSelectedControl);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_optionsPopupGUI != null)
            {
                _optionsPopupGUI.Dispose();
            }
        }
    }
}