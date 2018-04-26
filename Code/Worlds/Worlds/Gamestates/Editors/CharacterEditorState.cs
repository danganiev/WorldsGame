using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
    internal class CharacterEditorState : WorldsGameState, IAnimationHolderEditorState
    {
        private CharacterEditorRenderer _characterEditorRenderer;

        private ArcBallCamera _camera;

        private ModelEditorActionManager _actionManager;

        private ModelCharacterEditorGUI _modelCharacterEditorGUI;

        private ModelEditorPauseMenu _pauseMenu;

        private readonly WorldSettings _worldSettings;

        //        private List<List<CuboidPrimitive>> _currentTransformations;

        private ComputedAnimation _currentComputedAnimation;
        private bool _isAnimationComputed;

        private bool _isFullAnimationPlaying = false;
        private bool _isKeyframeAnimationPlaying = false;

        private int _currentAnimationIndex;
        private Task _animationComputationTask;
        private SpriteBatch _spriteBatch;

        internal string WorldSettingsName { get { return _worldSettings.Name; } }

        internal bool IsMenuOpenOrPaused { get { return _modelCharacterEditorGUI.IsMenuOpen || Paused; } }

        internal bool IsMenuOpen { get { return _modelCharacterEditorGUI.IsMenuOpen; } }

        public CompiledAnimation EditedAnimation { get; set; }

        public int EditedKeyframeIndex { get; set; }

        private Character _editedCharacter;

        internal Character EditedCharacter
        {
            get
            {
                if (_editedCharacter == null)
                {
                    EditedCharacter = new Character { OverriddenItemsData = new Dictionary<string, ItemCuboidData>() };
                }
                return _editedCharacter;
            }
            private set { _editedCharacter = value; }
        }

        public EditedModel EditedCharacterModel { get; private set; }

        public EditedModel EditedModelForAnimation
        {
            get { return EditedCharacterModel; }
        }

        internal List<SpawnedItemRule> InventorySpawnRules { get; private set; }

        internal Vector3 FaceNormal { get; set; }

        internal float FaceHeight { get; set; }

        internal bool IsItemAnimation { get; set; }

        internal bool IsNew { get { return _editedCharacter == null; } }

        public int CuboidCount
        {
            get { return EditedModelForAnimation.CuboidCount; }
        }

        internal CharacterEditorState(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            _worldSettings = worldSettings;
            Game.IsMouseVisible = true;
            InventorySpawnRules = new List<SpawnedItemRule>();
            FaceNormal = Vector3.Forward;
            FaceHeight = 1.7f;
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

            EditedCharacterModel = new EditedModel(GraphicsDevice, _camera, this);

            _modelCharacterEditorGUI = new ModelCharacterEditorGUI(Game, _worldSettings, this);
            _modelCharacterEditorGUI.Start();

            _pauseMenu = new ModelEditorPauseMenu(Game, this);

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            EditedCharacterModel.OnSideSelected += OnCuboidSideSelected;
            EditedCharacterModel.OnSideDeselected += OnCuboidSideDeselected;

            CreateRenderers();

            SubscribeModel();

            ShowItem();

            Game.OnAfterDraw += DrawAfterGUI;

            Messenger.On("EscapeKeyPressed", OnEscape);
        }

        private void OnCuboidSideSelected(CuboidPrimitive cuboid)
        {
            if (!cuboid.IsItem)
            {
                _modelCharacterEditorGUI.ShowSideSelectedControls();
            }
            else
            {
                _modelCharacterEditorGUI.ShowItemSelectedControls();
            }
        }

        private void OnCuboidSideDeselected()
        {
            _modelCharacterEditorGUI.HideSideSelectedControls();
        }

        private void SubscribeModel()
        {
            _actionManager.OnScale += EditedCharacterModel.OnScale;

            _actionManager.OnLeftClickContiniousAction += EditedCharacterModel.OnTransform;
            _actionManager.OnShiftLeftClickContiniousAction += EditedCharacterModel.OnTranslateX;
            _actionManager.OnCtrlLeftClickContiniousAction += EditedCharacterModel.OnTranslateY;
            _actionManager.OnAltLeftClickContiniousAction += EditedCharacterModel.OnTranslateZ;
            _actionManager.OnRightClickContiniousAction += EditedCharacterModel.OnRotateY;
            _actionManager.OnShiftRightClickContiniousAction += EditedCharacterModel.OnRotateX;
            _actionManager.OnCtrlRightClickContiniousAction += EditedCharacterModel.OnRotateY;
            _actionManager.OnAltRightClickContiniousAction += EditedCharacterModel.OnRotateZ;
            _actionManager.OnRightMouseButtonReleased += EditedCharacterModel.StopChanging;
            _actionManager.OnLeftMouseButtonReleased += EditedCharacterModel.StopChanging;
            _actionManager.MouseLeftClick += EditedCharacterModel.OnSelect;
        }

        private void OnEscape()
        {
            if (Paused)
            {
                Resume();
            }
            else
            {
                if (IsMenuOpen || _modelCharacterEditorGUI.IsKeyframeEdited)
                {
                    _modelCharacterEditorGUI.OnEscape();
                    return;
                }

                Pause();
            }
        }

        private void SetupCamera()
        {
            _camera = new ArcBallCamera(Graphics.GraphicsDevice.Viewport, _actionManager);
            _camera.Initialize();
        }

        private void CreateRenderers()
        {
            _characterEditorRenderer = new CharacterEditorRenderer(GraphicsDevice, _camera, this);
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

            _modelCharacterEditorGUI = new ModelCharacterEditorGUI(Game, /*EditedCharacterModel,*/ _worldSettings, this);
            _modelCharacterEditorGUI.Start();
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.LightBlue);

            _characterEditorRenderer.Draw(gameTime);

            _pauseMenu.Draw();
        }

        public void DrawAfterGUI(GameTime gameTime)
        {
            if (_modelCharacterEditorGUI != null)
            {
                _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                    DepthStencilState.Default, RasterizerState.CullNone);

                _modelCharacterEditorGUI.DrawAfterGUI(gameTime, _spriteBatch);

                _spriteBatch.End();
            }
        }

        public override void Update(GameTime gameTime)
        {
            _modelCharacterEditorGUI.Update(gameTime);
            _camera.Update(gameTime);
            EditedCharacterModel.Update(gameTime);
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
                EditedCharacterModel.UpdateAnimation(gameTime);
            }
        }

        private void OnAnimationEnd()
        {
            _currentAnimationIndex = 0;
            _isFullAnimationPlaying = false;

            _modelCharacterEditorGUI.OnAnimationEnd();
            EditedCharacterModel.OnAnimationStop -= OnAnimationEnd;
        }

        public void SaveCurrentCharacter(string name)
        {
            if (CheckIfOKToSave())
            {
                List<Cuboid> cuboids = EditedCharacterModel.GetCuboidsForSaving();
                Tuple<Vector3, Vector3> minMaxVertices = EditedCharacterModel.GetMinMaxVertices();

                if (IsNew)
                {
                    EditedCharacter = new Character { OverriddenItemsData = new Dictionary<string, ItemCuboidData>() };
                }

                EditedCharacter.Name = name;
                EditedCharacter.WorldSettingsName = WorldSettingsName;
                EditedCharacter.Cuboids = cuboids;
                EditedCharacter.InventorySpawnRules = InventorySpawnRules;
                EditedCharacter.MinVertice = minMaxVertices.Item1;
                EditedCharacter.MaxVertice = minMaxVertices.Item2;
                EditedCharacter.LengthInBlocks = EditedCharacterModel.LengthInBlocks;
                EditedCharacter.HeightInBlocks = EditedCharacterModel.HeightInBlocks;
                EditedCharacter.WidthInBlocks = EditedCharacterModel.WidthInBlocks;
                //                EditedCharacter.FaceNormal = FaceNormal;
                EditedCharacter.FaceHeight = FaceHeight;
                //                EditedCharacter.OverriddenItemsData = new Dictionary<string, ItemCuboidData>();

                EditedCharacter.PrepareAnimations(EditedCharacterModel, isNew: IsNew);
                EditedCharacter.Save();
            }
        }

        private bool CheckIfOKToSave()
        {
            if (!EditedCharacterModel.CheckIfMinimallyTextured())
            {
                _pauseMenu.CharacterEditorGUI.ShowAlertBox("At least one side of the character must be textured.");
                return false;
            }

            return true;
        }

        public void NewCharacter()
        {
            ShowItem();

            EditedCharacterModel.New();
        }

        public void LoadCharacter(string name)
        {
            Character character = Character.SaverHelper(WorldSettingsName).Load(name);
            EditedCharacter = character;
            EditedCharacterModel.Load(character);
            InventorySpawnRules = character.InventorySpawnRules;
            //            FaceNormal = character.FaceNormal;
            FaceHeight = character.FaceHeight;
        }

        public void ShowItem()
        {
            EditedCharacterModel.ItemCuboid = new CuboidPrimitive(
                GraphicsDevice, new Vector3(2, 2, 2), new Vector3(8, 8, 8),
                editedModel: EditedCharacterModel, isItem: true);

            EditedCharacterModel.ItemCuboid.IsSticky = true;
        }

        public void LoadAnimation(AnimationType animationType)
        {
            if (IsNew || EditedCharacter.Animations == null)
            {
                _modelCharacterEditorGUI.ShowAlertBox("Animations could only be set up for already saved characters");
                return;
            }

            EditedAnimation = EditedCharacter.Animations[animationType];
        }

        public void ClearAnimations()
        {
            if (!IsNew)
            {
                EditedCharacter.Animations = null;
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

                RefreshAnimationPerCuboids();

                for (int i = 0; i < EditedCharacterModel.CuboidCount; i++)
                {
                    EditedAnimation.Keyframes[i].Add(newKeyframe);
                }

                EditedAnimation.SortKeyframes();

                EditedCharacter.Save();
            }
        }

        public void RefreshAnimationPerCuboids()
        {
            EditedAnimation.RefreshAnimationPerCuboids(EditedCharacterModel);
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

            EditedCharacter.Save();
        }

        public void SaveAnimation()
        {
            if (!IsNew)
            {
                SaveCurrentCharacter(EditedCharacter.Name);
            }
        }

        public void SaveCurrentKeyframe()
        {
            var cuboids = EditedCharacterModel.GetCuboidsForSaving();

            // We need the time, so we just take the first possible keyframe, since they're synchronized
            var oldKeyframe = EditedAnimation.Keyframes[0][EditedKeyframeIndex];
            List<CompiledKeyframe> keyframeList = CompiledAnimation.CreateKeyframeList(oldKeyframe.Time, EditedCharacterModel.OriginalNonanimatedCuboids, cuboids);

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

            EditedCharacter.Save();
        }

        public void PlayFullAnimation()
        {
            EditedCharacter.PrepareAnimations(EditedCharacterModel, isNew: IsNew);

            _animationComputationTask =
                Task.Factory.StartNew(PrepareFullAnimationTransformations);
            _modelCharacterEditorGUI.OnAnimationComputing();
        }

        private void StartFullAnimation()
        {
            _isKeyframeAnimationPlaying = false;
            _currentAnimationIndex = 0;
            _isFullAnimationPlaying = true;
            EditedCharacterModel.IsBeingAnimated = true;
            EditedCharacterModel.OnAnimationStop += OnAnimationEnd;
        }

        private void PrepareFullAnimationTransformations()
        {
            _currentComputedAnimation = ComputedAnimation.CreateFromCompiledAnimation(EditedAnimation);
            EditedCharacterModel.CurrentAnimation = _currentComputedAnimation;
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        internal ContentManager GetContentManager()
        {
            return Content;
        }

        private void DisposeRenderers()
        {
            _characterEditorRenderer.Dispose();
        }

        private void UnsubscribeModel()
        {
            EditedCharacterModel.OnSideSelected -= OnCuboidSideSelected;
            EditedCharacterModel.OnSideDeselected -= OnCuboidSideDeselected;

            _actionManager.OnScale -= EditedCharacterModel.OnScale;

            _actionManager.OnLeftClickContiniousAction -= EditedCharacterModel.OnTransform;
            _actionManager.OnShiftLeftClickContiniousAction -= EditedCharacterModel.OnTranslateX;
            _actionManager.OnCtrlLeftClickContiniousAction -= EditedCharacterModel.OnTranslateY;
            _actionManager.OnAltLeftClickContiniousAction -= EditedCharacterModel.OnTranslateZ;
            _actionManager.OnRightClickContiniousAction -= EditedCharacterModel.OnRotateY;
            _actionManager.OnShiftRightClickContiniousAction -= EditedCharacterModel.OnRotateX;
            _actionManager.OnCtrlRightClickContiniousAction -= EditedCharacterModel.OnRotateY;
            _actionManager.OnAltRightClickContiniousAction -= EditedCharacterModel.OnRotateZ;
            _actionManager.OnRightMouseButtonReleased -= EditedCharacterModel.StopChanging;
            _actionManager.OnLeftMouseButtonReleased -= EditedCharacterModel.StopChanging;
            _actionManager.MouseLeftClick -= EditedCharacterModel.OnSelect;
        }

        public override void Dispose()
        {
            UnsubscribeModel();

            DisposeRenderers();

            Messenger.Off("EscapeKeyPressed", OnEscape);

            Game.OnAfterDraw -= DrawAfterGUI;

            _spriteBatch.Dispose();
            _pauseMenu.Dispose();
            _camera.Dispose();
        }
    }
}