using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using WorldsGame.Models;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.VertexTypes;
using WorldsGame.Terrain.Blocks.Types;

using WorldsGame.View.Blocks;
using WorldsLib;

namespace WorldsGame.Terrain.Blocks.VerticeBuilder
{
    internal class VerticeBuilder
    {
        private Dictionary<int, List<VertexPositionTextureLight>> _vertexListDictionary;
        private Dictionary<int, List<short>> _indexListDictionary;
        private Dictionary<int, List<VertexPositionTextureLight>> _transparentVListDictionary;
        private Dictionary<int, List<short>> _transparentIListDictionary;
        private Dictionary<int, List<VertexPositionTextureLight>> _animatedVListDictionary;
        private Dictionary<int, List<short>> _animatedIListDictionary;

        internal Chunk Chunkie { get; set; }

        // Cube drawing
        protected void AddVerticesAndIndices(BlockType block, Vector3i blockPosition, CubeFaceDirection cubeFaceDirection,
            Dictionary<Vector3, List<BlockType>> occlusionDictionary)
        {
            if (block.BlockParts.Count == 0)
            {
                return;
            }

            var blockPart = block.BlockParts[cubeFaceDirection];
            int atlasIndex = blockPart.Texture.AtlasIndex;

            Dictionary<int, List<VertexPositionTextureLight>> vDictionary;
            Dictionary<int, List<short>> iDictionary;

            if (blockPart.IsAnimated)
            {
                vDictionary = _animatedVListDictionary;
                iDictionary = _animatedIListDictionary;
            }
            else if (blockPart.IsTransparent)
            {
                vDictionary = _transparentVListDictionary;
                iDictionary = _transparentIListDictionary;
            }
            else
            {
                vDictionary = _vertexListDictionary;
                iDictionary = _indexListDictionary;
            }

            for (int index = 0; index < blockPart.VertexList.Length; index++)
            {
                Vector3 vertex = blockPart.VertexList[index];
                short occlusion = CalculateOcclusion(occlusionDictionary[vertex]);
                AddVertex(blockPosition, vertex, blockPart.UVMappings[blockPart.TextureEntranceIndexList[index]], atlasIndex, occlusion, vDictionary);
            }

            AddIndices(blockPart.IndicesList, atlasIndex, iDictionary, Chunkie.GetVertexCount(atlasIndex));

            //Do not move the underlying line up, it's in the right place now.
            Chunkie.SetVertexCount(atlasIndex, (short)(Chunkie.GetVertexCount(atlasIndex) + blockPart.VertexList.Length));
        }

        private void AddVertex(Vector3i blockPosition, Vector3 vertexAdd, Vector2 textureCoordinate, int atlasIndex, short occlusion, 
            Dictionary<int, List<VertexPositionTextureLight>> vertexListDictionary)
        {
            Vector3 position = blockPosition.AsVector3() + vertexAdd;

            vertexListDictionary[atlasIndex].Add(new VertexPositionTextureLight(new Short4(position.X, position.Y, position.Z, occlusion),
                new NormalizedShort2(textureCoordinate), Color.Black));
        }

        private void AddIndices(IList<short> indices, int atlasIndex, Dictionary<int, List<short>> indexListDictionary, short verticeCount)
        {
            var newIndices = new short[indices.Count];

            for (int i = 0; i < indices.Count; i++)
            {
                var index = indices[i];
                //                newIndices[i] = (short)(Chunkie.GetVertexCount(atlasIndex) + index);
                newIndices[i] = (short)(verticeCount + index);
            }

            // Sync problem here
            indexListDictionary[atlasIndex].AddRange(newIndices);
        }

        internal void BuildLiquidVertexList(BlockType block, Vector3i chunkRelativePosition,
            Dictionary<int, List<VertexPositionTextureLight>> vertexListDictionary, Dictionary<int, List<short>> indexListDictionary)
        {
            _vertexListDictionary = vertexListDictionary;
            _indexListDictionary = indexListDictionary;

            Vector3i blockPosition = Chunkie.DrawPosition + chunkRelativePosition;

            BuildCubicalVertexList(block, chunkRelativePosition, blockPosition);
        }

