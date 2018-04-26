using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nuclex.UserInterface;
using WorldsGame.Camera;
using WorldsGame.Editors;
using WorldsGame.Editors.Blocks;
using WorldsGame.GUI.ModelEditor;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.PauseMenu;
using WorldsGame.Playing.Renderers;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.GeometricPrimitives;

namespace WorldsGame.Gamestates
{
    internal class ItemEditorState : WorldsGameState, IAnimationHolderEditorState
    {
        private ItemEditorRenderer _itemEditorRenderer;

        private ModelEditorActionManager _actionManager;

        private ModelItemEditorGUI _itemEditorGUI;

        private ModelEditorPauseMenu _pauseMenu;

        private readonly WorldSettings _worldSettings;

        private List<List<CuboidPrimitive>> _currentTransformations;

        private Item _defaultItem;

        public ArcBallCamera MainCamera { get; private set; }

        public ModelEditorFirstPersonCamera FirstPersonCamera { get; private set; }

        internal string WorldSettingsName { get { return _worldSettings.Name; } }

        internal bool IsMenuOpen { get { return _itemEditorGUI.IsMenuOpen; } }

        internal bool IsMenuOpenOrPaused { get { return _itemEditorGUI.IsMenuOpen || Paused; } }

        internal string ItemName { get { return !IsNewItem ? EditedItem.Name : ""; } }

        internal Item EditedItem { get; private set; }

        internal bool IsNewItem { get { return EditedItem == null; } }

        public CompiledAnimation EditedAnimation { get; set; }

        public int EditedKeyframeIndex { get; set; }

        internal EditedModel EditedItemModel { get; private set; }

        internal EditedModel FPItemModel { get; private set; }

        public EditedModel EditedModelForAnimation
        {
            get { return FPItemModel; }
        }

        internal bool IsFirstPersonModeOn { get; private set; }

        internal List<Effect> Action1Effects { get; private set; }

        internal List<Effect> Action2Effects { get; private set; }

        internal Dictionary<AnimationType, CompiledAnimation> DefaultFirstPersonAnimations { get; private set; }

        private int _maxStackCount;
        private Task _animationComputationTask;
        private bool _isFullAnimationPlaying = false;
        private bool _isKeyframeAnimationPlaying = false;

        private int _currentAnimationIndex;
        private ComputedAnimation _currentComputedAnimation;

        internal int MaxStackCount
        {
            get { return _maxStackCount; }
            set
            {
                _maxStackCount = value == 0 ? 1 : value;
            }
        }

        public int CuboidCount
        {
            get { return EditedModelForAnimation.CuboidCount; }
        }

        public ItemQuality ItemQuality { get; set; }

        internal ItemEditorState(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            _worldSettings = worldSettings;
            Game.IsMouseVisible = true;
            _currentTransformations = new List<List<CuboidPrimitive>>();

            Action1Effects = new List<Effect>();
            Action2Effects = new List<Effect>();
        }

        protected override void Initialize()
        {
            Graphics.PreferMultiSampling = false;
            Graphics.ApplyChanges();

            Game.InputController.IsMouseCentered = false;
            Game.InputController.IsMouseUncenteredDetection = true;

            SetClearScreen();

            _actionManager = new ModelEditorActionManager(Game.InputController);
            _actionManager.SubscribeToInputs();

            SetupCamera();

            EditedItemModel = new EditedModel(GraphicsDevice, MainCamera, this);

            LoadFPItem();

            _itemEditorGUI = new ModelItemEditorGUI(Game, _worldSettings, this);
            _itemEditorGUI.Start();

            _pauseMenu = new ModelEditorPauseMenu(Game, this);

            CreateRenderers();

            SubscribeItem();
            LoadDefaultItem();

            Game.OnAfterDraw += DrawAfterGUI;

            Messenger.On("EscapeKeyPressed", OnEscape);
        }

        private void OnCuboidSideSelected(CuboidPrimitive cuboid)
        {
            if (!cuboid.IsItem)
            {
                _itemEditorGUI.ShowSideSelectedControls();
            }
        }

