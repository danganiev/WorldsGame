using System;
using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.GeometricPrimitives;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI.ModelEditor
{
    internal interface IAnimationHolderEditorState
    {
        CompiledAnimation EditedAnimation { get; }

        EditedModel EditedModelForAnimation { get; }

        int EditedKeyframeIndex { get; set; }

        int CuboidCount { get; }

        void ShowItem();

        void LoadAnimation(AnimationType animationType);

        void CopyKeyframe(string selectedName);

        void DeleteKeyframe(string selectedName);

        void SaveAnimation();

        void ClearAnimations();
    }

    internal class AnimationEditorPopupGUI : View.GUI.GUI
    {
        private readonly IAnimationHolderEditorState _editorState;

        private readonly ModelEditorGUIBase _parentGUI;

        internal PanelControl Panel { get; private set; }

        private LabelControl _animationsLabel;
        private LabelControl _keyframesLabel;

        private ListControl _keyframesList;

        private NumberInputControl _keyframeSecondsInput;

        private List<Control> _animationEditorControls;
        private List<Control> _keyframeEditorControls;

        private ButtonControl _keyframeEditButton;

        private ListControl _animationTypeList;

        private LabelControl _overriddenItemsLabel;
        private ListControl _overriddenItemsList;

        private ButtonControl _playAnimationButton;

        private LabelControl _titleLabel;
        private ButtonControl _copyKeyframeButton;
        private ButtonControl _deleteKeyframeButton;
        private ButtonControl _clearToDefaultButton;

        private int SelectedKeyframeIndex
        {
            get { return _keyframesList != null ? (_keyframesList.SelectedItems.Count > 0 ? _keyframesList.SelectedItems[0] : -1) : -1; }
        }

        private bool IsKeyframeEdited { get { return Panel.Children.Contains(_keyframeSecondsInput); } }

        internal AnimationEditorPopupGUI(
            WorldsGame game, ModelEditorGUIBase parentGUI, CharacterEditorState characterEditorState)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;
            _parentGUI = parentGUI;
            _editorState = characterEditorState;
            _animationEditorControls = new List<Control>();
            _keyframeEditorControls = new List<Control>();
        }

        internal AnimationEditorPopupGUI(
            WorldsGame game, ModelEditorGUIBase parentGUI, ItemEditorState itemEditorState)
        {
            Game = game;
            viewport = Game.GraphicsDevice.Viewport;
            _parentGUI = parentGUI;
            _editorState = itemEditorState;
            _animationEditorControls = new List<Control>();
            _keyframeEditorControls = new List<Control>();
        }

        internal void GenerateControls()
        {
            AddMainPanel();
        }

        protected override void LoadData()
        {
            LoadList(_animationTypeList, from at in AnimationTypeHelper.ANIMATION_TYPE_NAMES select at.Value);
        }

        private void LoadKeyframes()
        {
            if (_editorState.EditedAnimation != null)
            {
                _keyframesList.Clear();
                List<List<CompiledKeyframe>> keyframes = _editorState.EditedAnimation.Keyframes;
                foreach (var keyframe in keyframes[0])
                {
                    _keyframesList.Items.Add(keyframe.Name);
                }
            }
        }

        private void AddMainPanel()
        {
            const int Y = 15;
            const int X = 30;
            const int elementListWidth = 200;
            const int elementListHeight = 300;
            const int width = 880;
            const int height = 520;

            Panel = new PanelControl
            {
                Bounds = new UniRectangle(_parentGUI.Screen.Width / 2 - width / 2, _parentGUI.Screen.Height / 2 - height / 2, width, height)
            };

            _titleLabel = new LabelControl
            {
                Text = "Animation options",
                Bounds = new UniRectangle(X, Y + 20, 110, _parentGUI.LabelHeight),
                IsHeader = true
            };

            _animationsLabel = new LabelControl
            {
                Text = "Animations:",
                Bounds = new UniRectangle(X, _titleLabel.Bounds.Bottom + 30, 110, _parentGUI.LabelHeight)
            };

            _animationTypeList = new ListControl
            {
                Bounds = new UniRectangle(30, _animationsLabel.Bounds.Bottom + 10, elementListWidth, elementListHeight - 205 + 15),
                SelectionMode = ListSelectionMode.Single
            };
            _animationTypeList.SelectionChanged += OnAnimationTypeSelected;

            _overriddenItemsLabel = new LabelControl
            {
                Text = "Items:",
                Bounds = new UniRectangle(X, _animationTypeList.Bounds.Bottom + 30, 110, _parentGUI.LabelHeight)
            };

            _overriddenItemsList = new ListControl
            {
                Bounds = new UniRectangle(30, _overriddenItemsLabel.Bounds.Bottom + 10, elementListWidth, elementListHeight - 205 + 15),
                SelectionMode = ListSelectionMode.Single
            };
            _overriddenItemsList.SelectionChanged += OnOverriddenItemSelected;

            _playAnimationButton = new ButtonControl
            {
                Text = "Play",
                Bounds = new UniRectangle(_animationTypeList.Bounds.Right + 10, _animationTypeList.Bounds.Top, 70, 30),
                Enabled = false
            };
            _playAnimationButton.Pressed += OnAnimationPlayed;

            var secondRowX = _playAnimationButton.Bounds.Right + 30;

            _keyframesLabel = new LabelControl
            {
                Text = "Keyframes:",
                Bounds = new UniRectangle(secondRowX, _titleLabel.Bounds.Bottom + 30, 110, _parentGUI.LabelHeight)
            };

            _keyframesList = new ListControl
            {
                Bounds = new UniRectangle(secondRowX, _keyframesLabel.Bounds.Bottom + 10, elementListWidth, elementListHeight - 205 + 15),
                SelectionMode = ListSelectionMode.Single
            };
            _keyframesList.SelectionChanged += OnKeyframeSelected;

            _copyKeyframeButton = new ButtonControl
            {
                Text = "Copy",
                Bounds = new UniRectangle(_keyframesList.Bounds.Right + 10, _keyframesList.Bounds.Top, 70, 30),
            };
            _copyKeyframeButton.Pressed += CopyKeyframe;

            _deleteKeyframeButton = new ButtonControl
            {
                Text = "Delete",
                Bounds = new UniRectangle(_keyframesList.Bounds.Right + 10, _copyKeyframeButton.Bounds.Bottom + 10, 70, 30),
                Enabled = false
            };
            _deleteKeyframeButton.Pressed += DeleteKeyframe;

            var thirdRowX = _copyKeyframeButton.Bounds.Right + 30;

            var editedKeyframeLabel = new LabelControl
            {
                Text = "Edited keyframe",
                Bounds = new UniRectangle(thirdRowX, _titleLabel.Bounds.Bottom + 30, 40, _parentGUI.LabelHeight)
            };

            var secondsNameLabel = new LabelControl
            {
                Text = "Position in seconds",
                Bounds = new UniRectangle(thirdRowX, editedKeyframeLabel.Bounds.Bottom + 30, 130, _parentGUI.LabelHeight)
            };

            _keyframeSecondsInput = new NumberInputControl
            {
                IsPositiveOnly = true,
                Bounds = new UniRectangle(secondsNameLabel.Bounds.Right + 10, secondsNameLabel.Bounds.Top, elementListWidth - 130 - 10, _parentGUI.LabelHeight),
                MaxValue = 100
            };

            _keyframeEditButton = new ButtonControl
            {
                Text = "Adjust model for keyframe",
                Bounds = new UniRectangle(thirdRowX, _keyframeSecondsInput.Bounds.Bottom + 30, elementListWidth, 30)
            };
            _keyframeEditButton.Pressed += (sender, args) => AdjustModel();

            var saveWithoutAdjustButton = new ButtonControl
            {
                Text = "Save without adjusting",
                Bounds = new UniRectangle(thirdRowX, _keyframeEditButton.Bounds.Bottom + 10, elementListWidth, 30)
            };
            saveWithoutAdjustButton.Pressed += (sender, args) => Save();

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 70)), new UniScalar(1f, -(X + 30)), 70, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            _clearToDefaultButton = new ButtonControl
            {
                Text = "Clear to default",
                Bounds = new UniRectangle(backButton.Bounds.Left - 140, new UniScalar(1f, -(X + 30)), 130, 30)
            };
            _clearToDefaultButton.Pressed += OnClearToDefault;

            Panel.Children.Add(_titleLabel);
            Panel.Children.Add(_animationsLabel);
            Panel.Children.Add(_animationTypeList);

            Panel.Children.Add(_playAnimationButton);

            _animationEditorControls.Add(_keyframesLabel);
            _animationEditorControls.Add(_keyframesList);
            _animationEditorControls.Add(_copyKeyframeButton);
            _animationEditorControls.Add(_deleteKeyframeButton);

            _keyframeEditorControls.Add(editedKeyframeLabel);
            _keyframeEditorControls.Add(secondsNameLabel);
            _keyframeEditorControls.Add(_keyframeSecondsInput);
            _keyframeEditorControls.Add(_keyframeEditButton);
            _keyframeEditorControls.Add(saveWithoutAdjustButton);

            Panel.Children.Add(backButton);
            Panel.Children.Add(_clearToDefaultButton);
        }

        //        private void OnSetItemPosition(object sender, EventArgs e)
        //        {
        //            _parentGUI.HideOptionsPanel();
        //            _parentGUI.ShowItemPositionMenu();
        //
        //            if (_overriddenItemsList.SelectedName() == "All items (default)")
        //            {
        //                _editorState.ShowItem();
        //            }
        //        }

        private void OnKeyframeSelected(object sender, EventArgs eventArgs)
        {
            if (_keyframesList.SelectedItems.Count == 0 || _keyframesList.SelectedName() == "First")
            {
                HideKeyframeEditor();
                _deleteKeyframeButton.Enabled = false;
                return;
            }

            _deleteKeyframeButton.Enabled = true;

            _editorState.EditedKeyframeIndex =
                _editorState.EditedAnimation.Keyframes[0].FindIndex(
                    keyframe => keyframe.Name == _keyframesList.SelectedName());

            ShowKeyframeEditor();

            _keyframeSecondsInput.Text = _editorState.EditedAnimation.Keyframes[0][
                _editorState.EditedKeyframeIndex].Time.ToString();
        }

        private void OnAnimationPlayed(object sender, EventArgs eventArgs)
        {
            if (_animationTypeList.SelectedItems.Count > 0)
            {
                _parentGUI.HideOptionsPanel();
                _parentGUI.ShowAnimationPlayMenu();
            }
        }

        private void OnClearToDefault(object sender, EventArgs eventArgs)
        {
            _editorState.ClearAnimations();
            Back();
        }

        private void OnAnimationTypeSelected(object sender, EventArgs eventArgs)
        {
            if (_animationTypeList.SelectedName() == AnimationTypeHelper.ANIMATION_TYPE_NAMES[AnimationType.Consume] ||
                _animationTypeList.SelectedName() == AnimationTypeHelper.ANIMATION_TYPE_NAMES[AnimationType.Swing])
            {
                ShowOverriddenItems();
                HideAnimationEditor();
                _playAnimationButton.Enabled = false;
            }
            else
            {
                HideOverriddenItems();

                if (_animationTypeList.SelectedItems.Count == 0)
                {
                    _playAnimationButton.Enabled = false;
                    return;
                }

                _playAnimationButton.Enabled = true;

                _editorState.LoadAnimation((AnimationType)_animationTypeList.SelectedItems[0]);

                if (_editorState.EditedAnimation == null)
                {
                    return;
                }

                CreateFirstKeyframe();

                ShowAnimationEditor();
            }
        }

        private void CopyKeyframe(object sender, EventArgs e)
        {
            _editorState.CopyKeyframe(_keyframesList.SelectedName());
            LoadKeyframes();
        }

        private void DeleteKeyframe(object sender, EventArgs e)
        {
            _editorState.DeleteKeyframe(_keyframesList.SelectedName());
            HideKeyframeEditor();
            LoadKeyframes();
        }

        private void ShowAnimationEditor()
        {
            foreach (Control animationEditorControl in _animationEditorControls)
            {
                if (!Panel.Children.Contains(animationEditorControl))
                {
                    Panel.Children.Add(animationEditorControl);
                }
            }
            LoadKeyframes();
        }

        private void HideAnimationEditor()
        {
            HideKeyframeEditor();
            foreach (Control animationEditorControl in _animationEditorControls)
            {
                Panel.Children.Remove(animationEditorControl);
            }
        }

        private void ShowOverriddenItems()
        {
            if (Panel.Children.Contains(_overriddenItemsList))
            {
                return;
            }

            Panel.Children.Add(_overriddenItemsList);
            Panel.Children.Add(_overriddenItemsLabel);

            LoadOverriddenItems();
        }

        private void HideOverriddenItems()
        {
            Panel.Children.Remove(_overriddenItemsList);
            Panel.Children.Remove(_overriddenItemsLabel);
        }

        private void LoadOverriddenItems()
        {
            _overriddenItemsList.Clear();

            _overriddenItemsList.Items.Add("All items (default)");
        }

        private void OnOverriddenItemSelected(object sender, EventArgs eventArgs)
        {
            if (_animationTypeList.SelectedItems.Count == 0 || _overriddenItemsList.SelectedItems.Count == 0)
            {
                _playAnimationButton.Enabled = false;
                return;
            }

            _playAnimationButton.Enabled = true;

            //            if (_overriddenItemsList.SelectedName() == "All items (default)")
            //            {
            //                _setItemPositionButton.Text = "Set default items position";
            //            }
            //            else
            //            {
            //                _setItemPositionButton.Text = "Override item position";
            //            }
            //
            //            if (!Panel.Children.Contains(_setItemPositionButton))
            //            {
            //                Panel.Children.Add(_setItemPositionButton);
            //            }

            //            string itemName = _overriddenItemsList.SelectedName() == "All items (default)"
            //                                  ? null
            //                                  : _overriddenItemsList.SelectedName();

            _editorState.LoadAnimation((AnimationType)_animationTypeList.SelectedItems[0]);

            if (_editorState.EditedAnimation == null)
            {
                return;
            }

            CreateFirstKeyframe();

            ShowAnimationEditor();
        }

        private void CreateFirstKeyframe()
        {
            if (_editorState.EditedAnimation.Keyframes.Count == 0)
            {
                _editorState.EditedAnimation.Keyframes.Add(new List<CompiledKeyframe>());
                //                for (int i = 0; i < _characterEditorState.EditedCharacterModel.CuboidCount; i++)
                for (int i = 0; i < _editorState.CuboidCount; i++)
                {
                    _editorState.EditedAnimation.Keyframes[0].Add(new CompiledKeyframe());
                }
            }
        }

        private void ShowKeyframeEditor()
        {
            foreach (Control keyframeControl in _keyframeEditorControls)
            {
                if (!Panel.Children.Contains(keyframeControl))
                {
                    Panel.Children.Add(keyframeControl);
                }
            }
        }

        private void HideKeyframeEditor()
        {
            foreach (Control keyframeControl in _keyframeEditorControls)
            {
                Panel.Children.Remove(keyframeControl);
            }
        }

        protected override void Save()
        {
            if (IsKeyframeEdited)
            {
                SaveKeyframe();
                LoadKeyframes();
            }
        }

        private bool SaveKeyframe(bool hideKeyframe = true)
        {
            if (_keyframeSecondsInput.Text == "")
            {
                _parentGUI.ShowAlertBox(
                    "Keyframe must have a position on the animation timeline.",
                    "Fill the 'Position in seconds' field.");
                return false;
            }

            for (int i = 0; i < _editorState.CuboidCount; i++)
            {
                _editorState.EditedAnimation.Keyframes[i][_editorState.EditedKeyframeIndex].Time =
                    _keyframeSecondsInput.GetFloat();
            }

            _editorState.EditedAnimation.SortKeyframes();

            _editorState.SaveAnimation();

            if (hideKeyframe)
            {
                HideKeyframeEditor();
            }

            return true;
        }

        private void AdjustModel()
        {
            LoadKeyframeCuboidsPositions(SelectedKeyframeIndex);

            if (SaveKeyframe(hideKeyframe: false))
            {
                _parentGUI.HideOptionsPanel(isKeyframeEdited: true);
                _parentGUI.ShowKeyframeMenu();
            }
        }

        private void LoadKeyframeCuboidsPositions(int selectedKeyframeIndex)
        {
            var cuboids = new List<CuboidPrimitive>();
            for (int i = 0; i < _editorState.EditedAnimation.Keyframes.Count; i++)
            {
                CuboidPrimitive cuboidPrimitive = _editorState.EditedModelForAnimation.CuboidPrimitives[i].Clone();
                CompiledKeyframe compiledKeyframe = _editorState.EditedAnimation.Keyframes[i][selectedKeyframeIndex];

                cuboidPrimitive.Scale(compiledKeyframe.Scale);
                cuboidPrimitive.Rotate(compiledKeyframe.Rotation);
                cuboidPrimitive.Translate(compiledKeyframe.Translation);

                cuboids.Add(cuboidPrimitive);
            }
            _editorState.EditedModelForAnimation.LoadKeyframeCuboidsPositions(cuboids);
        }

        internal override void Start()
        {
            GenerateControls();
            LoadData();
        }

        protected override void Back()
        {
            _parentGUI.HideOptionsPanel();
        }

        public override void Dispose()
        {
            base.Dispose();

            _clearToDefaultButton.Pressed -= OnClearToDefault;
        }
    }
}