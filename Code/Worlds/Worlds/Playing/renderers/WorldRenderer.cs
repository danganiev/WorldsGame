#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion License

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Models;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Terrain;

namespace WorldsGame.Renderers
{
    internal class WorldRenderer : IRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly World _world;
        private readonly Player _player;

        //        private bool _areStaticEffectsSet;
        private WorldsContentLoader _worldsContentLoader;

        private Effect _solidBlockEffect;
        private Effect _liquidBlockEffect;
        private Effect _animatedBlockEffect;
        private Effect _customBlockEffect;

        private Dictionary<int, Texture2D> _textureAtlases;
        private Dictionary<int, float> _textureAtlasesSteps;

        internal WorldRenderer(GraphicsDevice graphicsDevice, World world)
        {
            _graphicsDevice = graphicsDevice;
            _world = world;
            _player = _world.ClientPlayer;
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            _worldsContentLoader = worldsContentLoader;

            _textureAtlases = _worldsContentLoader.TextureAtlases;
            _solidBlockEffect = _worldsContentLoader.SolidBlockEffect;
            _liquidBlockEffect = _worldsContentLoader.LiquidBlockEffect;
            _animatedBlockEffect = _worldsContentLoader.AnimatedBlockEffect;
            _customBlockEffect = _worldsContentLoader.CustomModelEffect;

            //            _solidBlockEffect.Parameters["SunColor"].SetValue(World.SUNCOLOR);
            //            _customModelEffect.Parameters["SunColor"].SetValue(World.SUNCOLOR);
            _solidBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _customBlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            _animatedBlockEffect.Parameters["World"].SetValue(Matrix.Identity);

            _textureAtlasesSteps = new Dictionary<int, float>();
            foreach (KeyValuePair<int, Texture2D> atlas in _textureAtlases)
            {
                _textureAtlasesSteps[atlas.Key] = (float)CompiledTexture.MAX_TEXTURE_SIZE / atlas.Value.Width;
            }
        }

        public void Draw(GameTime gameTime)
        {
            _solidBlockEffect.Parameters["View"].SetValue(_player.CameraView);
            _solidBlockEffect.Parameters["Projection"].SetValue(_player.CameraProjection);
            _solidBlockEffect.Parameters["CameraPosition"].SetValue(_player.CameraPosition);
            _solidBlockEffect.Parameters["TimeOfDay"].SetValue(_world.TimeOfDay);
            _solidBlockEffect.Parameters["CurrentAtmosphereColor"].SetValue(_world.CurrentAtmosphereColor.ToVector4());
            _solidBlockEffect.Parameters["NextAtmosphereColor"].SetValue(_world.TimeUpdater.NextAtmosphereColor.ToVector4());
            _solidBlockEffect.Parameters["PreviousHour"].SetValue(_world.TimeUpdater.PreviousHour);
            _solidBlockEffect.Parameters["NextHour"].SetValue(_world.TimeUpdater.NextHour);

            // TODO: This could be optimized, and set per whole drawing loop/tick just once (since this doesn't change)
            _customBlockEffect.Parameters["View"].SetValue(_player.CameraView);
            _customBlockEffect.Parameters["Projection"].SetValue(_player.CameraProjection);
            _customBlockEffect.Parameters["CameraPosition"].SetValue(_player.CameraPosition);
            _customBlockEffect.Parameters["TimeOfDay"].SetValue(_world.TimeOfDay);
            _customBlockEffect.Parameters["CurrentAtmosphereColor"].SetValue(_world.CurrentAtmosphereColor.ToVector4());
            _customBlockEffect.Parameters["NextAtmosphereColor"].SetValue(_world.TimeUpdater.NextAtmosphereColor.ToVector4());
            _customBlockEffect.Parameters["PreviousHour"].SetValue(_world.TimeUpdater.PreviousHour);
            _customBlockEffect.Parameters["NextHour"].SetValue(_world.TimeUpdater.NextHour);

            _animatedBlockEffect.Parameters["View"].SetValue(_player.CameraView);
            _animatedBlockEffect.Parameters["Projection"].SetValue(_player.CameraProjection);
            _animatedBlockEffect.Parameters["CameraPosition"].SetValue(_player.CameraPosition);
            _animatedBlockEffect.Parameters["TimeOfDay"].SetValue(_world.TimeOfDay);
            _animatedBlockEffect.Parameters["CurrentAtmosphereColor"].SetValue(_world.CurrentAtmosphereColor.ToVector4());
            _animatedBlockEffect.Parameters["NextAtmosphereColor"].SetValue(_world.TimeUpdater.NextAtmosphereColor.ToVector4());
            _animatedBlockEffect.Parameters["PreviousHour"].SetValue(_world.TimeUpdater.PreviousHour);
            _animatedBlockEffect.Parameters["NextHour"].SetValue(_world.TimeUpdater.NextHour);

            var viewFrustum = _player.ViewFrustum;

            _graphicsDevice.BlendState = BlendState.Opaque;

            foreach (KeyValuePair<int, Texture2D> textureAtlas in _textureAtlases)
            {
                _solidBlockEffect.Parameters["TextureAtlas"].SetValue(textureAtlas.Value);

                BlockPassLoop(textureAtlas, viewFrustum, ChunkBufferType.Opaque, _solidBlockEffect);
            }
        }

