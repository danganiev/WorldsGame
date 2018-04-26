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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Models;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.VertexTypes;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Terrain.Blocks.VerticeBuilder;
using WorldsGame.View;

using WorldsLib;

namespace WorldsGame.Terrain.Chunks.Processors
{
    internal class VertexBuildChunkProcessor : IChunkProcessor, IDisposable
    {
        private readonly object _locker = new object();
        private readonly GraphicsDevice _graphicsDevice;
        private readonly World _world;
        private readonly Dictionary<int, List<VertexPositionTextureLight>> _vertexListDictionary = new Dictionary<int, List<VertexPositionTextureLight>>();
        private readonly Dictionary<int, List<short>> _indexListDictionary = new Dictionary<int, List<short>>();
        private readonly Dictionary<int, List<VertexFloatPositionTextureLight>> _customVertexListDictionary = new Dictionary<int, List<VertexFloatPositionTextureLight>>();
        private readonly Dictionary<int, List<short>> _customIndexListDictionary = new Dictionary<int, List<short>>();
        private readonly Dictionary<int, List<VertexPositionTextureLight>> _transparentVertexListDictionary = new Dictionary<int, List<VertexPositionTextureLight>>();
        private readonly Dictionary<int, List<short>> _transparentIndexListDictionary = new Dictionary<int, List<short>>();
        private readonly Dictionary<int, List<VertexPositionTextureLight>> _animatedVertexListDictionary = new Dictionary<int, List<VertexPositionTextureLight>>();
        private readonly Dictionary<int, List<short>> _animatedIndexListDictionary = new Dictionary<int, List<short>>();

        private readonly VerticeBuilder _verticeBuilder = new VerticeBuilder();

        private IndexBuffer _preparedIndexBuffer;

        internal VertexBuildChunkProcessor(GraphicsDevice graphicsDevice, World world)
        {
            _graphicsDevice = graphicsDevice;
            _world = world;
            Chunkie = null;

            foreach (KeyValuePair<int, TextureAtlas> atlas in _world.CompiledGameBundle.TextureAtlases)
            {
                _vertexListDictionary.Add(atlas.Key, new List<VertexPositionTextureLight>());
                _indexListDictionary.Add(atlas.Key, new List<short>());
                _customVertexListDictionary.Add(atlas.Key, new List<VertexFloatPositionTextureLight>());
                _customIndexListDictionary.Add(atlas.Key, new List<short>());
                _transparentVertexListDictionary.Add(atlas.Key, new List<VertexPositionTextureLight>());
                _transparentIndexListDictionary.Add(atlas.Key, new List<short>());
                _animatedVertexListDictionary.Add(atlas.Key, new List<VertexPositionTextureLight>());
                _animatedIndexListDictionary.Add(atlas.Key, new List<short>());
            }
        }

        internal Chunk Chunkie { get; set; }

        public void ProcessChunk(Chunk chunk)
        {
            lock (chunk)
            {
                if (chunk.IsDisposing)
                {
                    return;
                }

                lock (_locker)
                {
                    Chunkie = chunk;
                    Chunkie.Clear();

                    _verticeBuilder.Chunkie = Chunkie;

                    BuildVertexList();
                }
            }
        }

        private void BuildVertexList()
        {
            for (int x = 0; x < Chunk.SIZE.X; x++)
            {
                for (int z = 0; z < Chunk.SIZE.Z; z++)
                {
                    int offset = x * Chunk.FLATTEN_OFFSET + z * Chunk.SIZE.Y;

                    for (int y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        BlockType block = Chunkie.GetBlock(offset, y);

                        if (block != null && !block.IsAirType())
                        {
                            if (block.IsLiquid)
                            {
                                _verticeBuilder.BuildLiquidVertexList(
                                     block, new Vector3i(x, y, z), _transparentVertexListDictionary,
                                     _transparentIndexListDictionary);
                            }
                            else if (block.IsCubical)
                            {
                                // we reuse the same function cause the only think that matters is the order of drawing, and we have different buffers for it
                                _verticeBuilder.BuildVertexList(
                                    block, new Vector3i(x, y, z), _vertexListDictionary,
                                    _indexListDictionary, _transparentVertexListDictionary,
                                    _transparentIndexListDictionary, _animatedVertexListDictionary,
                                    _animatedIndexListDictionary);
                            }
                            else
                            {
                                _verticeBuilder.BuildCustomVertexList(
                                    block, new Vector3i(x, y, z), _customVertexListDictionary,
                                    _customIndexListDictionary);
                            }
                        }
                    }
                }
            }

            // ТОЛЬКО НЕ ТУТ, А В КЛАССЕ ИНТЕРПОЛЯТОРА СВЕТА

            //Тут надо интерполировать свет, но не так интенсивно, как делали до этого,
            // а учитывать, не окружен ли блок темными блоками, плюс делать это иными, наверное методами
            // менее жрущими ресурсы.

            //TODO: Это можно перенести в WorldRenderer, а оттуда вообще выделить в отдельные классы
            foreach (KeyValuePair<int, TextureAtlas> atlas in Chunkie.World.CompiledGameBundle.TextureAtlases)
            {
                PrepareCubicalBuffers(atlas);
                PrepareCustomBuffers(atlas);
                PrepareTransparentBuffers(atlas);
                PrepareAnimatedBuffers(atlas);
            }
        }

