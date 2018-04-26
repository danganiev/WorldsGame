using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.View.Blocks;
using Plane = WorldsGame.Saving.Plane;

namespace WorldsGame.Playing.DataClasses
{
    public enum CompiledBlockType
    {
        Cube = 1,
        CustomBlock = 2,
        Liquid = 3
    }

    // All Compiled* objects should have reference to their CompiledGameBundle
    [Serializable]
    public class CompiledBlock
    {
        public static readonly Vector3 BLOCK_NATURAL_NUM_SPACE_DIFF = new Vector3(16, 16, 16);

        public const float BLOCK_HEIGHT = 1f;
        public const string WAIT_CUBE = "System__WaitBlock";
        public const string WHITE_CUBE = "System__WhiteCube";
        public const string AIR_CUBE = "System__Air";

        public bool IsDestroyable { get; set; }

        public bool IsSystem { get; set; }

        [NonSerialized]
        private CompiledGameBundle _gameBundle;

        public CompiledGameBundle GameBundle
        {
            get { return _gameBundle; }
            set { _gameBundle = value; }
        }

        // Should be unique per bundle
        public string Name { get; set; }

        public Dictionary<int, string> SideTextures { get; set; }

        [NonSerialized]
        private Dictionary<CubeFaceDirection, BlockPart> _blockParts = new Dictionary<CubeFaceDirection, BlockPart>();

        public Dictionary<CubeFaceDirection, BlockPart> BlockParts
        {
            get { return _blockParts; }
        }

        public List<CustomEntityPart> Parts { get; set; }

        public CompiledBlockType BlockType { get; set; }

        public short Health { get; set; }

        public List<SpawnedItemRule> ItemDropRules { get; set; }

        public bool IsCubical { get { return BlockType == CompiledBlockType.Cube; } }

        public bool IsLiquid { get; set; }

        public bool IsTransparent { get; set; }

        public bool IsAnimated { get; set; }

        // For serialization only!
        public CompiledBlock()
        {
        }

        public CompiledBlock(CompiledGameBundle gameBundle, Block block)
        {
            IsDestroyable = !block.IsUnbreakable;
            IsSystem = false;

            GameBundle = gameBundle;

            BlockType = block.IsFullBlock ? CompiledBlockType.Cube : CompiledBlockType.CustomBlock;
            if (block.IsLiquid)
            {
                BlockType = CompiledBlockType.Liquid;
            }

            Name = block.Name;
            Health = block.Health;
            IsLiquid = block.IsLiquid;
            IsTransparent = block.IsTransparent;
            IsAnimated = block.IsAnimated;

            ItemDropRules = new List<SpawnedItemRule>();

            foreach (SpawnedItemRule rule in block.ItemDropRules)
            {
                rule.IconColors = null;
                ItemDropRules.Add(rule);
            }

            if (BlockType == CompiledBlockType.Cube)
            {
                PrepareCubeSideTextures(block);
                InitializeCube();
            }
            else
            {
                InitializeCustomBlock(block);
            }
        }

        // This constructor is used for system blocks mostly.
        public CompiledBlock(CompiledGameBundle gameBundle, string name, string textureName, bool isDestroyable = true,
            bool isSystem = false)
        {
            IsDestroyable = isDestroyable;
            GameBundle = gameBundle;
            Name = name;
            SideTextures = new Dictionary<int, string>();
            FillWithSameTextures(textureName);
            IsSystem = isSystem;

            BlockType = CompiledBlockType.Cube;

            InitializeCube();
        }

        private void InitializeCustomBlock(Block block)
        {
            Parts = new List<CustomEntityPart>();

            foreach (Cuboid cuboid in block.Cuboids)
            {
                foreach (Plane plane in cuboid.Planes)
                {
                    if (plane.TextureName == "")
                    {
                        continue;
                    }

                    CompiledTexture texture = GameBundle.GetTexture(plane.TextureName);
                    int textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                    Vector3[] vertices = cuboid.GetTransformedVerticeList(plane/*, BLOCK_NATURAL_NUM_SPACE_DIFF*/).ToArray();
                    Vector2[] uvMappings = BlockPart.GetCustomUVMappingList(texture, textureAtlasSizeInPixels);
                    var customPart = new CustomEntityPart(vertices, uvMappings);

                    Parts.Add(customPart);
                }
            }
        }

        private void PrepareCubeSideTextures(Block block)
        {
            SideTextures = new Dictionary<int, string>();

            for (int i = 0; i < block.Cuboids[0].Planes.Count; i++)
            {
                Plane plane = block.Cuboids[0].Planes[i];
                SideTextures[i] = plane.TextureName;
            }
        }

        private void FillWithSameTextures(string textureName)
        {
            // This foreach would be slow in general, but it's in the rarely used method so let it be
            foreach (CubeFaceDirection face in (CubeFaceDirection[])Enum.GetValues(typeof(CubeFaceDirection)))
            {
                int faceInt = (int)face;
                if (!SideTextures.ContainsKey(faceInt))
                {
                    SideTextures[faceInt] = textureName;
                }
            }
        }

