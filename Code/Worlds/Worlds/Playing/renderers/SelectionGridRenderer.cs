using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Playing.VertexTypes;
using WorldsGame.Renderers;
using WorldsGame.Utils;

namespace WorldsGame.Playing.Renderers
{
    internal class SelectionGridRenderer : IRenderer, IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SelectionGrid _selectionGrid;
        private VertexBuffer _vertexBuffer;
        private Dictionary<int, Texture2D> _textureAtlases;
        private Effect _blockEffect;

        public SelectionGridRenderer(GraphicsDevice graphicsDevice, SelectionGrid selectionGrid)
        {
            _graphicsDevice = graphicsDevice;
            _selectionGrid = selectionGrid;
        }

        public void Initialize()
        {
        }

        public void Draw(GameTime gameTime)
        {
        }

        public void DrawTransparent(GameTime gameTime)
        {
            //            if (_selectionGrid.SelectionStage == SelectionStage.Hidden)
            //            {
            //                return;
            //            }

            Texture2D textureAtlas = _textureAtlases[GameBundleCompiler.SYSTEM_ATLAS_ID];

            _blockEffect.Parameters["TextureAtlas"].SetValue(textureAtlas);

            if (_selectionGrid.AreVerticesChanged)
            {
                if (_vertexBuffer != null)
                {
                    _vertexBuffer.Dispose();
                }

                if (_selectionGrid.Vertices.Count == 0)
                {
                    return;
                }

                _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTextureLight),
                                                 _selectionGrid.Vertices.Count, BufferUsage.WriteOnly);
                _vertexBuffer.SetData(_selectionGrid.Vertices.ToArray());

                _selectionGrid.AreVerticesChanged = false;
            }

            if (_vertexBuffer == null)
            {
                return;
            }

            _graphicsDevice.BlendState = BlendState.NonPremultiplied;

            foreach (EffectPass pass in _blockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.SetVertexBuffer(_vertexBuffer);
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount / 3);
            }

            _graphicsDevice.BlendState = BlendState.Opaque;
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            _textureAtlases = worldsContentLoader.TextureAtlases;
            _blockEffect = worldsContentLoader.SolidBlockEffect;
        }

        public void Dispose()
        {
            _selectionGrid.Dispose();
        }
    }
}