        public void DrawTransparent(GameTime gameTime)
        {
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            var viewFrustum = _player.ViewFrustum;

            foreach (KeyValuePair<int, Texture2D> textureAtlas in _textureAtlases)
            {
                _customBlockEffect.Parameters["TextureAtlas"].SetValue(textureAtlas.Value);
                _animatedBlockEffect.Parameters["TextureAtlas"].SetValue(textureAtlas.Value);
                _animatedBlockEffect.Parameters["TextureStep"].SetValue(_textureAtlasesSteps[textureAtlas.Key]); // since width == height
                _animatedBlockEffect.Parameters["AnimationFrame"].SetValue(_world.TimeUpdater.TextureAnimationFrame);

                BlockPassLoop(textureAtlas, viewFrustum, ChunkBufferType.Custom, _customBlockEffect);
                BlockPassLoop(textureAtlas, viewFrustum, ChunkBufferType.Transparent, _solidBlockEffect);
                BlockPassLoop(textureAtlas, viewFrustum, ChunkBufferType.Animated, _animatedBlockEffect);
            }
        }

        private void BlockPassLoop(KeyValuePair<int, Texture2D> textureAtlas, BoundingFrustum viewFrustum, ChunkBufferType bufferType, Effect blockEffect)
        {
            foreach (EffectPass pass in blockEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (Chunk chunk in _world.Chunks.Values)
                {
                    if (chunk == null)
                    {
                        continue;
                    }

                    if (chunk.IsDisposing || chunk.State != ChunkState.Ready)
                    {
                        continue;
                    }

                    if (_player.IsChunkOutOfView(chunk))
                    {
                        continue;
                    }

                    lock (chunk)
                    {
                        IndexBuffer indexBuffer = null;

                        switch (bufferType)
                        {
                            case ChunkBufferType.Opaque:
                                indexBuffer = chunk.GetOpaqueIndexBuffer(textureAtlas.Key);
                                break;

                            case ChunkBufferType.Custom:
                                indexBuffer = chunk.GetCustomIndexBuffer(textureAtlas.Key);
                                break;

                            case ChunkBufferType.Transparent:
                                indexBuffer = chunk.GetTransparentIndexBuffer(textureAtlas.Key);
                                break;

                            case ChunkBufferType.Animated:
                                indexBuffer = chunk.GetAnimatedIndexBuffer(textureAtlas.Key);
                                break;
                        }

                        if (chunk.BoundingBox.Intersects(viewFrustum) && indexBuffer != null)
                        {
                            if (indexBuffer.IndexCount > 0)
                            {
                                //                                VertexBuffer vertexBuffer = chunk.GetOpaqueVertexBuffer(textureAtlas.Key);
                                VertexBuffer vertexBuffer = null;
                                switch (bufferType)
                                {
                                    case ChunkBufferType.Opaque:
                                        vertexBuffer = chunk.GetOpaqueVertexBuffer(textureAtlas.Key);
                                        break;

                                    case ChunkBufferType.Custom:
                                        vertexBuffer = chunk.GetCustomVertexBuffer(textureAtlas.Key);
                                        break;

                                    case ChunkBufferType.Transparent:
                                        vertexBuffer = chunk.GetTransparentVertexBuffer(textureAtlas.Key);
                                        break;

                                    case ChunkBufferType.Animated:
                                        vertexBuffer = chunk.GetAnimatedVertexBuffer(textureAtlas.Key);
                                        break;
                                }

                                if (vertexBuffer != null && !indexBuffer.IsDisposed &&
                                    !vertexBuffer.IsDisposed)
                                {
                                    _graphicsDevice.Indices = indexBuffer;
                                    _graphicsDevice.SetVertexBuffer(vertexBuffer);
                                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                                          vertexBuffer.VertexCount, 0,
                                                                          indexBuffer.IndexCount / 3);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}