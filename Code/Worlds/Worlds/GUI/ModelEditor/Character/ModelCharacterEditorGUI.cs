using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.GUI.TexturePackEditor;
using WorldsGame.Saving;
using WorldsGame.Utils;

namespace WorldsGame.GUI.ModelEditor
{
    internal class ModelCharacterEditorGUI : ModelEditorGUIBase
    {
        private readonly WorldSettings _worldSettings;
        private readonly CharacterEditorState _characterEditorState;

        private AnimationEditorPopupGUI _animationEditorPopupGUI;
        private CharacterOptionsPopupGUI _optionsPopupGUI;

        private List<Control> _mainControls;
        private List<Control> _keyframeControls;
        private List<Control> _animationPlayControls;
        private List<Control> _sideSelectedControls;
        private List<Control> _itemPositionControls;
        private List<Control> _itemSelectedControls;

        private ButtonControl _playAnimationButton;

        private ButtonControl _saveAndPlayAnimationButton;
        private bool _isKeyframeEdited;

        internal bool IsMenuOpen { get { return textureListPopupGUI != null || _animationEditorPopupGUI != null || _optionsPopupGUI != null; } }

        internal EditedModel EditedCharacter { get { return _characterEditorState.EditedCharacterModel; } }

        internal bool IsKeyframeEdited
        {
            get { return _isKeyframeEdited; }
        }

        internal ModelCharacterEditorGUI(
            WorldsGame game, WorldSettings worldSettings, CharacterEditorState characterEditorState)
            : base(game)
        {
            _worldSettings = worldSettings;
            _characterEditorState = characterEditorState;
            _mainControls = new List<Control>();
            _keyframeControls = new List<Control>();
            _animationPlayControls = new List<Control>();
            _sideSelectedControls = new List<Control>();
            _itemPositionControls = new List<Control>();
            _itemSelectedControls = new List<Control>();
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddButtons();

            ShowMainMenu();

            HideSideSelectedControls();
        }

        private void AddButtons()
        {
            var buttonX = Screen.Desktop.Bounds.Left + 100;
            var buttonY = Screen.Desktop.Bounds.Top + 30;

            var blockOptionsButton = new ButtonControl
                                         {
                                             Bounds = new UniRectangle(buttonX, buttonY, 160, ButtonHeight),
                                             Text = "Character properties"
                                         };
            blockOptionsButton.Pressed += (sender, args) => ShowOptionsPopup();
            pressableControls.Add(blockOptionsButton);

            var selectTextureButton = new ButtonControl
                                          {
                                              Bounds = new UniRectangle(blockOptionsButton.Bounds.Right + 30, buttonY, 150, ButtonHeight),
                                              Text = "Select texture"
                                          };
            selectTextureButton.Pressed += (sender, args) => SelectTexture();
            pressableControls.Add(selectTextureButton);
            _sideSelectedControls.Add(selectTextureButton);

            var rotateTextureButton = new ButtonControl
                                          {
                                              Bounds = new UniRectangle(selectTextureButton.Bounds.Right + 10, buttonY, 140, ButtonHeight),
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

            var animationOptionsButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, blockOptionsButton.Bounds.Bottom + 10, 160, ButtonHeight),
                Text = "Animation options"
            };
            animationOptionsButton.Pressed += (sender, args) => ShowAnimationPopup();
            pressableControls.Add(animationOptionsButton);

            var setFaceButton = new ButtonControl
            {
                Bounds = new UniRectangle(selectTextureButton.Bounds.Left, selectTextureButton.Bounds.Bottom + 10, 150, ButtonHeight),
                Text = "Set face height here",
                PopupText = "Sets height of first person camera at center of the selected side"
            };
            setFaceButton.Pressed += (sender, args) => SetFace();
            pressableControls.Add(setFaceButton);
            _sideSelectedControls.Add(setFaceButton);

            _mainControls.Add(blockOptionsButton);
            _mainControls.Add(selectTextureButton);
            _mainControls.Add(rotateTextureButton);
            _mainControls.Add(animationOptionsButton);
            _mainControls.Add(addPartButton);
            _mainControls.Add(removePartButton);
            _mainControls.Add(copyPartButton);
            _mainControls.Add(setFaceButton);

