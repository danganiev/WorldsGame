using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Camera;
using WorldsGame.Editors.Blocks;
using WorldsGame.Gamestates;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Renderers;

namespace WorldsGame.Playing.Renderers
{
    internal class ItemEditorRenderer : IRenderer
    {
        private AlphaTestEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ItemEditorState _itemEditorState;

        private readonly CoordinateAxes _coordinateAxes;
        private readonly EditorCellGrid _editorCellGrid;
        private readonly EditorCellGrid _fpEditorCellGrid;

        internal ItemEditorRenderer(GraphicsDevice graphicsDevice, ItemEditorState itemEditorState)
        {
            _graphicsDevice = graphicsDevice;
            _itemEditorState = itemEditorState;

            _coordinateAxes = new CoordinateAxes(_graphicsDevice, _itemEditorState.EditedItemModel);
            _editorCellGrid = new EditorCellGrid(_graphicsDevice, _itemEditorState.EditedItemModel);
            _fpEditorCellGrid = new EditorCellGrid(_graphicsDevice, 64, 0, 64);

            Initialize();
        }

        public void Initialize()
        {
            _effect = new AlphaTestEffect(_graphicsDevice)
            {
                VertexColorEnabled = false,
                ReferenceAlpha = 255,
                AlphaFunction = CompareFunction.GreaterEqual
            };
            _coordinateAxes.SetupCoordinateLines();
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (_itemEditorState.IsFirstPersonModeOn)
            {
                //                _coordinateAxes.DrawCoordinateSystem(_itemEditorState.FirstPersonCamera);
                _fpEditorCellGrid.Draw(_itemEditorState.FirstPersonCamera);

                if (_itemEditorState.FPItemModel != null)
                {
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    _effect.AlphaFunction = CompareFunction.Equal;
                    _itemEditorState.FPItemModel.Draw(
                        _itemEditorState.FirstPersonCamera.View, _itemEditorState.FirstPersonCamera.Projection, _effect);

                    _graphicsDevice.BlendState = BlendState.AlphaBlend;
                    _effect.AlphaFunction = CompareFunction.Less;
                    _itemEditorState.FPItemModel.Draw(
                        _itemEditorState.FirstPersonCamera.View, _itemEditorState.FirstPersonCamera.Projection, _effect);
                }
            }
            else
            {
                _coordinateAxes.DrawCoordinateSystem(_itemEditorState.MainCamera);
                _editorCellGrid.Draw(_itemEditorState.MainCamera);

                if (_itemEditorState.EditedItemModel != null)
                {
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    _effect.AlphaFunction = CompareFunction.Equal;
                    _itemEditorState.EditedItemModel.Draw(
                        _itemEditorState.MainCamera.View, _itemEditorState.MainCamera.Projection, _effect);

                    _graphicsDevice.BlendState = BlendState.AlphaBlend;
                    _effect.AlphaFunction = CompareFunction.Less;
                    _itemEditorState.EditedItemModel.Draw(
                        _itemEditorState.MainCamera.View, _itemEditorState.MainCamera.Projection, _effect);
                }
            }
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
        }

        public void Dispose()
        {
            _effect.Dispose();
        }
    }
}