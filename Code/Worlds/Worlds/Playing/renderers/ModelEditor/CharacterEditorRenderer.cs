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
    internal class CharacterEditorRenderer : IRenderer
    {
        //        private BasicEffect _colorEffect;
        //        private BasicEffect _textureEffect;
        //        private AlphaTestEffect _colorEffect;

        private AlphaTestEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ArcBallCamera _camera;
        private readonly CharacterEditorState _characterEditorState;

        private readonly CoordinateAxes _coordinateAxes;
        private readonly EditorCellGrid _editorCellGrid;

        internal CharacterEditorRenderer(GraphicsDevice graphicsDevice, ArcBallCamera camera, CharacterEditorState characterEditorState)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            _characterEditorState = characterEditorState;

            _coordinateAxes = new CoordinateAxes(_graphicsDevice, _characterEditorState.EditedCharacterModel);
            _editorCellGrid = new EditorCellGrid(_graphicsDevice, _characterEditorState.EditedCharacterModel);

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

            _coordinateAxes.DrawCoordinateSystem(_camera);
            _editorCellGrid.Draw(_camera);

            _graphicsDevice.BlendState = BlendState.Opaque;
            _effect.AlphaFunction = CompareFunction.Equal;
            _characterEditorState.EditedCharacterModel.Draw(_camera.View, _camera.Projection, _effect);

            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _effect.AlphaFunction = CompareFunction.Less;
            _characterEditorState.EditedCharacterModel.Draw(_camera.View, _camera.Projection, _effect);
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
        }

        public void Dispose()
        {
            //            _colorEffect.Dispose();
            _effect.Dispose();
        }
    }
}