using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using WorldsGame.Camera;
using WorldsGame.Editors;
using WorldsGame.Editors.Blocks;
using WorldsGame.GUI.ModelEditor;
using WorldsGame.Playing.PauseMenu;
using WorldsGame.Playing.Renderers;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Gamestates
{
    internal enum MouseClickActionType
    {
        Transform,
        Move,
        Rotate
    }

    internal class BlockEditorState : WorldsGameState
    {
        private const int MAX_BLOCK_ITEM_STACK_COUNT = 64;
        private BlockEditorRenderer _blockEditorRenderer;

        private ArcBallCamera _camera;

        private ModelEditorActionManager _actionManager;

        private EditedModel _editedBlockModel;
        private EditedModel _notFullBlockModel;
        private EditedModel _notLiquidBlockModel;

        private ModelBlockEditorGUI _modelBlockEditorGUI;

        private ModelEditorPauseMenu _pauseMenu;

        private readonly WorldSettings _worldSettings;
        private SpriteBatch _spriteBatch;

        internal string WorldSettingsName { get { return _worldSettings.Name; } }

        internal bool IsMenuOpenOrPaused { get { return _modelBlockEditorGUI.IsMenuOpen || Paused; } }

        internal bool IsMenuOpen { get { return _modelBlockEditorGUI.IsMenuOpen; } }

        private Block _editedBlock;

        internal Block EditedBlock
        {
            get { return _editedBlock ?? (_editedBlock = new Block()); }
            set { _editedBlock = value; }
        }

        internal bool IsNew { get { return _editedBlock == null; } }

        internal BlockEditorState(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            _worldSettings = worldSettings;
            Game.IsMouseVisible = true;
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

            _editedBlockModel = new EditedModel(GraphicsDevice, _camera, this);

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            _modelBlockEditorGUI = new ModelBlockEditorGUI(Game, _editedBlockModel, _worldSettings, this);
            _modelBlockEditorGUI.Start();

            _pauseMenu = new ModelEditorPauseMenu(Game, this);

            Game.OnAfterDraw += DrawAfterGUI;

            CreateRenderers();

            Subscribe();
        }

        private void Subscribe()
        {
            _actionManager.OnScale += _editedBlockModel.OnScale;

            _actionManager.OnLeftClickContiniousAction += _editedBlockModel.OnTransform;
            _actionManager.OnShiftLeftClickContiniousAction += _editedBlockModel.OnTranslateX;
            _actionManager.OnCtrlLeftClickContiniousAction += _editedBlockModel.OnTranslateY;
            _actionManager.OnAltLeftClickContiniousAction += _editedBlockModel.OnTranslateZ;
            _actionManager.OnRightClickContiniousAction += _editedBlockModel.OnRotateY;
            _actionManager.OnShiftRightClickContiniousAction += _editedBlockModel.OnRotateX;
            _actionManager.OnCtrlRightClickContiniousAction += _editedBlockModel.OnRotateY;
            _actionManager.OnAltRightClickContiniousAction += _editedBlockModel.OnRotateZ;
            _actionManager.OnRightMouseButtonReleased += _editedBlockModel.StopChanging;
            _actionManager.OnLeftMouseButtonReleased += _editedBlockModel.StopChanging;
            _actionManager.MouseLeftClick += _editedBlockModel.OnSelect;

            Messenger.On("EscapeKeyPressed", OnEscape);
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
                    _modelBlockEditorGUI.OnEscape();
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
            _blockEditorRenderer = new BlockEditorRenderer(GraphicsDevice, _camera, _editedBlockModel);
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

            _modelBlockEditorGUI = new ModelBlockEditorGUI(Game, _editedBlockModel, _worldSettings, this);
            _modelBlockEditorGUI.Start();
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.LightBlue);

            _blockEditorRenderer.Draw(gameTime);

            _pauseMenu.Draw();
        }

        public void DrawAfterGUI(GameTime gameTime)
        {
            if (_modelBlockEditorGUI != null)
            {
                _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                    DepthStencilState.Default, RasterizerState.CullNone);

                _modelBlockEditorGUI.DrawAfterGUI(gameTime, _spriteBatch);

                _spriteBatch.End();
            }
        }

        public override void Update(GameTime gameTime)
        {
            _modelBlockEditorGUI.Update(gameTime);
            _camera.Update(gameTime);
            _editedBlockModel.Update(gameTime);
        }

        public void SaveCurrentBlock(string name)
        {
            if (CheckIfOKToSave())
            {
                List<Cuboid> cuboids = _editedBlockModel.GetCuboidsForSaving();

                EditedBlock.Name = name;
                EditedBlock.WorldSettingsName = WorldSettingsName;
                EditedBlock.Cuboids = cuboids;
                EditedBlock.IsFullBlock = _editedBlockModel.IsFullBlock;
                EditedBlock.IsLiquid = _editedBlockModel.IsLiquid;
                EditedBlock.LengthInBlocks = _editedBlockModel.LengthInBlocks;
                EditedBlock.HeightInBlocks = _editedBlockModel.HeightInBlocks;
                EditedBlock.WidthInBlocks = _editedBlockModel.WidthInBlocks;

                EditedBlock.Save();

                RenderCuboidsToIcon();

                SaveRelatedItem(name);
            }
        }

        private void SaveRelatedItem(string name)
        {
            var items = _worldSettings.Items;

            Item relatedItem = (from item in items
                                where item.Name == string.Format("{0} Block", name) && item.IsBlock
                                select item).FirstOrDefault() ?? new Item();

            Tuple<Vector3, Vector3> minMaxVertices = _editedBlockModel.GetMinMaxVertices();

            relatedItem.IsBlock = true;

            relatedItem.Name = string.Format("{0} Block", name);
            relatedItem.WorldSettingsName = WorldSettingsName;
            relatedItem.Cuboids = EditedBlock.Cuboids;
            relatedItem.IconColors = RenderCuboidsToIcon();
            relatedItem.MaxStackCount = MAX_BLOCK_ITEM_STACK_COUNT;

            relatedItem.LengthInBlocks = EditedBlock.LengthInBlocks;
            relatedItem.HeightInBlocks = EditedBlock.HeightInBlocks;
            relatedItem.WidthInBlocks = EditedBlock.WidthInBlocks;
            relatedItem.MinVertice = minMaxVertices.Item1;
            relatedItem.MaxVertice = minMaxVertices.Item2;
            relatedItem.ItemQuality = ItemQuality.Consumable;

            relatedItem.PrepareAnimations(_editedBlockModel, isNew: true);

            relatedItem.Save();
        }

        private Color[] RenderCuboidsToIcon()
        {
            Texture2D texture = _blockEditorRenderer.RenderModelToTexture();

            var result = new Color[24 * 24];

            texture.GetData(result);

            texture.Dispose();

            return result;
        }

        private bool CheckIfOKToSave()
        {
            if (_editedBlockModel.IsFullBlock)
            {
                if (!_editedBlockModel.CheckIfFullyTextured())
                {
                    _pauseMenu.BlockEditorGUI.ShowAlertBox("All sides of a fully cubical block must be textured.");
                    return false;
                }
            }
            else
            {
                if (!_editedBlockModel.CheckIfMinimallyTextured())
                {
                    _pauseMenu.BlockEditorGUI.ShowAlertBox("At least one side of the block must be textured.");
                    return false;
                }
            }

            return true;
        }

        public void NewBlock()
        {
            _editedBlockModel.New();
        }

        public void LoadBlock(string blockName)
        {
            EditedBlock = Block.SaverHelper(WorldSettingsName).Load(blockName);

            _editedBlockModel.Load(EditedBlock);
        }

        public void ToggleFullBlock(bool isFull)
        {
            Unsubscribe();
            //            _editedBlock.Clear();
            if (isFull)
            {
                _notFullBlockModel = _editedBlockModel;
                _editedBlockModel = new EditedModel(GraphicsDevice, _camera, this, isFullBlock: isFull);
            }
            else
            {
                _editedBlockModel.Clear();
                _editedBlockModel = _notFullBlockModel ?? new EditedModel(GraphicsDevice, _camera, this, isFullBlock: isFull);
            }

            _blockEditorRenderer.EditedBlock = _editedBlockModel;

            _modelBlockEditorGUI.EditedBlock = _editedBlockModel;
            Subscribe();
        }

        public void ToggleLiquid(bool isLiquid)
        {
            Unsubscribe();
            //            _editedBlock.Clear();
            if (isLiquid)
            {
                _notLiquidBlockModel = _editedBlockModel;
                _editedBlockModel = new EditedModel(GraphicsDevice, _camera, this, isLiquid: true);
            }
            else
            {
                _editedBlockModel.Clear();
                _editedBlockModel = _notLiquidBlockModel ?? new EditedModel(GraphicsDevice, _camera, this, isFullBlock: true);
            }

            _blockEditorRenderer.EditedBlock = _editedBlockModel;

            _modelBlockEditorGUI.EditedBlock = _editedBlockModel;
            Subscribe();
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        private void DisposeRenderers()
        {
            _blockEditorRenderer.Dispose();
        }

        public override void Dispose()
        {
            Unsubscribe();

            DisposeRenderers();

            _camera.Dispose();
            _pauseMenu.Dispose();
            _spriteBatch.Dispose();
        }

        private void Unsubscribe()
        {
            _actionManager.OnScale -= _editedBlockModel.OnScale;

            _actionManager.OnLeftClickContiniousAction -= _editedBlockModel.OnTransform;
            _actionManager.OnShiftLeftClickContiniousAction -= _editedBlockModel.OnTranslateX;
            _actionManager.OnCtrlLeftClickContiniousAction -= _editedBlockModel.OnTranslateY;
            _actionManager.OnAltLeftClickContiniousAction -= _editedBlockModel.OnTranslateZ;
            _actionManager.OnRightClickContiniousAction -= _editedBlockModel.OnRotateY;
            _actionManager.OnShiftRightClickContiniousAction -= _editedBlockModel.OnRotateX;
            _actionManager.OnCtrlRightClickContiniousAction -= _editedBlockModel.OnRotateY;
            _actionManager.OnAltRightClickContiniousAction -= _editedBlockModel.OnRotateZ;
            _actionManager.OnRightMouseButtonReleased -= _editedBlockModel.StopChanging;
            _actionManager.OnLeftMouseButtonReleased -= _editedBlockModel.StopChanging;
            _actionManager.MouseLeftClick -= _editedBlockModel.OnSelect;

            Game.OnAfterDraw -= DrawAfterGUI;

            Messenger.Off("EscapeKeyPressed", OnEscape);
        }
    }
}