        internal void BuildVertexList(BlockType block, Vector3i chunkRelativePosition,
            Dictionary<int, List<VertexPositionTextureLight>> vertexListDictionary, Dictionary<int, List<short>> indexListDictionary,
            Dictionary<int, List<VertexPositionTextureLight>> transparentVListDictionary, Dictionary<int, List<short>> transparentIListDictionary,
            Dictionary<int, List<VertexPositionTextureLight>> animatedVListDictionary, Dictionary<int, List<short>> animatedIListDictionary)
        {
            _vertexListDictionary = vertexListDictionary;
            _indexListDictionary = indexListDictionary;
            _transparentVListDictionary = transparentVListDictionary;
            _transparentIListDictionary = transparentIListDictionary;
            _animatedVListDictionary = animatedVListDictionary;
            _animatedIListDictionary = animatedIListDictionary;

            Vector3i blockPosition = Chunkie.DrawPosition + chunkRelativePosition;

            BuildCubicalVertexList(block, chunkRelativePosition, blockPosition);
        }

        private void BuildCubicalVertexList(BlockType block, Vector3i chunkRelativePosition, Vector3i blockPosition)
        {
            //get signed bytes from these to be able to remove 1 without further casts
            var X = (sbyte)chunkRelativePosition.X;
            var Y = (sbyte)chunkRelativePosition.Y;
            var Z = (sbyte)chunkRelativePosition.Z;

            BlockType blockMidW, blockBotM, blockMidE, blockMidN, blockMidS, blockTopM;

            blockTopM = Chunkie.RelativeGetBlock(X, Y + 1, Z);
            blockMidN = Chunkie.RelativeGetBlock(X, Y, Z + 1);
            blockMidW = Chunkie.RelativeGetBlock(X - 1, Y, Z);
            blockMidE = Chunkie.RelativeGetBlock(X + 1, Y, Z);
            blockMidS = Chunkie.RelativeGetBlock(X, Y, Z - 1);
            blockBotM = Chunkie.RelativeGetBlock(X, Y - 1, Z);

            Dictionary<Vector3, List<BlockType>> occlusionDictionary = null;

            if (IsTransparentBlock(blockMidW))
            {
                occlusionDictionary = MakeOcclusionDictionary(X, Y, Z);
                AddVerticesAndIndices(block, blockPosition, CubeFaceDirection.Left, occlusionDictionary);
            }

            if (IsTransparentBlock(blockMidE))
            {
                if (occlusionDictionary == null)
                {
                    occlusionDictionary = MakeOcclusionDictionary(X, Y, Z);
                }
                AddVerticesAndIndices(block, blockPosition, CubeFaceDirection.Right, occlusionDictionary);
            }

            if (IsTransparentBlock(blockBotM))
            {
                if (occlusionDictionary == null)
                {
                    occlusionDictionary = MakeOcclusionDictionary(X, Y, Z);
                }
                AddVerticesAndIndices(block, blockPosition, CubeFaceDirection.Down, occlusionDictionary);
            }

            if (IsTransparentBlock(blockTopM))
            {
                if (occlusionDictionary == null)
                {
                    occlusionDictionary = MakeOcclusionDictionary(X, Y, Z);
                }
                AddVerticesAndIndices(block, blockPosition, CubeFaceDirection.Up, occlusionDictionary);
            }

            if (IsTransparentBlock(blockMidS))
            {
                if (occlusionDictionary == null)
                {
                    occlusionDictionary = MakeOcclusionDictionary(X, Y, Z);
                }
                AddVerticesAndIndices(block, blockPosition, CubeFaceDirection.Forward, occlusionDictionary);
            }

            if (IsTransparentBlock(blockMidN))
            {
                if (occlusionDictionary == null)
                {
                    occlusionDictionary = MakeOcclusionDictionary(X, Y, Z);
                }
                AddVerticesAndIndices(block, blockPosition, CubeFaceDirection.Back, occlusionDictionary);
            }
        }

        internal void BuildCustomVertexList(BlockType block, Vector3i chunkRelativePosition,
            Dictionary<int, List<VertexFloatPositionTextureLight>> vertexListDictionary,
            Dictionary<int, List<short>> indexListDictionary)
        {
            Vector3i blockPosition = Chunkie.DrawPosition + chunkRelativePosition;

            foreach (CustomEntityPart customBlockPart in block.CustomBlockParts)
            {
                int atlasIndex = customBlockPart.AtlasIndex;

                for (int i = 0; i < customBlockPart.VertexList.Length; i++)
                {
                    Vector3 vertex = customBlockPart.VertexList[i];
                    AddCustomVertex(blockPosition, vertex, customBlockPart.UVMappings[i], atlasIndex, vertexListDictionary);
                }

                AddIndices(CustomEntityPart.INDICES_LIST, atlasIndex, indexListDictionary, Chunkie.GetCustomVertexCount(atlasIndex));

                // Do not move the underlying line up, it's in the right place now.
                // I think the bug in mixing custom blocks and normal blocks, is because we mix vertex count
                Chunkie.SetCustomVertexCount(
                    atlasIndex, (short)(Chunkie.GetCustomVertexCount(atlasIndex) + customBlockPart.VertexList.Length));
            }
        }