        internal void InitializeCube()
        {
            if (_blockParts == null)
            {
                _blockParts = new Dictionary<CubeFaceDirection, BlockPart>();
            }

            if (BlockType == CompiledBlockType.Cube)
            {
                CompiledTexture texture = GameBundle.GetTexture(SideTextures[(int)CubeFaceDirection.Right]);
                int textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                BlockParts.Add(
                    CubeFaceDirection.Right,
                    new BlockPart(
                        new[]
                        {
                            new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, BLOCK_HEIGHT),
                            new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, 0),
                            new Vector3(BLOCK_HEIGHT, 0, BLOCK_HEIGHT),
                            new Vector3(BLOCK_HEIGHT, 0, 0)
                        },
                        new byte[] { 0, 1, 2, 5 },
                        new short[] { 0, 1, 2, 2, 1, 3 },
                        texture,
                        BlockPart.GetCubeUVMappingList(texture, textureAtlasSizeInPixels, CubeFaceDirection.Right)
                        )
                );
                texture = GameBundle.GetTexture(SideTextures[(int)CubeFaceDirection.Left]);
                textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                BlockParts.Add(
                    CubeFaceDirection.Left,
                    new BlockPart(
                    //                        new[] { new Vector3(0, BLOCK_HEIGHT, BLOCK_HEIGHT), new Vector3(0, BLOCK_HEIGHT, 0), new Vector3(0, 0, BLOCK_HEIGHT), new Vector3(0, 0, 0) },
                        new[] { new Vector3(0, BLOCK_HEIGHT, 0), new Vector3(0, BLOCK_HEIGHT, BLOCK_HEIGHT), new Vector3(0, 0, 0), new Vector3(0, 0, BLOCK_HEIGHT) },
                        new byte[] { 0, 1, 5, 2 },
                        new short[] { 0, 1, 3, 0, 3, 2 },
                        texture,
                        BlockPart.GetCubeUVMappingList(texture, textureAtlasSizeInPixels, CubeFaceDirection.Left)
                        )
                    );

                texture = GameBundle.GetTexture(SideTextures[(int)CubeFaceDirection.Up]);
                textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                BlockParts.Add(
                    CubeFaceDirection.Up,
                    new BlockPart(
                        new[] { new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, BLOCK_HEIGHT), new Vector3(0, BLOCK_HEIGHT, BLOCK_HEIGHT), new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, 0), new Vector3(0, BLOCK_HEIGHT, 0) },
                        new byte[] { 4, 5, 1, 3 },
                        new short[] { 3, 2, 0, 3, 0, 1 },
                        texture,
                        BlockPart.GetCubeUVMappingList(texture, textureAtlasSizeInPixels, CubeFaceDirection.Up)
                        )
                    );

                texture = GameBundle.GetTexture(SideTextures[(int)CubeFaceDirection.Down]);
                textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                BlockParts.Add(
                    CubeFaceDirection.Down,
                    new BlockPart(
                    //                        new[] { new Vector3(BLOCK_HEIGHT, 0, BLOCK_HEIGHT), new Vector3(0, 0, BLOCK_HEIGHT), new Vector3(BLOCK_HEIGHT, 0, 0), new Vector3(0, 0, 0) },
                        new[] { new Vector3(0, 0, BLOCK_HEIGHT), new Vector3(BLOCK_HEIGHT, 0, BLOCK_HEIGHT), new Vector3(0, 0, 0), new Vector3(BLOCK_HEIGHT, 0, 0) },
                        new byte[] { 0, 2, 4, 5 },
                        new short[] { 0, 2, 1, 1, 2, 3 },
                        texture,
                        BlockPart.GetCubeUVMappingList(texture, textureAtlasSizeInPixels, CubeFaceDirection.Down)
                        )
                    );

                texture = GameBundle.GetTexture(SideTextures[(int)CubeFaceDirection.Back]);
                textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                BlockParts.Add(
                    CubeFaceDirection.Back,
                    new BlockPart(
                        new[] { new Vector3(0, BLOCK_HEIGHT, BLOCK_HEIGHT), new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, BLOCK_HEIGHT), new Vector3(0, 0, BLOCK_HEIGHT), new Vector3(BLOCK_HEIGHT, 0, BLOCK_HEIGHT) },
                        new byte[] { 0, 1, 5, 2 },
                        new short[] { 0, 1, 3, 0, 3, 2 },
                        texture,
                        BlockPart.GetCubeUVMappingList(texture, textureAtlasSizeInPixels, CubeFaceDirection.Back)
                        )
                    );

                texture = GameBundle.GetTexture(SideTextures[(int)CubeFaceDirection.Forward]);
                textureAtlasSizeInPixels = GameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                BlockParts.Add(
                    CubeFaceDirection.Forward,
                    new BlockPart(
                                            new[] { new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, 0), new Vector3(0, BLOCK_HEIGHT, 0), new Vector3(BLOCK_HEIGHT, 0, 0), new Vector3(0, 0, 0) },
                    //                        new[] { new Vector3(0, BLOCK_HEIGHT, 0), new Vector3(BLOCK_HEIGHT, BLOCK_HEIGHT, 0), new Vector3(0, 0, 0), new Vector3(BLOCK_HEIGHT, 0, 0) },
                        new byte[] { 0, 1, 2, 5 },
                        new short[] { 0, 1, 2, 2, 1, 3 },
                        texture,
                        BlockPart.GetCubeUVMappingList(texture, textureAtlasSizeInPixels, CubeFaceDirection.Forward)
                        )
                    );
            }
        }
    }
}