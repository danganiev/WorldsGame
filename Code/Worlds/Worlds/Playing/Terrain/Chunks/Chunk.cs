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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils;
using WorldsLib;
using RNG = WorldsGame.Utils.RandomNumberGenerator;

namespace WorldsGame.Models
{
    internal enum ChunkState
    {
        New,
        Generated,
        Verticing,
        Ready
    }

    internal class Chunk : IDisposable
    {
        internal static readonly Vector3i SIZE = ChunkHelper.SIZE;
        internal static readonly Vector3i MAX_VECTOR = ChunkHelper.MAX_VECTOR;

        /// <summary>
        /// Used when accessing flatten blocks array.
        /// </summary>
        internal static readonly int FLATTEN_OFFSET = 1024; //SIZE.Z * SIZE.Y; //(Yes, yes, Y!)

        internal World World { get; private set; }

        /// <summary>
        /// Contains blocks as a flattened array.
        /// </summary>
        internal PooledBlockTypeArray Blocks { get; private set; }

        internal int[] BlocksAsKeys { get { return (from block in Blocks select block.Key).ToArray(); } }

        // The overall brightness from 0-15 which can change by natural light/light sources
        // internal byte[] _lightBrightness = new byte[SIZE.X * SIZE.Z * SIZE.Y];

        // Additional coloring from light sources, mixed if needed for each block
        // internal byte[] _rLight = new byte[SIZE.X * SIZE.Z * SIZE.Y];
        // internal byte[] _gLight = new byte[SIZE.X * SIZE.Z * SIZE.Y];
        // internal byte[] _bLight = new byte[SIZE.X * SIZE.Z * SIZE.Y];

        // Bits = Sx4 Lx4 Rx8 Gx8 Bx8, where S = sunlight level, and
        // L = luminosity level from other light sources
        internal int[] LightMap = new int[SIZE.X * SIZE.Z * SIZE.Y];

        private readonly Dictionary<int, short> _vertexCount;
        private readonly Dictionary<int, short> _customVertexCount;

        internal Vector3i Position { get; private set; }

        internal Vector3i DrawPosition { get; private set; }

        internal Vector3i Index { get; private set; }

        private Vector3i _drawIndex;

        internal BoundingBox BoundingBox { get; private set; }

        internal ChunkState State { get; private set; }

        internal bool IsDisposing { get; private set; }

        // key - npc name, float - spawn rate
        internal Dictionary<string, float> SpawnData { get; private set; }

        internal List<Vector3i> PossibleSpawnPoints { get; private set; }

        // which pool id does each liquid block belongs to
        internal Dictionary<int, int> LiquidPoolData { get; private set; }

        // blocks which are gonna be updated in next update cycle
        internal List<int> BlocksUpdatedNextCycle { get; set; }

        internal void SetVertexCount(int atlasIndex, short value)
        {
            _vertexCount[atlasIndex] = value;
        }

        internal short GetVertexCount(int atlasIndex)
        {
            return _vertexCount[atlasIndex];
        }

        internal void SetCustomVertexCount(int atlasIndex, short value)
        {
            _customVertexCount[atlasIndex] = value;
        }

        internal short GetCustomVertexCount(int atlasIndex)
        {
            return _customVertexCount[atlasIndex];
        }

        internal Dictionary<int, ChunkBlocksGraphicsRepresentation> ChunkBlocksRepresentation { get; private set; }

        internal Chunk(World world, Vector3i index)
        {
            State = ChunkState.New;
            World = world;

            Blocks = World.BlockArrayPool.Acquire();

            InitializeLight();
            FillWaitBlocks();

            ChunkBlocksRepresentation = new Dictionary<int, ChunkBlocksGraphicsRepresentation>();

            _vertexCount = new Dictionary<int, short>();
            _customVertexCount = new Dictionary<int, short>();

            foreach (var atlas in World.CompiledGameBundle.TextureAtlases)
            {
                ChunkBlocksRepresentation.Add(atlas.Value.AtlasIndex, new ChunkBlocksGraphicsRepresentation());
            }

            Index = index;

            IsDisposing = false;

            SpawnData = new Dictionary<string, float>();
            PossibleSpawnPoints = new List<Vector3i>();

            LiquidPoolData = new Dictionary<int, int>();
            BlocksUpdatedNextCycle = new List<int>();

            Initialize();
        }