        private void OnCuboidSideDeselected()
        {
            _itemEditorGUI.HideSideSelectedControls();
        }

        private void LoadDefaultItem()
        {
            _defaultItem = Item.SaverHelper(WorldSettingsName).Load(Item.DEFAULT_ITEM_NAME);

            if (_defaultItem == null)
            {
                _defaultItem = new Item
                {
                    Name = Item.DEFAULT_ITEM_NAME,
                    WorldSettingsName = WorldSettingsName,
                    Cuboids = FPItemModel.GetCuboidsForSaving(),
                    MaxStackCount = 0,
                    LengthInBlocks = 1,
                    HeightInBlocks = 1,
                    WidthInBlocks = 1,
                    IsSystem = true,
                    ItemQuality = ItemQuality.Unbreakable
                };

                _defaultItem.PrepareAnimations(null, isNew: true, forceAnimations: true);

                _defaultItem.Save();
            }
        }

        private void SubscribeItem()
        {
            UnsubscribeFPItem();

            _actionManager.OnScale += EditedItemModel.OnScale;

            _actionManager.OnLeftClickContiniousAction += EditedItemModel.OnTransform;
            _actionManager.OnShiftLeftClickContiniousAction += EditedItemModel.OnTranslateX;
            _actionManager.OnCtrlLeftClickContiniousAction += EditedItemModel.OnTranslateY;
            _actionManager.OnAltLeftClickContiniousAction += EditedItemModel.OnTranslateZ;
            _actionManager.OnRightClickContiniousAction += EditedItemModel.OnRotateY;
            _actionManager.OnShiftRightClickContiniousAction += EditedItemModel.OnRotateX;
            _actionManager.OnCtrlRightClickContiniousAction += EditedItemModel.OnRotateY;
            _actionManager.OnAltRightClickContiniousAction += EditedItemModel.OnRotateZ;
            _actionManager.OnRightMouseButtonReleased += EditedItemModel.StopChanging;
            _actionManager.OnLeftMouseButtonReleased += EditedItemModel.StopChanging;
            _actionManager.MouseLeftClick += EditedItemModel.OnSelect;

            EditedItemModel.OnSideSelected += OnCuboidSideSelected;
            EditedItemModel.OnSideDeselected += OnCuboidSideDeselected;
        }

        private void SubscribeFPItem()
        {
            UnsubscribeItem();

            _actionManager.OnScale += FPItemModel.OnScale;

            _actionManager.OnLeftClickContiniousAction += FPItemModel.OnTransform;
            _actionManager.OnShiftLeftClickContiniousAction += FPItemModel.OnTranslateX;
            _actionManager.OnCtrlLeftClickContiniousAction += FPItemModel.OnTranslateY;
            _actionManager.OnAltLeftClickContiniousAction += FPItemModel.OnTranslateZ;
            _actionManager.OnRightClickContiniousAction += FPItemModel.OnRotateY;
            _actionManager.OnShiftRightClickContiniousAction += FPItemModel.OnRotateX;
            _actionManager.OnCtrlRightClickContiniousAction += FPItemModel.OnRotateY;
            _actionManager.OnAltRightClickContiniousAction += FPItemModel.OnRotateZ;
            _actionManager.OnRightMouseButtonReleased += FPItemModel.StopChanging;
            _actionManager.OnLeftMouseButtonReleased += FPItemModel.StopChanging;
            _actionManager.MouseLeftClick += FPItemModel.OnSelect;
        }

        private void OnEscape()
        {
            if (Paused)
            {
                Resume();
            }
            else
            {
                if (IsMenuOpen)
                {
                    _itemEditorGUI.OnEscape();
                    return;
                }

                Pause();
            }
        }

        private void SetupCamera()
        {
            MainCamera = new ArcBallCamera(Graphics.GraphicsDevice.Viewport, _actionManager);
            MainCamera.Initialize();

            FirstPersonCamera = new ModelEditorFirstPersonCamera(Graphics.GraphicsDevice.Viewport, new Vector3(0, 1.8f, 0), 0, 0);
            FirstPersonCamera.Initialize();
        }