        private void AddCustomVertex(Vector3i blockPosition, Vector3 vertexAdd, Vector2 textureCoordinate, int atlasIndex,
            Dictionary<int, List<VertexFloatPositionTextureLight>> vertexListDictionary)
        {
            Vector3 position = blockPosition.AsVector3() + vertexAdd;

            vertexListDictionary[atlasIndex].Add(new VertexFloatPositionTextureLight(new Vector4(position.X, position.Y, position.Z, 1),
                new NormalizedShort2(textureCoordinate), Color.Black));
        }

        private short CalculateOcclusion(List<BlockType> blockTypes)
        {
            int side1 = IsTransparentBlock(blockTypes[0]) ? 0 : 1;
            int side2 = IsTransparentBlock(blockTypes[1]) ? 0 : 1;
            int corner = IsTransparentBlock(blockTypes[2]) ? 0 : 1;

            if (side1 == 1 && side2 == 1)
            {
                return 0;
            }

            return (short)(3 - (side1 + side2 + corner));
        }

        private Dictionary<Vector3, List<BlockType>> MakeOcclusionDictionary(sbyte X, sbyte Y, sbyte Z)
        {
            //occlusionBlocks
            BlockType blockTopN = Chunkie.RelativeGetBlock(X, Y + 1, Z + 1);
            BlockType blockTopNE = Chunkie.RelativeGetBlock(X + 1, Y + 1, Z + 1);
            BlockType blockTopE = Chunkie.RelativeGetBlock(X + 1, Y + 1, Z);
            BlockType blockTopSE = Chunkie.RelativeGetBlock(X + 1, Y + 1, Z - 1);
            BlockType blockTopS = Chunkie.RelativeGetBlock(X, Y + 1, Z - 1);
            BlockType blockTopSW = Chunkie.RelativeGetBlock(X - 1, Y + 1, Z - 1);
            BlockType blockTopW = Chunkie.RelativeGetBlock(X - 1, Y + 1, Z);
            BlockType blockTopNW = Chunkie.RelativeGetBlock(X - 1, Y + 1, Z + 1);

            BlockType blockMiddleN = Chunkie.RelativeGetBlock(X, Y, Z + 1);
            BlockType blockMiddleNE = Chunkie.RelativeGetBlock(X + 1, Y, Z + 1);
            BlockType blockMiddleE = Chunkie.RelativeGetBlock(X + 1, Y, Z);
            BlockType blockMiddleSE = Chunkie.RelativeGetBlock(X + 1, Y, Z - 1);
            BlockType blockMiddleS = Chunkie.RelativeGetBlock(X, Y, Z - 1);
            BlockType blockMiddleSW = Chunkie.RelativeGetBlock(X - 1, Y, Z - 1);
            BlockType blockMiddleW = Chunkie.RelativeGetBlock(X - 1, Y, Z);
            BlockType blockMiddleNW = Chunkie.RelativeGetBlock(X - 1, Y, Z + 1);

            var occlusionDictionary = new Dictionary<Vector3, List<BlockType>>
            {
                {new Vector3(0,0,0), new List<BlockType>{blockMiddleS, blockMiddleW, blockMiddleSW}},
                {new Vector3(0,0,1), new List<BlockType>{blockMiddleW, blockMiddleN, blockMiddleNW}},
                {new Vector3(1,0,1), new List<BlockType>{blockMiddleN, blockMiddleE, blockMiddleNE}},
                {new Vector3(1,0,0), new List<BlockType>{blockMiddleE, blockMiddleS, blockMiddleSE}},
                {new Vector3(0,1,0), new List<BlockType>{blockTopS, blockTopW, blockTopSW}},
                {new Vector3(0,1,1), new List<BlockType>{blockTopW, blockTopN, blockTopNW}},
                {new Vector3(1,1,1), new List<BlockType>{blockTopN, blockTopE, blockTopNE}},
                {new Vector3(1,1,0), new List<BlockType>{blockTopE, blockTopS, blockTopSE}},
            };

            return occlusionDictionary;
        }

        private bool IsTransparentBlock(BlockType block)
        {
            //block != null is just for problems on chunk startup generation
            return block != null && ((block.IsTransparent || block == BlockTypeHelper.WaitBlockType) || !block.IsCubical);
        }
    }
}