        private void FillWaitBlocks()
        {
            for (int x = 0; x < SIZE.X; x++)
            {
                for (int z = 0; z < SIZE.Z; z++)
                {
                    for (int y = 0; y < SIZE.Y; y++)
                    {
                        SetBlock(x, y, z, BlockTypeHelper.SystemBlockTypes[CompiledBlock.WAIT_CUBE]);
                    }
                }
            }
        }

        private void InitializeLight()
        {
            for (var x = 0; x < SIZE.X; x++)
            {
                for (var z = 0; z < SIZE.Z; z++)
                {
                    for (int y = 0; y < SIZE.Y; y++)
                    {
                        LightMap[GetOffset(x, y, z)] = 0;
                    }
                }
            }
        }

        //        internal void SetLight(int offset, Color color, byte brightness)
        //        {
        //            _lightBrightness[offset] = brightness;
        //            _rLight[offset] = color.R;
        //            _gLight[offset] = color.G;
        //            _bLight[offset] = color.B;
        //        }

        internal void Initialize()
        {
            World.Chunks[Index.X, Index.Y, Index.Z] = this;

            _drawIndex = new Vector3i(Index.X - World.WORLD_DIVIDER * World.IndexMultiplier.X, Index.Y,
                Index.Z - World.WORLD_DIVIDER * World.IndexMultiplier.Z);

            // What is the difference between Position and DrawPosition?
            Position = new Vector3i(Index.X * SIZE.X, Index.Y * SIZE.Y, Index.Z * SIZE.Z);
            DrawPosition = new Vector3i(_drawIndex.X * SIZE.X, _drawIndex.Y * SIZE.Y, _drawIndex.Z * SIZE.Z);

            BoundingBox = new BoundingBox(new Vector3(DrawPosition.X, DrawPosition.Y, DrawPosition.Z),
                new Vector3(DrawPosition.X + SIZE.X, DrawPosition.Y + SIZE.Y, DrawPosition.Z + SIZE.Z));
        }

        internal void Clear()
        {
            foreach (var textureAtlas in World.CompiledGameBundle.TextureAtlases)
            {
                SetVertexCount(textureAtlas.Key, 0);
                SetCustomVertexCount(textureAtlas.Key, 0);
            }
        }

        internal void RawSetBlock(int arrayIndex, BlockType block)
        {
            Blocks[arrayIndex] = block;
        }

        internal void SetBlock(int x, int y, int z, BlockType block)
        {
            Blocks[GetOffset(x, y, z)] = block;
        }

        internal void SetBlock(Vector3i position, BlockType block)
        {
            SetBlock(position.X, position.Y, position.Z, block);
        }

        internal void SetBlocksFromKeys(int[] keys)
        {
            Blocks.SetBlocksFromKeys(keys);
        }

        /// <summary>
        /// Gets the block only from current chunk
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        internal BlockType GetBlock(int x, int y, int z)
        {
            return Blocks[GetOffset(x, y, z)];
        }

        /// <summary>
        /// Gets the block only from current chunk
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        internal BlockType GetBlock(Vector3i position)
        {
            return Blocks[GetOffset(position.X, position.Y, position.Z)];
        }

        /// <summary>
        /// Gets the block only from current chunk
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal BlockType GetBlock(int offset, int y)
        {
            return Blocks[offset + y];
        }

        /// <summary>
        /// Can get the block even from any chunk in the world, relatively to this chunk
        /// </summary>
        /// <param name="relx"></param>
        /// <param name="rely"></param>
        /// <param name="relz"></param>
        /// <returns></returns>
        internal BlockType RelativeGetBlock(int relx, int rely, int relz)
        {
            //handle the normal simple case
            if (relx >= 0 && rely >= 0 && relz >= 0 && relx < SIZE.X && rely < SIZE.Y && relz < SIZE.Z)
            {
                BlockType block = Blocks[GetOffset(relx, rely, relz)];
                return block;
            }

            BlockType blockie = World.GetBlock(GetWorldPosition(relx, rely, relz));
            return blockie;
        }

        // Get the first 4 bits
        internal int GetSunlight(int x, int y, int z)
        {
            return (LightMap[GetOffset(x, y, z)] >> 24) & 0xF;
        }

