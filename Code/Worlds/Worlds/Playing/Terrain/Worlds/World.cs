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
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Gamestates;
using WorldsGame.Models;
using WorldsGame.Models.Terrain;
using WorldsGame.Network;
using WorldsGame.Network.Manager;
using WorldsGame.Network.Message;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Effects;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.Terrain.Light;
using WorldsGame.Playing.Terrain.Liquid;
using WorldsGame.Playing.Terrain.Worlds;
using WorldsGame.Playing.Terrain.Worlds.Time;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.World;
using WorldsGame.Sound;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Terrain.Chunks.Processors;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.View.Processors;
using WorldsLib;
using Effect = WorldsGame.Saving.DataClasses.Effect;
using Texture = Microsoft.Xna.Framework.Graphics.Texture;

namespace WorldsGame.Terrain
{
    public class World : IDisposable
    {
        //Constants

        // TODO: Learn what this constant is about. It seems like it is about dividing by chunks for overcoming float division errors
        internal const int WORLD_DIVIDER = 4096;

        //        internal static readonly Vector4 NIGHTCOLOR = Color.Navy.ToVector4();
        //        internal static readonly Vector4 SUNCOLOR = Color.White.ToVector4();
        //        internal static readonly Vector4 HORIZONCOLOR = Color.White.ToVector4();
        //
        //        internal static readonly Vector4 EVENINGTINT = Color.Red.ToVector4();
        //        internal static readonly Vector4 MORNINGTINT = Color.Gold.ToVector4();

        // The original chunk generation radius
        internal const byte CHUNK_INITIAL_GENERATION_RANGE = 1;

        // View distance
        internal static byte ChunkGenerationRange;

        // View distance Y
        internal static readonly byte CHUNK_GENERATION_RANGE_Y = 1;

        internal const int POOL_SIZE = 2000;

        internal static readonly Vector3i ORIGIN = new Vector3i(0, 0, 0);

        //Fields
        private BaseTimeUpdater _timeUpdater;

        private readonly WorldBlockOperator _worldBlockOperator;
        private readonly WorldObjectOperator _worldObjectOperator;

        private IFirstChunksInitializer _firstChunksInitializer;

        internal ChunkGenerator Generator { get; private set; }

        internal VertexBuildChunkProcessor VertexBuildChunkProcessor { get; private set; }

        internal LightingChunkProcessor LightingChunkProcessor { get; private set; }

        internal ChunkManager Chunks { get; private set; }

        internal ChunkRegionManager ChunkRegions { get; private set; }

        internal bool IsWireframed { get; private set; }

        internal GraphicsDevice Graphics { get; private set; }

        internal CompiledGameBundle CompiledGameBundle { get; private set; }

        internal BaseTimeUpdater TimeUpdater
        {
            get { return _timeUpdater ?? (_timeUpdater = new SimpleTimeUpdater()); }
            set { _timeUpdater = value; }
        }

        internal bool IsDayMode
        {
            get { return TimeUpdater.IsDayMode; }
            set { TimeUpdater.IsDayMode = value; }
        }

        internal bool IsNightMode
        {
            get { return TimeUpdater.IsNightMode; }
            set { TimeUpdater.IsNightMode = value; }
        }

        internal float TimeOfDay
        {
            get { return TimeUpdater.TimeOfDay; }
            set { TimeUpdater.TimeOfDay = value; }
        }

        internal Vector3i IndexMultiplier { get; set; }

        internal Pool<PooledBlockTypeArray> BlockArrayPool { get; private set; }

        internal WorldType WorldType { get; private set; }

        internal Player ClientPlayer { get { return PlayerManager.ClientPlayer; } }

        internal EmptyObjectCreationWorldHelper ObjectCreationHelper { get; set; }

        // For object editor usage only
        internal GameObject GameObject
        {
            get { return ObjectCreationHelper.GameObject; }
            set { ObjectCreationHelper.GameObject = value; }
        }

        internal HashSet<Vector3i> FilledCoords { get { return ObjectCreationHelper.FilledCoords; } }