        private void PrepareCubicalBuffers(KeyValuePair<int, TextureAtlas> atlas)
        {// 0 2 1 1 2 3 7 6 4 7 4 5
            VertexPositionTextureLight[] chunkVerticesArray = _vertexListDictionary[atlas.Key].ToArray();
            _vertexListDictionary[atlas.Key].Clear();

            short[] chunkIndicesArray2 = _indexListDictionary[atlas.Key].ToArray();

            PrepareIndexBuffer();

            _indexListDictionary[atlas.Key].Clear();

            if (chunkVerticesArray.Length > 0)
            {
                if (Chunkie.GetOpaqueVertexBuffer(atlas.Key) != null)
                {
                    Chunkie.GetOpaqueVertexBuffer(atlas.Key).Dispose();
                }

                var vbuffer = new VertexBuffer(_graphicsDevice, VertexPositionTextureLight.VertexDeclaration,
                                               chunkVerticesArray.Length, BufferUsage.WriteOnly);

                vbuffer.SetData(chunkVerticesArray);
                Chunkie.SetOpaqueVertexBuffer(atlas.Key, vbuffer);

                //                if (Chunkie.GetOpaqueIndexBuffer(atlas.Key) != null)
                //                {
                //                    Chunkie.GetOpaqueIndexBuffer(atlas.Key).Dispose();
                //                }

                var ibuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits,
                                              chunkIndicesArray2.Length, BufferUsage.WriteOnly);

                ibuffer.SetData(chunkIndicesArray2);
                //                Chunkie.SetOpaqueIndexBuffer(atlas.Key, ibuffer);
                Chunkie.SetOpaqueIndexBuffer(atlas.Key, _preparedIndexBuffer);
            }
            else
            {
                Chunkie.SetOpaqueVertexBuffer(atlas.Key, null);
                Chunkie.SetOpaqueIndexBuffer(atlas.Key, null);
            }
        }