        // Set the first 4 bits
        internal void SetSunlight(int x, int y, int z, int val)
        {
            LightMap[GetOffset(x, y, z)] = (int)(LightMap[GetOffset(x, y, z)] & 0xF0000000) | (val << 24);
        }

        private int GetLuminosity(int x, int y, int z)
        {
            return (LightMap[GetOffset(x, y, z)] >> 22) & 0xF;
        }

        private void SetLuminosity(int x, int y, int z, int val)
        {
            LightMap[GetOffset(x, y, z)] = (LightMap[GetOffset(x, y, z)] & 0x0F000000) | (val << 22);
        }

        private int GetRedLight(int x, int y, int z)
        {
            return (LightMap[GetOffset(x, y, z)] >> 16) & 0xFF;
        }

        private void SetRedLight(int x, int y, int z, int val)
        {
            LightMap[GetOffset(x, y, z)] = (LightMap[GetOffset(x, y, z)] & 0x00FF0000) | (val << 16);
        }

        private int GetGreenLight(int x, int y, int z)
        {
            return (LightMap[GetOffset(x, y, z)] >> 8) & 0xFF;
        }

        private void SetGreenLight(int x, int y, int z, int val)
        {
            LightMap[GetOffset(x, y, z)] = (LightMap[GetOffset(x, y, z)] & 0x0000FF00) | (val << 8);
        }

        private int GetBlueLight(int x, int y, int z)
        {
            return LightMap[GetOffset(x, y, z)] & 0xFF;
        }

        private void SetBlueLight(int x, int y, int z, int val)
        {
            LightMap[GetOffset(x, y, z)] = (LightMap[GetOffset(x, y, z)] & 0x000000FF) | (val);
        }

        internal void GetLightData(int x, int y, int z, out int luminosity, out int r, out int g, out int b)
        {
            luminosity = GetLuminosity(x, y, z);
            r = GetRedLight(x, y, z);
            g = GetGreenLight(x, y, z);
            b = GetBlueLight(x, y, z);
        }

        internal void SetLightData(int x, int y, int z, int luminosity, int r, int g, int b)
        {
            SetLuminosity(x, y, z, luminosity);
            SetRedLight(x, y, z, r);
            SetGreenLight(x, y, z, g);
            SetBlueLight(x, y, z, b);
        }

        internal int RelativeGetSunlight(int relx, int rely, int relz)
        {
            //handle the normal simple case
            if (relx >= 0 && rely >= 0 && relz >= 0 &&
                relx < SIZE.X && rely < SIZE.Y && relz < SIZE.Z)
            {
                return GetSunlight(relx, rely, relz);
            }

            return World.GetSunlight(GetWorldPosition(relx, rely, relz));
        }

        internal void RelativeSetSunlight(int relx, int rely, int relz, int sunlight)
        {
            //handle the normal simple case
            if (relx >= 0 && rely >= 0 && relz >= 0 &&
                relx < SIZE.X && rely < SIZE.Y && relz < SIZE.Z)
            {
                SetSunlight(relx, rely, relz, sunlight);
            }

            World.SetSunlight(GetWorldPosition(relx, rely, relz), sunlight);
        }

        internal void RelativeGetLightData(int relx, int rely, int relz,
            out int luminosity, out int r, out int g, out int b)
        {
            //handle the normal simple case
            if (relx >= 0 && rely >= 0 && relz >= 0 &&
                relx < SIZE.X && rely < SIZE.Y && relz < SIZE.Z)
            {
                GetLightData(relx, rely, relz, out luminosity, out r, out g, out b);
            }

            World.GetLightData(GetWorldPosition(relx, rely, relz), out luminosity, out r, out g, out b);
        }

        internal void RelativeSetLightData(int relx, int rely, int relz,
            int luminosity, int r, int g, int b)
        {
            //handle the normal simple case
            if (relx >= 0 && rely >= 0 && relz >= 0 &&
                relx < SIZE.X && rely < SIZE.Y && relz < SIZE.Z)
            {
                SetLightData(relx, rely, relz, luminosity, r, g, b);
            }

            World.SetLightData(GetWorldPosition(relx, rely, relz), luminosity, r, g, b);
        }

        #region N S E W NE NW SE SW Top Bottom Neighbours accessors

        //this neighbours check can not be done in constructor, there would be some holes => it has to be done at access time
        //seems there is no mem leak so no need for weak references