        private void CreateRenderers()
        {
            _itemEditorRenderer = new ItemEditorRenderer(GraphicsDevice, this);
        }

        private void SetClearScreen()
        {
            var clearScreen = new Screen(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            Game.GUIManager.Screen = clearScreen;
        }

        protected override void OnPause()
        {
            base.OnPause();
            _pauseMenu.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _pauseMenu.Stop();

            _itemEditorGUI.ShowMenu();
            _itemEditorGUI = new ModelItemEditorGUI(Game, _worldSettings, this);
            _itemEditorGUI.Start();
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.LightBlue);

            _itemEditorRenderer.Draw(gameTime);

            _pauseMenu.Draw();
        }

        public void DrawAfterGUI(GameTime gameTime)
        {
            if (_itemEditorGUI != null)
            {
                _itemEditorGUI.Draw(gameTime);
            }
        }

        public override void Update(GameTime gameTime)
        {
            _itemEditorGUI.Update(gameTime);
            MainCamera.Update(gameTime);
            // has no use at the moment
            //            EditedItemModel.Update(gameTime);
            CheckTasks();
            UpdateAnimation(gameTime);
        }

        private void CheckTasks()
        {
            if (_animationComputationTask != null)
            {
                if (_animationComputationTask.IsFaulted || _animationComputationTask.IsCanceled)
                {
                    _animationComputationTask = null;
                }
                else if (_animationComputationTask.IsCompleted)
                {
                    StartFullAnimation();
                    _animationComputationTask = null;
                }
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            if (_isFullAnimationPlaying)
            {
                FPItemModel.UpdateAnimation(gameTime);
            }
        }

        public void CopyKeyframe(string name)
        {
            CompiledKeyframe keyframe = (
                from kf in EditedAnimation.Keyframes[0]
                where kf.Name == name
                select kf).FirstOrDefault();

            if (keyframe != null)
            {
                var newKeyframe = new CompiledKeyframe
                                      {
                                          IsFirst = false,
                                          Time = name == "First" ? 0.1f : keyframe.Time + 0.1f
                                      };

                EditedAnimation.RefreshAnimationPerCuboids(FPItemModel);

                for (int i = 0; i < FPItemModel.CuboidCount; i++)
                {
                    EditedAnimation.Keyframes[i].Add(newKeyframe);
                }

                EditedAnimation.SortKeyframes();

                if (EditedItem != null)
                {
                    EditedItem.Save();
                }
            }
        }

        public void DeleteKeyframe(string name)
        {
            if (name == "First")
            {
                return;
            }

            int keyframeIndex = EditedAnimation.Keyframes[0].FindIndex(
                kf => kf.Name == name);
            EditedAnimation.RemoveKeyframe(keyframeIndex);

            EditedAnimation.SortKeyframes();

            if (EditedItem != null)
            {
                EditedItem.Save();
            }
        }

        public void SaveAnimation()
        {
            UpdateFirstPersonData();
            EditedItem.Save();
        }

        public void SaveCurrentKeyframe()
        {
            var cuboids = FPItemModel.GetCuboidsForSaving();

            // We need the time, so we just take the first possible keyframe, since they're synchronized
            var oldKeyframe = EditedAnimation.Keyframes[0][EditedKeyframeIndex];
            List<CompiledKeyframe> keyframeList = CompiledAnimation.CreateKeyframeList(oldKeyframe.Time, FPItemModel.OriginalNonanimatedCuboids, cuboids);

            for (int i = 0; i < cuboids.Count; i++)
            {
                // For some reason EditedAnimation.Keyframes[N] is referencing to the same object for each N here, so we just create a new object
                EditedAnimation.Keyframes[i][EditedKeyframeIndex] = new CompiledKeyframe
                {
                    IsFirst = false,
                    Rotation = keyframeList[i].Rotation,
                    Scale = keyframeList[i].Scale,
                    Translation = keyframeList[i].Translation,
                    Time = keyframeList[i].Time
                };
            }

            if (IsNewItem)
            {
                _defaultItem.Save();
            }
            else
            {
                EditedItem.Save();
            }
        }

        public void PlayFullAnimation()
        {
            _animationComputationTask =
                Task.Factory.StartNew(PrepareFullAnimationTransformations);
            _itemEditorGUI.OnAnimationComputing();
        }

        private void StartFullAnimation()
        {
            _isKeyframeAnimationPlaying = false;
            _currentAnimationIndex = 0;
            _isFullAnimationPlaying = true;
            FPItemModel.IsBeingAnimated = true;
            FPItemModel.OnAnimationStop += OnAnimationEnd;
        }

        private void OnAnimationEnd()
        {
            _currentAnimationIndex = 0;
            _isFullAnimationPlaying = false;

            _itemEditorGUI.OnAnimationEnd();
            FPItemModel.OnAnimationStop -= OnAnimationEnd;
        }

        private void PrepareFullAnimationTransformations()
        {
            _currentComputedAnimation = ComputedAnimation.CreateFromCompiledAnimation(EditedAnimation);
            FPItemModel.CurrentAnimation = _currentComputedAnimation;
        }

        public void SaveCurrentItem(string name)
        {
            if (CheckIfOKToSave())
            {
                List<Cuboid> cuboids = EditedItemModel.GetCuboidsForSaving();
                Tuple<Vector3, Vector3> minMaxVertices = EditedItemModel.GetMinMaxVertices();

                bool isNew = false;
                if (IsNewItem)
                {
                    isNew = true;
                    EditedItem = new Item();
                }

                EditedItem.Name = name;
                EditedItem.WorldSettingsName = WorldSettingsName;
                EditedItem.Cuboids = cuboids;
                EditedItem.IconColors = EditedItemModel.IconTextureColors;
                EditedItem.MaxStackCount = MaxStackCount;
                EditedItem.LengthInBlocks = EditedItemModel.LengthInBlocks;
                EditedItem.HeightInBlocks = EditedItemModel.HeightInBlocks;
                EditedItem.WidthInBlocks = EditedItemModel.WidthInBlocks;
                EditedItem.MinVertice = minMaxVertices.Item1;
                EditedItem.MaxVertice = minMaxVertices.Item2;
                EditedItem.Action1Effects = Action1Effects;
                EditedItem.Action2Effects = Action2Effects;
                EditedItem.ItemQuality = ItemQuality;

                EditedItem.PrepareAnimations(FPItemModel, isNew: isNew);

                UpdateFirstPersonData();

                EditedItem.Save();
            }
        }

        private bool CheckIfOKToSave()
        {
            if (!EditedItemModel.CheckIfMinimallyTextured())
            {
                _pauseMenu.ItemEditorGUI.ShowAlertBox("At least one side of the item must be textured.");
                return false;
            }
            if (EditedItemModel.IconTextureColors == null)
            {
                _pauseMenu.ItemEditorGUI.ShowAlertBox("Item should have an assigned icon.");
                return false;
            }

            return true;
        }

        public void LoadItem(string name)
        {
            EditedItem = Item.SaverHelper(WorldSettingsName).Load(name);
            MaxStackCount = EditedItem.MaxStackCount;
            EditedItemModel.Load(EditedItem);
            Action1Effects = EditedItem.Action1Effects;
            Action2Effects = EditedItem.Action2Effects;
            ItemQuality = EditedItem.ItemQuality;

            UnsubscribeFPItem();
            DisposeFPItem();

            if (IsFirstPersonModeOn)
            {
                LoadFPItem();
                SubscribeFPItem();
            }
        }

        public void ShowItem()
        {
            LoadFPItem();
        }

        public void LoadAnimation(AnimationType animationType)
        {
            if (EditedItem == null)
            {
                EditedAnimation = _defaultItem.FirstPersonAnimations[animationType];
            }
            else
            {
                if (EditedItem.FirstPersonAnimations == null)
                {
                    EditedItem.PrepareAnimations(FPItemModel, isNew: IsNewItem, forceAnimations: true);
                }

                if (EditedItem.FirstPersonAnimations.ContainsKey(animationType))
                {
                    EditedAnimation = EditedItem.FirstPersonAnimations[animationType];
                }
                else
                {
                    EditedAnimation = new CompiledAnimation(FPItemModel.CuboidCount);
                    EditedItem.FirstPersonAnimations.Add(animationType, EditedAnimation);
                }
            }
        }

        public void ClearAnimations()
        {
            EditedItem.FirstPersonAnimations = null;
        }

        public void LoadFPItem()
        {
            if (FPItemModel != null)
            {
                DisposeFPItem();
            }

            FPItemModel = new EditedModel(
                GraphicsDevice, FirstPersonCamera, this, isFirstPersonItem: true,
                position: new Vector3(0, 1.8f, 8), size: new Vector3(2, 2, 2));

            FPItemModel.LengthInBlocks = 4;
            FPItemModel.HeightInBlocks = 4;
            FPItemModel.WidthInBlocks = 4;

            FPItemModel.OnSideSelected += OnCuboidSideSelected;
            FPItemModel.OnSideDeselected += OnCuboidSideDeselected;

            if (EditedItem == null)
            {
                return;
            }

            if (EditedItem.FirstPersonCuboid == null)
            {
                FPItemModel.ItemCuboid.LoadChildCuboids(EditedItemModel.CuboidPrimitives);
            }
            else
            {
                FPItemModel.ItemCuboid = EditedItem.FirstPersonCuboid.GetPrimitive(GraphicsDevice, WorldSettingsName, FPItemModel);

                FPItemModel.ItemCuboid.LoadChildCuboids(EditedItemModel.CuboidPrimitives);
            }
        }

        public void OpenItemOptions()
        {
            _itemEditorGUI.OpenItemOptions();
        }

        public void ToggleFirstPersonMode()
        {
            IsFirstPersonModeOn = !IsFirstPersonModeOn;

            if (IsFirstPersonModeOn)
            {
                LoadFirstPersonItemModel();
                SubscribeFPItem();
            }
            else
            {
                SubscribeItem();
            }
        }

        private void LoadFirstPersonItemModel()
        {
            if (FPItemModel == null)
            {
                LoadFPItem();
            }
        }

        public void SaveDefaultFirstPersonData()
        {
            if (!IsNewItem)
            {
                List<Cuboid> cuboidsForSaving = FPItemModel.GetCuboidsForSaving();

                if (cuboidsForSaving.Count > 0)
                {
                    _defaultItem.FirstPersonCuboid = cuboidsForSaving[0];

                    _defaultItem.Save();
                }
            }
            else
            {
                List<Cuboid> cuboidsForSaving = FPItemModel.GetCuboidsForSaving();

                Tuple<Vector3, Vector3> minMaxVertices = FPItemModel.GetMinMaxVertices();

                _defaultItem.MinVertice = minMaxVertices.Item1;
                _defaultItem.MaxVertice = minMaxVertices.Item2;

                if (cuboidsForSaving.Count > 0)
                {
                    _defaultItem.FirstPersonCuboid = cuboidsForSaving[0];
                }

                _defaultItem.PrepareAnimations(FPItemModel, isNew: true, forceAnimations: true);

                _defaultItem.Save();
            }
        }

        public void UpdateFirstPersonData()
        {
            if (FPItemModel == null)
            {
                return;
            }

            if (!IsNewItem)
            {
                List<Cuboid> cuboidsForSaving = FPItemModel.GetCuboidsForSaving();

                if (cuboidsForSaving.Count > 0)
                {
                    EditedItem.FirstPersonCuboid = cuboidsForSaving[0];
                }
            }
            else
            {
                List<Cuboid> cuboidsForSaving = FPItemModel.GetCuboidsForSaving();

                Tuple<Vector3, Vector3> minMaxVertices = FPItemModel.GetMinMaxVertices();

                EditedItem.MinVertice = minMaxVertices.Item1;
                EditedItem.MaxVertice = minMaxVertices.Item2;

                if (cuboidsForSaving.Count > 0)
                {
                    EditedItem.FirstPersonCuboid = cuboidsForSaving[0];
                }

                EditedItem.PrepareAnimations(FPItemModel, isNew: true, forceAnimations: true);
            }
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        private void DisposeRenderers()
        {
            _itemEditorRenderer.Dispose();
        }

        private void DisposeFPItem()
        {
            if (FPItemModel != null)
            {
                UnsubscribeFPItem();

                FPItemModel.Dispose();
                FPItemModel = null;
            }
        }

        private void UnsubscribeItem()
        {
            if (EditedItemModel != null)
            {
                _actionManager.OnScale -= EditedItemModel.OnScale;

                _actionManager.OnLeftClickContiniousAction -= EditedItemModel.OnTransform;
                _actionManager.OnShiftLeftClickContiniousAction -= EditedItemModel.OnTranslateX;
                _actionManager.OnCtrlLeftClickContiniousAction -= EditedItemModel.OnTranslateY;
                _actionManager.OnAltLeftClickContiniousAction -= EditedItemModel.OnTranslateZ;
                _actionManager.OnRightClickContiniousAction -= EditedItemModel.OnRotateY;
                _actionManager.OnShiftRightClickContiniousAction -= EditedItemModel.OnRotateX;
                _actionManager.OnCtrlRightClickContiniousAction -= EditedItemModel.OnRotateY;
                _actionManager.OnAltRightClickContiniousAction -= EditedItemModel.OnRotateZ;
                _actionManager.OnRightMouseButtonReleased -= EditedItemModel.StopChanging;
                _actionManager.OnLeftMouseButtonReleased -= EditedItemModel.StopChanging;
                _actionManager.MouseLeftClick -= EditedItemModel.OnSelect;

                EditedItemModel.OnSideSelected -= OnCuboidSideSelected;
                EditedItemModel.OnSideDeselected -= OnCuboidSideDeselected;
            }
        }

        private void UnsubscribeFPItem()
        {
            if (FPItemModel != null)
            {
                _actionManager.OnScale -= FPItemModel.OnScale;

                _actionManager.OnLeftClickContiniousAction -= FPItemModel.OnTransform;
                _actionManager.OnShiftLeftClickContiniousAction -= FPItemModel.OnTranslateX;
                _actionManager.OnCtrlLeftClickContiniousAction -= FPItemModel.OnTranslateY;
                _actionManager.OnAltLeftClickContiniousAction -= FPItemModel.OnTranslateZ;
                _actionManager.OnRightClickContiniousAction -= FPItemModel.OnRotateY;
                _actionManager.OnShiftRightClickContiniousAction -= FPItemModel.OnRotateX;
                _actionManager.OnCtrlRightClickContiniousAction -= FPItemModel.OnRotateY;
                _actionManager.OnAltRightClickContiniousAction -= FPItemModel.OnRotateZ;
                _actionManager.OnRightMouseButtonReleased -= FPItemModel.StopChanging;
                _actionManager.OnLeftMouseButtonReleased -= FPItemModel.StopChanging;
                _actionManager.MouseLeftClick -= FPItemModel.OnSelect;

                FPItemModel.OnSideSelected -= OnCuboidSideSelected;
                FPItemModel.OnSideDeselected -= OnCuboidSideDeselected;
            }
        }

        public override void Dispose()
        {
            UnsubscribeItem();
            UnsubscribeFPItem();

            Game.OnAfterDraw -= DrawAfterGUI;

            Messenger.Off("EscapeKeyPressed", OnEscape);

            DisposeRenderers();

            _pauseMenu.Dispose();
            MainCamera.Dispose();
            FirstPersonCamera.Dispose();
        }
    }
}