        internal string WorldSettingsName { get { return CompiledGameBundle.WorldSettingsName; } }

        private int _seed;

        internal string SeedBase
        {
            set
            {
                _seed = value == "" ? DateTime.UtcNow.GetHashCode() : value.GetHashCode();
            }
        }

        internal WorldSave WorldSave { get; private set; }

        internal bool IsNetworkWorld { get { return WorldType == WorldType.NetworkWorld; } }

        internal bool IsServerWorld { get { return WorldType == WorldType.ServerWorld; } }

        private INetworkManager _networkManager;
        private EffectTargetDetector _effectTargetDetector;

        internal INetworkManager NetworkManager
        {
            get
            {
                if (_networkManager == null)
                {
                    _networkManager = new EmptyNetworkManager();
                }

                return _networkManager;
            }
            set { _networkManager = value; }
        }

        #region Multiplayer stuff    
    
        internal ClientMessageProcessor ClientMessageProcessor { get; set; }

        internal ServerMessageProcessor ServerMessageProcessor { get; set; }

        internal IPlayerManager PlayerManager { get; set; }

        internal ServerWorldNetworkProcessor ServerWorldNetworkProcessor { get; set; }

        internal ClientWorldNetworkProcessor ClientWorldNetworkProcessor { get; set; }

        #endregion

        internal AudioManager AudioManager { get; set; }

        internal IRecipeManager RecipeManager { get; set; }

        internal EntityWorld EntityWorld { get; set; }

        internal Queue<LightAdditionNode> LightNodeBFSQueue { get; set; }

        internal Queue<LightRemovalNode> LightRemovalNodeBFSQueue { get; set; }

        internal Queue<LightAdditionNode> SunlightNodeBFSQueue { get; set; }

        internal LiquidStorage LiquidStorage { get; set; }

        internal int SunlitHeight { get { return CompiledGameBundle.SunlitHeight; } }

        public Color CurrentAtmosphereColor { get { return TimeUpdater.CurrentAtmosphereColor; } }

        internal World(GraphicsDevice graphicsDevice, CompiledGameBundle compiledGameBundle,
            WorldType worldType = WorldType.LocalWorld, string seedBase = "", WorldSave worldSave = null)
        {
            Messenger.Invoke("LoadingMessageChange", "Started world initialization");

            Graphics = graphicsDevice;
            CompiledGameBundle = compiledGameBundle;
            WorldType = worldType;
            SeedBase = seedBase;
            WorldSave = worldSave;

            LiquidPool.NextPoolId = 0;

            if (worldSave != null)
            {
                _seed = worldSave.Seed;
                LiquidPool.NextPoolId = worldSave.NextLiquidPoolId;
            }

            CompiledGameBundle.SeedNoises(_seed);

            ChunkGenerationRange = ViewDistanceEnum.SettingsNames[SettingsManager.Settings.ViewDistance];

            TimeOfDay = 12;
            Chunks = new ChunkManager(this);
            ChunkRegions = new ChunkRegionManager(this, CompiledGameBundle);
            IndexMultiplier = new Vector3i(0, 0, 0);

            _worldBlockOperator = new WorldBlockOperator(this);
            _worldObjectOperator = new WorldObjectOperator(this);

            ObjectCreationHelper = WorldType == WorldType.ObjectCreationWorld
                                       ? new ObjectCreationWorldHelper(this)
                                       : new EmptyObjectCreationWorldHelper();

            BlockArrayPool = new Pool<PooledBlockTypeArray>(POOL_SIZE, pool => new PooledBlockTypeArray(pool), LoadingMode.Lazy, AccessMode.LIFO);

            Generator = ChunkGenerator.GetGenerator(compiledGameBundle, worldType);
            VertexBuildChunkProcessor = new VertexBuildChunkProcessor(Graphics, this);

            LightingChunkProcessor = new LightingChunkProcessor();

            Inventory.CompiledGameBundle = CompiledGameBundle;

            LightNodeBFSQueue = new Queue<LightAdditionNode>();
            LightRemovalNodeBFSQueue = new Queue<LightRemovalNode>();
            SunlightNodeBFSQueue = new Queue<LightAdditionNode>();

            BlockTypeHelper.Initialize(compiledGameBundle);
            Messenger.Invoke("LoadingMessageChange", "Loading characters...");
            CharacterHelper.Initialize(compiledGameBundle);
            CharacterAttributeHelper.Initialize(compiledGameBundle);
            ItemHelper.Initialize(compiledGameBundle);
            RecipeHelper.Initialize(compiledGameBundle);
            TimeUpdater.Initialize(compiledGameBundle);
        }