        // Doubling because of notion about performance (just intuition here)
        internal Vector3i NPos { get { return new Vector3i(Index.X, Index.Y, Index.Z + 1); } }

        internal Chunk N
        {
            get { return World.Chunks[Index.X, Index.Y, Index.Z + 1]; }
        }

        internal Vector3i SPos { get { return new Vector3i(Index.X, Index.Y, Index.Z - 1); } }

        internal Chunk S
        {
            get { return World.Chunks[Index.X, Index.Y, Index.Z - 1]; }
        }

        internal Vector3i EPos { get { return new Vector3i(Index.X + 1, Index.Y, Index.Z); } }

        internal Chunk E
        {
            get { return World.Chunks[Index.X + 1, Index.Y, Index.Z]; }
        }

        internal Vector3i WPos { get { return new Vector3i(Index.X - 1, Index.Y, Index.Z); } }

        internal Chunk W
        {
            get { return World.Chunks[Index.X - 1, Index.Y, Index.Z]; }
        }

        internal Chunk NW
        {
            get { return World.Chunks[Index.X - 1, Index.Y, Index.Z + 1]; }
        }

        internal Chunk NE
        {
            get { return World.Chunks[Index.X + 1, Index.Y, Index.Z + 1]; }
        }

        internal Chunk SW
        {
            get { return World.Chunks[Index.X - 1, Index.Y, Index.Z - 1]; }
        }

        internal Chunk SE
        {
            get { return World.Chunks[Index.X + 1, Index.Y, Index.Z - 1]; }
        }

        internal Chunk Top
        {
            get { return World.Chunks[Index.X, Index.Y + 1, Index.Z]; }
        }

        internal Chunk Bottom
        {
            get { return World.Chunks[Index.X, Index.Y - 1, Index.Z]; }
        }

        #endregion N S E W NE NW SE SW Top Bottom Neighbours accessors

        public override string ToString()
        {
            return ("Chunk at index " + Index);
        }

        internal static int GetOffset(int x, int y, int z)
        {
            return Math.Abs(x) * FLATTEN_OFFSET + Math.Abs(z) * SIZE.Y + y;
        }

        internal static Vector3i GetLocalPosition(int offset)
        {
            var result = new Vector3i
            {
                X = offset / FLATTEN_OFFSET,
                Y = (offset % FLATTEN_OFFSET) % SIZE.Y,
                Z = (offset % FLATTEN_OFFSET) / SIZE.Y,
            };
            return result;
            // x = 14, y = 41, z = 8 / 14889
        }

        //Generates terrain for chunk with noises
        internal void DoGenerate()
        {
            World.Generator.Generate(this);
            World.Generator.ClearAfterGenerating();
            SpawnInitialCharacters();

            State = ChunkState.Generated;
        }