        private void PrepareIndexBuffer()
        {
            if (_preparedIndexBuffer == null)
            {
                short[] chunkIndicesArray = new short[16384];
                short a = 0;
                int i = 0;
                try
                {
                    while (i < chunkIndicesArray.Length)
                    {
                        //                        chunkIndicesArray[i] = a;
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 2);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 1);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 1);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 2);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 3);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 7);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 6);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 4);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 7);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 4);
                        //                        i++;
                        //                        chunkIndicesArray[i] = (short)(a + 5);
                        //                        i++;
                        //
                        //                        a += 8;

                        chunkIndicesArray[i] = a;
                        i++;
                        chunkIndicesArray[i] = (short)(a + 1);
                        i++;
                        chunkIndicesArray[i] = (short)(a + 2);
                        i++;
                        chunkIndicesArray[i] = (short)(a + 2);
                        i++;
                        chunkIndicesArray[i] = (short)(a + 1);
                        i++;
                        chunkIndicesArray[i] = (short)(a + 3);
                        i++;

                        a += 4;
                    }
                }
                catch (Exception e)
                {
                }

                _preparedIndexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits,
                                                       chunkIndicesArray.Length, BufferUsage.WriteOnly);

                _preparedIndexBuffer.SetData(chunkIndicesArray);
            }
        }

        private void PrepareCustomBuffers(KeyValuePair<int, TextureAtlas> atlas)
        {
            VertexFloatPositionTextureLight[] chunkVerticesArray = _customVertexListDictionary[atlas.Key].ToArray();
            _customVertexListDictionary[atlas.Key].Clear();

            short[] chunkIndicesArray = _customIndexListDictionary[atlas.Key].ToArray();
            _customIndexListDictionary[atlas.Key].Clear();

            if (chunkVerticesArray.Length > 0)
            {
                if (Chunkie.GetCustomVertexBuffer(atlas.Key) != null)
                {
                    Chunkie.GetCustomVertexBuffer(atlas.Key).Dispose();
                }

                var vbuffer = new VertexBuffer(_graphicsDevice, VertexFloatPositionTextureLight.VertexDeclaration,
                                               chunkVerticesArray.Length, BufferUsage.WriteOnly);

                vbuffer.SetData(chunkVerticesArray);
                Chunkie.SetCustomVertexBuffer(atlas.Key, vbuffer);

                if (Chunkie.GetCustomIndexBuffer(atlas.Key) != null)
                {
                    Chunkie.GetCustomIndexBuffer(atlas.Key).Dispose();
                }

                var ibuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits,
                                              chunkIndicesArray.Length, BufferUsage.WriteOnly);

                ibuffer.SetData(chunkIndicesArray);
                Chunkie.SetCustomIndexBuffer(atlas.Key, ibuffer);
            }
            else
            {
                Chunkie.SetCustomVertexBuffer(atlas.Key, null);
                Chunkie.SetCustomIndexBuffer(atlas.Key, null);
            }
        }

        private void PrepareTransparentBuffers(KeyValuePair<int, TextureAtlas> atlas)
        {
            VertexPositionTextureLight[] chunkVerticesArray = _transparentVertexListDictionary[atlas.Key].ToArray();
            _transparentVertexListDictionary[atlas.Key].Clear();

            short[] chunkIndicesArray = _transparentIndexListDictionary[atlas.Key].ToArray();
            _transparentIndexListDictionary[atlas.Key].Clear();

            if (chunkVerticesArray.Length > 0)
            {
                if (Chunkie.GetTransparentVertexBuffer(atlas.Key) != null)
                {
                    Chunkie.GetTransparentVertexBuffer(atlas.Key).Dispose();
                }

                var vbuffer = new VertexBuffer(_graphicsDevice, VertexPositionTextureLight.VertexDeclaration,
                                               chunkVerticesArray.Length, BufferUsage.WriteOnly);

                vbuffer.SetData(chunkVerticesArray);
                Chunkie.SetTransparentVertexBuffer(atlas.Key, vbuffer);

                if (Chunkie.GetTransparentIndexBuffer(atlas.Key) != null)
                {
                    Chunkie.GetTransparentIndexBuffer(atlas.Key).Dispose();
                }

                var ibuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits,
                                              chunkIndicesArray.Length, BufferUsage.WriteOnly);

                ibuffer.SetData(chunkIndicesArray);
                Chunkie.SetTransparentIndexBuffer(atlas.Key, ibuffer);
            }
            else
            {
                Chunkie.SetTransparentVertexBuffer(atlas.Key, null);
                Chunkie.SetTransparentIndexBuffer(atlas.Key, null);
            }
        }

        private void PrepareAnimatedBuffers(KeyValuePair<int, TextureAtlas> atlas)
        {
            VertexPositionTextureLight[] chunkVerticesArray = _animatedVertexListDictionary[atlas.Key].ToArray();
            _animatedVertexListDictionary[atlas.Key].Clear();

            //            short[] chunkIndicesArray = _animatedIndexListDictionary[atlas.Key].ToArray();
            _animatedIndexListDictionary[atlas.Key].Clear();

            if (chunkVerticesArray.Length > 0)
            {
                if (Chunkie.GetAnimatedVertexBuffer(atlas.Key) != null)
                {
                    Chunkie.GetAnimatedVertexBuffer(atlas.Key).Dispose();
                }

                var vbuffer = new VertexBuffer(_graphicsDevice, VertexPositionTextureLight.VertexDeclaration,
                                               chunkVerticesArray.Length, BufferUsage.WriteOnly);

                vbuffer.SetData(chunkVerticesArray);
                Chunkie.SetAnimatedVertexBuffer(atlas.Key, vbuffer);

                //                if (Chunkie.GetAnimatedIndexBuffer(atlas.Key) != null)
                //                {
                //                    Chunkie.GetAnimatedIndexBuffer(atlas.Key).Dispose();
                //                }

                //                var ibuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits,
                //                                              chunkIndicesArray.Length, BufferUsage.WriteOnly);
                //
                //                ibuffer.SetData(chunkIndicesArray);
                //                Chunkie.SetAnimatedIndexBuffer(atlas.Key, ibuffer);
                Chunkie.SetAnimatedIndexBuffer(atlas.Key, _preparedIndexBuffer);
            }
            else
            {
                Chunkie.SetAnimatedVertexBuffer(atlas.Key, null);
                Chunkie.SetAnimatedIndexBuffer(atlas.Key, null);
            }
        }

        public void Dispose()
        {
            _preparedIndexBuffer.Dispose();
        }
    }
}