        internal void Initialize()
        {
            InitializePlayerManager();

            if (IsNetworkWorld)
            {
                InitializeNetworkWorld();
            }
            else if (IsServerWorld)
            {
                InitializeServerWorld();
            }
            else
            {
                InitializeLocalWorld();
            }

            InitializeRecipeManager();

            InitializeEffectTargetDetector();
        }

        private void InitializePlayerManager()
        {
            // Network world initializes manager in NetworkLoadingState
            if (!IsNetworkWorld && !IsServerWorld)
            {
                PlayerManager = new SinglePlayerManager();
            }

            PlayerManager.Initialize(this);
        }

        private void InitializeRecipeManager()
        {
            if (!IsNetworkWorld && !IsServerWorld)
            {
                RecipeManager = new LocalRecipeManager();
            }
            else
            {
                RecipeManager = new NetworkRecipeManager();
            }
        }

        private void InitializeEffectTargetDetector()
        {
            _effectTargetDetector = new EffectTargetDetector();
        }

        private void InitializeLocalWorld()
        {
            SpawnSinglePlayer(WorldSave);
            InitializeChunks(WorldSave);
        }

        private void SpawnSinglePlayer(WorldSave worldSave)
        {
            if (worldSave != null)
            {
                var spawnParams = new SpawnPlayerParams
                {
                    Position = worldSave.PlayerPosition,
                    CameraUpDownRotation = worldSave.CameraUpDownRotation,
                    CameraLeftRightRotation = worldSave.CameraLeftRightRotation,
                    Inventory = worldSave.PlayerInventory,
                    InventorySelectedSlot = worldSave.PlayerInventorySelectedSlot
                };

                PlayerManager.SpawnMainPlayer(spawnParams);
            }
            else
            {
                PlayerManager.SpawnMainPlayer(new SpawnPlayerParams());
            }
        }

        private void InitializeNetworkWorld()
        {
            InitializeClientWorldNetworkProcessor();
        }

        private void InitializeClientWorldNetworkProcessor()
        {
            ClientWorldNetworkProcessor = new ClientWorldNetworkProcessor(this, (ClientNetworkManager)NetworkManager);
            ClientMessageProcessor.OnBlockUpdate += ClientWorldNetworkProcessor.OnBlockUpdate;
        }

        private void InitializeServerWorld()
        {
            InitializeChunks(WorldSave);
            InitializeServerWorldNetworkProcessor();
        }

        private void InitializeServerWorldNetworkProcessor()
        {
            ServerWorldNetworkProcessor = new ServerWorldNetworkProcessor(this, (ServerNetworkManager)NetworkManager);
            Messenger.On<Vector3i, BlockType>("WorldBlockChange", ServerWorldNetworkProcessor.OnWorldBlockChange);
        }

        internal void InitializeChunks(WorldSave worldSave)
        {
            if (IsNetworkWorld)
            {
                _firstChunksInitializer = new NetworkFirstChunkInitializer(this, ORIGIN, ClientMessageProcessor);
            }
            else
            {
                _firstChunksInitializer = new FirstChunksInitializer(
                    this, worldSave != null ? Chunk.GetChunkIndex(new Vector3i(worldSave.PlayerPosition)) : ORIGIN);
            }

            _firstChunksInitializer.Initialize();
        }

        internal void Update(GameTime gameTime)
        {
            PlayerManager.Update(gameTime);
            TimeUpdater.Update(gameTime);

            UpdateLight();
        }