        /// <summary>
        /// Generates initial NPCs on chunk
        /// </summary>
        private void SpawnInitialCharacters()
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnNPCs();
            }
        }

        private Vector3 FindSpawnPosition(out bool isFound)
        {
            isFound = true;
            var randomBlockPosition = new Vector3i(RNG.GetInt(MAX_VECTOR.X), RNG.GetInt(MAX_VECTOR.Y), RNG.GetInt(MAX_VECTOR.Z));

            bool isEligiblePosition = CheckIfPositionEligibleForSpawn(randomBlockPosition);

            if (!isEligiblePosition)
            {
                if (PossibleSpawnPoints.Count > 0)
                {
                    return GetWorldPosition(PossibleSpawnPoints[RNG.GetInt(PossibleSpawnPoints.Count)]).AsVector3();
                }

                var Y = randomBlockPosition.Y;
                while (randomBlockPosition.Y < SIZE.Y)
                {
                    randomBlockPosition.Y = randomBlockPosition.Y + 1;

                    if (CheckIfPositionEligibleForSpawn(randomBlockPosition))
                    {
                        PossibleSpawnPoints.Add(randomBlockPosition);
                        return GetWorldPosition(randomBlockPosition).AsVector3();
                    }
                }
                randomBlockPosition.Y = Y;
                while (randomBlockPosition.Y > 0)
                {
                    randomBlockPosition.Y = randomBlockPosition.Y - 1;

                    if (CheckIfPositionEligibleForSpawn(randomBlockPosition))
                    {
                        PossibleSpawnPoints.Add(randomBlockPosition);
                        return GetWorldPosition(randomBlockPosition).AsVector3();
                    }
                }
                isFound = false;
                return GetWorldPosition(randomBlockPosition).AsVector3();
            }

            if (!PossibleSpawnPoints.Contains(randomBlockPosition))
            {
                // NOTE: I also need some way to detect if chunk can't spawn anything,
                // to not waste resources continually trying to find spawn points
                PossibleSpawnPoints.Add(randomBlockPosition);
            }

            return GetWorldPosition(randomBlockPosition).AsVector3();
        }

        private bool CheckIfPositionEligibleForSpawn(Vector3i position)
        {
            BlockType block = GetBlock(position);

            if (block.IsSolid)
            {
                return false;
            }

            var downBlock = RelativeGetBlock(position.X, position.Y - 1, position.Z);
            var upBlock1 = RelativeGetBlock(position.X, position.Y + 1, position.Z);
            var upBlock2 = RelativeGetBlock(position.X, position.Y + 2, position.Z);

            if (upBlock1.IsSolid || upBlock2.IsSolid || !downBlock.IsSolid)
            {
                return false;
            }

            return true;
        }

        internal void SpawnNPCs()
        {
            if (State != ChunkState.Generated)
            {
                return;
            }

            foreach (KeyValuePair<string, float> spawnInfo in SpawnData)
            {
                if (NPCEntityTemplate.NPCCurrentCount <= NPCEntityTemplate.MAX_NPC_COUNT && RNG.CheckProbabilityOnce(spawnInfo.Value))
                {
                    bool isFound;
                    Vector3 position = FindSpawnPosition(out isFound);

                    if (isFound)
                    {
                        NPCEntityTemplate.Instance.BuildEntity(World.EntityWorld, spawnInfo.Key, position);
                    }
                }
            }
        }

        //Prepares vertices and indices of the chunk to draw.
        internal void DoDraw()
        {
            State = ChunkState.Verticing;
            World.VertexBuildChunkProcessor.ProcessChunk(this);
            World.LightingChunkProcessor.ProcessChunk(this);
            State = ChunkState.Ready;
        }

        internal void RedrawWithNeighbours(int localX, int localY, int localZ)
        {
            DoDraw();

            // Most probable place of "block or its side not drawing" bug
            if (localX == 0 || localX == MAX_VECTOR.X)
            {
                if (E != null)
                {
                    E.DoDraw();
                }

                if (W != null)
                {
                    W.DoDraw();
                }
            }

            if (localZ == 0 || localZ == MAX_VECTOR.Z)
            {
                if (S != null)
                {
                    S.DoDraw();
                }

                if (N != null)
                {
                    N.DoDraw();
                }
            }

            if (localY == 0 || localY == MAX_VECTOR.Y)
            {
                if (Bottom != null)
                {
                    Bottom.DoDraw();
                }

                if (Top != null)
                {
                    Top.DoDraw();
                }
            }
        }

        internal void RedrawWithNeighbours()
        {
            DoDraw();

            if (E != null)
            {
                E.DoDraw();
            }

            if (W != null)
            {
                W.DoDraw();
            }

            if (S != null)
            {
                S.DoDraw();
            }

            if (N != null)
            {
                N.DoDraw();
            }

            if (Bottom != null)
            {
                Bottom.DoDraw();
            }

            if (Top != null)
            {
                Top.DoDraw();
            }
        }

        internal bool AreStraightNeighbourChunksFilled()
        {
            return N != null && S != null && E != null && W != null;
        }

        internal bool HasFinishedGenerating()
        {
            return State >= ChunkState.Generated;
        }

        internal VertexBuffer GetOpaqueVertexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].VertexBuffer;
        }

        internal void SetOpaqueVertexBuffer(int atlasIndex, VertexBuffer vertexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].VertexBuffer = vertexBuffer;
        }

        internal IndexBuffer GetOpaqueIndexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].IndexBuffer;
        }

        internal void SetOpaqueIndexBuffer(int atlasIndex, IndexBuffer indexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].IndexBuffer = indexBuffer;
        }

        internal VertexBuffer GetCustomVertexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].CustomVertexBuffer;
        }

        internal void SetCustomVertexBuffer(int atlasIndex, VertexBuffer vertexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].CustomVertexBuffer = vertexBuffer;
        }

        internal IndexBuffer GetCustomIndexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].CustomIndexBuffer;
        }

        internal void SetCustomIndexBuffer(int atlasIndex, IndexBuffer indexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].CustomIndexBuffer = indexBuffer;
        }

        internal VertexBuffer GetTransparentVertexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].TransparentVertexBuffer;
        }

        internal void SetTransparentVertexBuffer(int atlasIndex, VertexBuffer vertexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].TransparentVertexBuffer = vertexBuffer;
        }

        internal IndexBuffer GetTransparentIndexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].TransparentIndexBuffer;
        }

        internal void SetTransparentIndexBuffer(int atlasIndex, IndexBuffer indexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].TransparentIndexBuffer = indexBuffer;
        }

        internal VertexBuffer GetAnimatedVertexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].AnimatedVertexBuffer;
        }

        internal void SetAnimatedVertexBuffer(int atlasIndex, VertexBuffer vertexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].AnimatedVertexBuffer = vertexBuffer;
        }

        internal IndexBuffer GetAnimatedIndexBuffer(int atlasIndex)
        {
            return ChunkBlocksRepresentation[atlasIndex].AnimatedIndexBuffer;
        }

        internal void SetAnimatedIndexBuffer(int atlasIndex, IndexBuffer indexBuffer)
        {
            ChunkBlocksRepresentation[atlasIndex].AnimatedIndexBuffer = indexBuffer;
        }

        internal void SetDefaultBlock(int x, int y, int z)
        {
            SetBlock(x, y, z, BlockTypeHelper.AIR_BLOCK_TYPE);
        }

        internal static Vector3i GetChunkIndex(Vector3i position)
        {
            int x = position.X;
            int y = position.Y;
            int z = position.Z;

            int newX = x / SIZE.X;
            int newY = y / SIZE.Y;
            int newZ = z / SIZE.Z;

            int xMod = x % SIZE.X;
            int yMod = y % SIZE.Y;
            int zMod = z % SIZE.Z;

            if (x < 0 && xMod != 0)
                newX -= 1;
            if (y < 0 && yMod != 0)
                newY -= 1;
            if (z < 0 && zMod != 0)
                newZ -= 1;

            return new Vector3i(newX, newY, newZ);
        }

        internal static Vector3i GetChunkIndex(int x, int y, int z)
        {
            return GetChunkIndex(new Vector3i(x, y, z));
        }

        internal Vector3i GetWorldPosition(int x, int y, int z)
        {
            return Position + new Vector3i(x, y, z);
        }

        internal Vector3i GetWorldPosition(Vector3i position)
        {
            return GetWorldPosition(position.X, position.Y, position.Z);
        }

        internal static Vector3i GetLocalPosition(Vector3i worldPosition)
        {
            return ChunkHelper.GetLocalPosition(worldPosition);
        }

        internal void SetGeneratedState()
        {
            State = ChunkState.Generated;
        }

        internal void SetSpawnData(CompiledRule compiledRule)
        {
            SpawnData[compiledRule.CharacterName] = compiledRule.SpawnRate;
        }

        public void Update100(GameTime gameTime)
        {
            for (int index = 0; index < BlocksUpdatedNextCycle.Count; index++)
            {
                int i = BlocksUpdatedNextCycle[index];
                Blocks[i].Update100(gameTime, World, GetWorldPosition(GetLocalPosition(i)));
            }

            BlocksUpdatedNextCycle.Clear();
        }

        public void Dispose()
        {
            if (!IsDisposing)
            {
                IsDisposing = true;

                try
                {
                    foreach (var representation in ChunkBlocksRepresentation)
                    {
                        representation.Value.Dispose();
                    }
                }
                catch (NullReferenceException)
                {
                }

                try
                {
                    World.BlockArrayPool.Release(Blocks);
                }
                catch (ObjectDisposedException)
                {
                }

                ChunkBlocksRepresentation = null;

                //                _lightBrightness = null;
                //                _rLight = null;
                //                _gLight = null;
                //                _bLight = null;

                LightMap = null;

                foreach (var textureAtlas in World.CompiledGameBundle.TextureAtlases)
                {
                    SetVertexCount(textureAtlas.Key, 0);
                    SetCustomVertexCount(textureAtlas.Key, 0);
                }
            }
        }

        ~Chunk()
        {
            // Recommended as ManagedDispose + NativeDispose, but works here.
            Dispose();
        }
    }
}