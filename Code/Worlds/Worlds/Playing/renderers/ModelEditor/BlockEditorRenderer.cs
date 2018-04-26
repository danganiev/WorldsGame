using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Camera;
using WorldsGame.Editors.Blocks;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Renderers;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WorldsGame.Playing.Renderers
{
    internal class BlockEditorRenderer : IRenderer
    {
        private AlphaTestEffect _effect;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ArcBallCamera _camera;

        private readonly CoordinateAxes _coordinateAxes;
        private readonly EditorCellGrid _editorCellGrid;

        internal EditedModel EditedBlock { get; set; }

        internal AlphaTestEffect Effect
        {
            get { return _effect; }
        }

        internal BlockEditorRenderer(GraphicsDevice graphicsDevice, ArcBallCamera camera, EditedModel editedBlock)
        {
            _graphicsDevice = graphicsDevice;
            _camera = camera;
            EditedBlock = editedBlock;

            _coordinateAxes = new CoordinateAxes(_graphicsDevice, editedBlock);
            _editorCellGrid = new EditorCellGrid(_graphicsDevice, editedBlock);

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
            Effect.AlphaFunction = CompareFunction.Equal;

            EditedBlock.Draw(_camera.View, _camera.Projection, Effect);

            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            Effect.AlphaFunction = CompareFunction.Less;
            EditedBlock.Draw(_camera.View, _camera.Projection, Effect);
        }

        public Texture2D RenderModelToTexture()
        {
            var device = _graphicsDevice;
            PresentationParameters pp = device.PresentationParameters;
            Texture2D result;

            using (var renderTarget = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, true, device.DisplayMode.Format, DepthFormat.Depth24))
            {
                var tempCamera = new ArcBallCamera(device.Viewport, null);
                tempCamera.Initialize();

                _graphicsDevice.SetRenderTarget(renderTarget);

                _graphicsDevice.Clear(new Color(0, 0, 0, 0));

                _graphicsDevice.BlendState = BlendState.Opaque;
                Effect.AlphaFunction = CompareFunction.Equal;

                EditedBlock.Draw(tempCamera.View, tempCamera.Projection, Effect);

                _graphicsDevice.BlendState = BlendState.AlphaBlend;
                Effect.AlphaFunction = CompareFunction.Less;
                EditedBlock.Draw(tempCamera.View, tempCamera.Projection, Effect);

                _graphicsDevice.SetRenderTarget(null);

                int screenPixelWidthToHeightDiff = (pp.BackBufferWidth - pp.BackBufferHeight) / 2;

                Color[] data = new Color[pp.BackBufferHeight * pp.BackBufferHeight];

                renderTarget.GetData(0,
                                     new Rectangle(screenPixelWidthToHeightDiff, 0, pp.BackBufferHeight,
                                                   pp.BackBufferHeight), data, 0, data.Length);

                using (var newTexture = new Texture2D(device, pp.BackBufferHeight, pp.BackBufferHeight))
                {
                    newTexture.SetData(data);

                    using (var stream = new MemoryStream())
                    {
                        newTexture.SaveAsPng(stream, 24, 24);
                        result = Texture2D.FromStream(device, stream);
                    }
                }
            }

            return result;
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
        }

        public void Dispose()
        {
            Effect.Dispose();
        }
    }
}