        internal void Update1000(GameTime gameTime)
        {
            SpawnNPCs(gameTime);
            UpdateChunks(gameTime);
            TimeUpdater.Update1000(gameTime);
        }

        // Server-only
        internal void UpdateStep(GameTime gameTime)
        {
            PlayerManager.UpdateStep(gameTime);
        }

        private void UpdateChunks(GameTime gameTime)
        {
            foreach (KeyValuePair<ulong, Chunk> chunk in Chunks)
            {
                chunk.Value.Update100(gameTime);
            }
        }

        private void UpdateLight()
        {
            int i = 0;

            while (LightRemovalNodeBFSQueue.Count > 0 && i < 500)
            {
                LightRemovalNode node = LightRemovalNodeBFSQueue.Dequeue();

                int x = node.Index.X;
                int y = node.Index.Y;
                int z = node.Index.Z;

                Chunk chunk = node.Chunk;

                int luminosity, r, g, b;
                chunk.GetLightData(x, y, z, out luminosity, out r, out g, out b);

                RemoveNeighbourLight(x - 1, y, z, luminosity, r, g, b, chunk);
                RemoveNeighbourLight(x + 1, y, z, luminosity, r, g, b, chunk);
                RemoveNeighbourLight(x, y - 1, z, luminosity, r, g, b, chunk);
                RemoveNeighbourLight(x, y + 1, z, luminosity, r, g, b, chunk);
                RemoveNeighbourLight(x, y, z - 1, luminosity, r, g, b, chunk);
                RemoveNeighbourLight(x, y, z + 1, luminosity, r, g, b, chunk);

                i++;
            }

            while (LightNodeBFSQueue.Count > 0 && i < 500)
            {
                LightAdditionNode additionNode = LightNodeBFSQueue.Dequeue();

                int x = additionNode.Index.X;
                int y = additionNode.Index.Y;
                int z = additionNode.Index.Z;

                Chunk chunk = additionNode.Chunk;

                int luminosity, r, g, b;
                chunk.GetLightData(x, y, z, out luminosity, out r, out g, out b);

                AddNeighbourLight(x - 1, y, z, luminosity, r, g, b, chunk);
                AddNeighbourLight(x + 1, y, z, luminosity, r, g, b, chunk);
                AddNeighbourLight(x, y - 1, z, luminosity, r, g, b, chunk);
                AddNeighbourLight(x, y + 1, z, luminosity, r, g, b, chunk);
                AddNeighbourLight(x, y, z - 1, luminosity, r, g, b, chunk);
                AddNeighbourLight(x, y, z + 1, luminosity, r, g, b, chunk);

                i++;
            }

            while (SunlightNodeBFSQueue.Count > 0 && i < 500)
            {
                LightAdditionNode additionNode = LightNodeBFSQueue.Dequeue();

                int x = additionNode.Index.X;
                int y = additionNode.Index.Y;
                int z = additionNode.Index.Z;

                Chunk chunk = additionNode.Chunk;

                int sunlight = chunk.GetSunlight(x, y, z);

                AddNeighbourSunlight(x - 1, y, z, y, sunlight, chunk);
                AddNeighbourSunlight(x + 1, y, z, y, sunlight, chunk);
                AddNeighbourSunlight(x, y - 1, z, y, sunlight, chunk);
                AddNeighbourSunlight(x, y + 1, z, y, sunlight, chunk);
                AddNeighbourSunlight(x, y, z - 1, y, sunlight, chunk);
                AddNeighbourSunlight(x, y, z + 1, y, sunlight, chunk);

                i++;
            }
        }

        private void AddNeighbourLight(int x, int y, int z, int luminosity, int r, int g, int b, Chunk chunk)
        {
            int otherLuminosity, otherR, otherG, otherB;

            chunk.RelativeGetLightData(
                x, y, z, out otherLuminosity, out otherR, out otherG, out otherB);

            if (otherLuminosity + 2 <= luminosity)
            {
                BlockType block = chunk.RelativeGetBlock(x, y, z);

                chunk.RelativeSetLightData(x, y, z, luminosity - 1, r, g, b);

                Vector3i worldPosition = chunk.GetWorldPosition(x, y, z);

                if (block.IsTransparent)
                {
                    LightNodeBFSQueue.Enqueue(new LightAdditionNode(chunk.GetWorldPosition(x, y, z), GetChunk(worldPosition)));
                }
            }
        }