            _saveAndPlayAnimationButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, buttonY, 200, ButtonHeight),
                Text = "Save and play animation"
            };
            _saveAndPlayAnimationButton.Pressed += (sender, args) => PlayFullAnimation(true);
            pressableControls.Add(_saveAndPlayAnimationButton);

            var saveKeyframeButton = new ButtonControl
            {
                Bounds = new UniRectangle(_saveAndPlayAnimationButton.Bounds.Right + 10, buttonY, 200, ButtonHeight),
                Text = "Save keyframe"
            };
            saveKeyframeButton.Pressed += (sender, args) => SaveKeyframe();
            pressableControls.Add(saveKeyframeButton);

            _keyframeControls.Add(_saveAndPlayAnimationButton);
            _keyframeControls.Add(saveKeyframeButton);

            _playAnimationButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, buttonY, 150, ButtonHeight),
                Text = "Play animation"
            };
            _playAnimationButton.Pressed += (sender, args) => PlayFullAnimation();
            pressableControls.Add(_playAnimationButton);

            var backToAnimationButton = new ButtonControl
            {
                Bounds = new UniRectangle(_playAnimationButton.Bounds.Right + 10, buttonY, 80, ButtonHeight),
                Text = "Back"
            };
            backToAnimationButton.Pressed += (sender, args) => ShowAnimationPopup();
            pressableControls.Add(backToAnimationButton);

            _animationPlayControls.Add(_playAnimationButton);
            _animationPlayControls.Add(backToAnimationButton);

            var saveItemPositionButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, buttonY, 200, ButtonHeight),
                Text = "Save item position"
            };
            saveItemPositionButton.Pressed += (sender, args) => ShowAnimationPopup();
            pressableControls.Add(saveItemPositionButton);

            _itemPositionControls.Add(saveItemPositionButton);

            var itemExampleButton = new ButtonControl
            {
                Bounds = new UniRectangle(blockOptionsButton.Bounds.Right + 30, buttonY, 140, ButtonHeight),
                Text = "Load item for example look"
            };
            itemExampleButton.Pressed += (sender, args) => LoadExampleItemClick();
            pressableControls.Add(itemExampleButton);

            _itemSelectedControls.Add(itemExampleButton);
        }

        private void DisableModelModification()
        {
            EditedCharacter.DeselectEverything();
            EditedCharacter.IsSelectable = false;
        }

        private void EnableModelModification()
        {
            EditedCharacter.IsSelectable = true;
        }

        private void HideAllMenus()
        {
            HideMainMenu();
            HideKeyframeMenu();
            HideAnimationPlayMenu();
            HideItemPositionMenu();
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

        internal override void ShowKeyframeMenu()
        {
            HideAllMenus();

            _isKeyframeEdited = true;

            EnableModelModification();
            EditedCharacter.IsBeingTransformedForAnimation = true;
            foreach (Control keyframeControl in _keyframeControls)
            {
                if (!Screen.Desktop.Children.Contains(keyframeControl))
                {
                    Screen.Desktop.Children.Add(keyframeControl);
                }
            }
        }

        private void HideKeyframeMenu()
        {
            _isKeyframeEdited = false;

            EditedCharacter.IsBeingTransformedForAnimation = false;

            foreach (Control animationControl in _keyframeControls)
            {
                Screen.Desktop.Children.Remove(animationControl);
            }
        }

        private void SaveKeyframe()
        {
            _characterEditorState.SaveCurrentKeyframe();
            ShowAnimationPopup(isInitialized: false);
        }

        private void BackFromKeyframeEditor()
        {
            ShowAnimationPopup(isInitialized: false);
        }

        internal override void ShowAnimationPlayMenu()
        {
            HideAllMenus();
            DisableModelModification();
            foreach (Control animationControl in _animationPlayControls)
            {
                if (!Screen.Desktop.Children.Contains(animationControl))
                {
                    Screen.Desktop.Children.Add(animationControl);
                }
            }
        }

        private void HideAnimationPlayMenu()
        {
            foreach (Control animationControl in _animationPlayControls)
            {
                Screen.Desktop.Children.Remove(animationControl);
            }
        }

        internal void ShowItemPositionMenu()
        {
            HideAllMenus();
            DisableModelModification();
            foreach (Control itemPositionControl in _itemPositionControls)
            {
                Screen.Desktop.Children.Add(itemPositionControl);
            }
        }

        private void HideItemPositionMenu()
        {
            foreach (Control itemPositionControl in _itemPositionControls)
            {
                Screen.Desktop.Children.Remove(itemPositionControl);
            }
        }

        /// <summary>
        /// Shows animation popup
        /// </summary>
        /// <param name="isInitialized">True if popup is opened not from already edited animation (like if keyframe is saved), but from character editor</param>
        private void ShowAnimationPopup(bool isInitialized = true)
        {
            if (_characterEditorState.IsNew)
            {
                ShowAlertBox("Animations could only be set up for already saved characters");
                return;
            }

            if (isInitialized)
            {
                _characterEditorState.EditedCharacterModel.StoreOriginalCuboids();
            }

            ShowMainMenu();
            DisableControls();

            if (_animationEditorPopupGUI == null)
            {
                _animationEditorPopupGUI = new AnimationEditorPopupGUI(
                    Game, this, _characterEditorState);
                _animationEditorPopupGUI.Start();
            }

            Screen.Desktop.Children.Add(_animationEditorPopupGUI.Panel);
        }

        private void ShowOptionsPopup()
        {
            DisableControls();
            _optionsPopupGUI = new CharacterOptionsPopupGUI(Game, _worldSettings, this, EditedCharacter, _characterEditorState);
            _optionsPopupGUI.Start();
            Screen.Desktop.Children.Add(_optionsPopupGUI.PropertiesPanel);
        }

        private void PlayFullAnimation(bool saveKeyframe = false)
        {
            _saveAndPlayAnimationButton.Enabled = false;
            _playAnimationButton.Enabled = false;

            if (saveKeyframe)
            {
                _characterEditorState.SaveCurrentKeyframe();
            }

            _characterEditorState.PlayFullAnimation();
        }

        private void SelectTexture()
        {
            if (!EditedCharacter.IsSideSelected)
            {
                ShowAlertBox("To set a texture to the side, you need to select a side first.");
            }
            else
            {
                DisableControls();
                textureListPopupGUI = new TextureListPopupGUI(Game, _worldSettings, this, EditedCharacter);
                textureListPopupGUI.Start();
                Screen.Desktop.Children.Add(textureListPopupGUI.Panel);
            }
        }

        private void LoadExampleItemClick()
        {
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.DrawAfterGUI(gameTime, spriteBatch);

            if (_optionsPopupGUI != null)
            {
                _optionsPopupGUI.DrawAfterGUI(gameTime, spriteBatch);
            }
        }

        private void RotateTexture()
        {
            EditedCharacter.RotateSelectedTexture();
        }

        private void AddPart()
        {
            EditedCharacter.AddNewCuboid();
        }

        private void RemovePart()
        {
            EditedCharacter.RemoveSelectedCuboid();
        }

        private void CopyPart()
        {
            EditedCharacter.CopySelectedCuboid();
        }

        private void SetFace()
        {
            if (!EditedCharacter.IsSideSelected)
            {
                ShowAlertBox("You need to select a side first.");
            }
            else
            {
                Vector3 normal = EditedCharacter.GetSelectedSideNormal();

                //                bool checkResult = MathUtils.CheckIfParallelToXZ(normal);
                //
                //                if (!checkResult)
                //                {
                //                    ShowAlertBox("Side must be perpendicular to the floor");
                //                }
                //                else
                //                {
                _characterEditorState.FaceNormal = normal;
                Vector3 center = EditedCharacter.GetSelectedSideCenter();
                _characterEditorState.FaceHeight = center.Y;
                //                }
            }
        }

        internal void OnEscape()
        {
            if (textureListPopupGUI != null)
            {
                HideTextureSelectionPanel();
            }
            if (IsKeyframeEdited)
            {
                BackFromKeyframeEditor();
            }
            else
            {
                HideOptionsPanel();
            }
        }

        internal override void HideOptionsPanel(bool isKeyframeEdited = false)
        {
            if (_animationEditorPopupGUI != null)
            {
                Screen.Desktop.Children.Remove(_animationEditorPopupGUI.Panel);
                _animationEditorPopupGUI = null;

                if (!isKeyframeEdited)
                {
                    _characterEditorState.EditedCharacterModel.LoadOriginalCuboids();
                }
            }
            if (_optionsPopupGUI != null)
            {
                Screen.Desktop.Children.Remove(_optionsPopupGUI.PropertiesPanel);
                _optionsPopupGUI = null;
            }

            ShowMainMenu();
            EnableControls();
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

        public void ShowSideSelectedControls()
        {
            if (!EditedCharacter.IsBeingTransformedForAnimation)
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

        public void ShowItemSelectedControls()
        {
            if (!EditedCharacter.IsBeingTransformedForAnimation)
            {
                foreach (Control sideSelectedControl in _itemSelectedControls)
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
            foreach (Control sideSelectedControl in _sideSelectedControls.Concat(_itemSelectedControls))
            {
                Screen.Desktop.Children.Remove(sideSelectedControl);
            }
        }
    }
}