        private void RemoveNeighbourLight(int x, int y, int z, int luminosity, int r, int g, int b, Chunk chunk)
        {
            int otherLuminosity, otherR, otherG, otherB;

            chunk.RelativeGetLightData(
                x, y, z, out otherLuminosity, out otherR, out otherG, out otherB);

            if (otherLuminosity != 0 && otherLuminosity < luminosity)
            {
                // BlockType block = chunk.RelativeGetBlock(x, y, z);

                // We would need values like otherR * 2 - r later here
                chunk.RelativeSetLightData(x, y, z, 0, 0, 0, 0);

                // Vector3i worldPosition = chunk.GetWorldPosition(x, y, z);

                // if (block.IsTransparent)
                // {
                //     LightNodeBFSQueue.Enqueue(new LightAdditionNode(chunk.GetWorldPosition(x, y, z), GetChunk(worldPosition)));
                // }
            }
            else
            {
                BlockType block = chunk.RelativeGetBlock(x, y, z);

                Vector3i worldPosition = chunk.GetWorldPosition(x, y, z);

                if (block.IsTransparent)
                {
                    LightNodeBFSQueue.Enqueue(
                        new LightAdditionNode(chunk.GetWorldPosition(x, y, z),
                            GetChunk(worldPosition)));
                }
            }
        }

        private void AddNeighbourSunlight(int x, int y, int z, int previousY, int sunlight, Chunk chunk)
        {
            int otherSunlight = chunk.RelativeGetSunlight(x, y, z);

            if (otherSunlight < sunlight)
            {
                BlockType block = chunk.RelativeGetBlock(x, y, z);

                chunk.RelativeSetSunlight(x, y, z, sunlight == LightAdditionNode.MAX_SUNLIGHT && y < previousY ? sunlight : sunlight - 1);

                Vector3i worldPosition = chunk.GetWorldPosition(x, y, z);

                if (block.IsTransparent)
                {
                    LightNodeBFSQueue.Enqueue(new LightAdditionNode(chunk.GetWorldPosition(x, y, z), GetChunk(worldPosition)));
                }
            }
        }

        internal void ToggleRasterMode()
        {
            IsWireframed = !IsWireframed;
        }

        internal Chunk GetChunk(Vector3i position)
        {
            Vector3i chunkIndex = Chunk.GetChunkIndex(position);

            return Chunks.Get(chunkIndex);
        }

        internal Chunk GetChunkByIndex(Vector3i chunkIndex)
        {
            return Chunks.Get(chunkIndex);
        }

        internal Chunk GetOrCreate(Vector3i chunkIndex)
        {
            Chunk chunk = Chunks.Get(chunkIndex);

            if (chunk == null)
            {
                chunk = new Chunk(this, chunkIndex);
                chunk.DoGenerate();
                Chunks.AddOrUpdate(chunkIndex, chunk);
            }

            return chunk;
        }

        // User-friendly get block. Tries to just work
        public BlockType GetBlock(Vector3 position)
        {
            return _worldBlockOperator.GetBlock(position);
        }

        internal BlockType GetBlock(Vector3i position)
        {
            return _worldBlockOperator.GetBlock(position.X, position.Y, position.Z);
        }

        internal int GetSunlight(Vector3i position)
        {
            return _worldBlockOperator.GetSunlight(position.X, position.Y, position.Z);
        }

        internal void SetSunlight(Vector3i position, int sunlight)
        {
            _worldBlockOperator.SetSunlight(
                 position.X, position.Y, position.Z, sunlight);
        }

        internal void GetLightData(Vector3i position, out int luminosity,
            out int r, out int g, out int b)
        {
            _worldBlockOperator.GetLightData(position.X, position.Y, position.Z, out luminosity, out r, out g, out b);
        }

        internal void SetLightData(Vector3i position, int luminosity, int r, int g, int b)
        {
            _worldBlockOperator.SetLightData(
                position.X, position.Y, position.Z, luminosity, r, g, b);
        }

        internal BlockType GetBlock(int x, int y, int z)
        {
            return _worldBlockOperator.GetBlock(x, y, z);
        }

        internal void SetBlock(Vector3i position, BlockType newBlock, bool suppressEvents = false)
        {
            _worldBlockOperator.SetBlockAndRedraw(position.X, position.Y, position.Z, newBlock);

            if (!suppressEvents)
            {
                Messenger.Invoke("WorldBlockChange", position, newBlock);
            }
        }

        internal void DestroyBlock(Vector3i position, bool suppressEvents = false)
        {
            BlockType block = GetBlock(position);

            if (block != BlockTypeHelper.AIR_BLOCK_TYPE)
            {
                foreach (SpawnedItemRule rule in block.ItemDropRules)
                {
                    SpawnItem(position, rule);
                }
            }

            SetBlock(position, BlockTypeHelper.AIR_BLOCK_TYPE, suppressEvents: suppressEvents);
        }

        private void SpawnItem(Vector3i position, SpawnedItemRule rule)
        {
            if (RandomNumberGenerator.Next(0, 100) <= rule.Probability)
            {
                int quantity = RandomNumberGenerator.GetInt(rule.MinQuantity, rule.MaxQuantity);

                ItemEntityTemplate.Instance.BuildEntity(EntityWorld, rule.Name, quantity, position.AsVector3());
            }
        }

        public void SetObject(CompiledGameObject gameObject, Vector3i position, Vector2 direction, int priority)
        {
            _worldObjectOperator.SetObject(gameObject, position, direction, priority);
        }

        internal Dictionary<Entity, List<Effect>> GetEffectTargets(CompiledEffect effect, Entity effectStarter)
        {
            return _effectTargetDetector.GetEffectTargets(effect, effectStarter);
        }

        internal void SpawnNPCs(GameTime gameTime)
        {
            foreach (KeyValuePair<ulong, Chunk> chunk in Chunks)
            {
                chunk.Value.SpawnNPCs();
            }
        }

        internal void Save()
        {
            if (WorldSave == null)
            {
                WorldSave = new WorldSave();
            }

            WorldSave.Guid = CompiledGameBundle.Guid;
            WorldSave.Name = CompiledGameBundle.Name;

            if (WorldType != WorldType.ServerWorld)
            {
                WorldSave.PlayerPosition = ClientPlayer.Position;
                WorldSave.CameraLeftRightRotation = ClientPlayer.LeftRightRotation;
                WorldSave.CameraUpDownRotation = ClientPlayer.UpDownRotation;
                WorldSave.PlayerInventory = ClientPlayer.Inventory.Items;
                WorldSave.PlayerInventorySelectedSlot = ClientPlayer.Inventory.SelectedSlot;
            }

            WorldSave.NextLiquidPoolId = LiquidPool.NextPoolId;

            WorldSave.Seed = _seed;

            WorldSave.Save();

            ChunkRegions.SaveEverything();
        }

        //        public void SpawnNPC(string name, Vector3 position)
        //        {
        //            throw new NotImplementedException();
        //        }

        public void Dispose()
        {
            PlayerManager.Dispose();
            Chunks.Dispose();
            Generator.Dispose();
            VertexBuildChunkProcessor.Dispose();

            BlockArrayPool.Dispose();

            ClientDispose();

            Messenger.Off<Vector3i, BlockType>("WorldBlockChange", null);
        }

        private void ClientDispose()
        {
            if (IsNetworkWorld)
            {
                ClientMessageProcessor.OnBlockUpdate -= ClientWorldNetworkProcessor.OnBlockUpdate;
            }
        }
    }

    internal enum WorldType
    {
        LocalWorld = 1,
        ObjectCreationWorld = 2,
        NetworkWorld = 3,
        ServerWorld = 4
    }
}