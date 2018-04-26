using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Terrain.Blocks.Behaviours;
using WorldsGame.Saving.DataClasses;
using WorldsGame.View.Blocks;
using WorldsLib;

namespace WorldsGame.Terrain.Blocks.Types
{
    public enum BlockTypeMatterState : byte
    {
        Solid = 1,
        Liquid = 2,
        Gas = 3
    }

    public class BlockType
    {
        internal string Name { get; private set; }

        public BlockTypeMatterState MatterState { get; private set; }

        public Dictionary<CubeFaceDirection, BlockPart> BlockParts { get; private set; }

        public List<CustomEntityPart> CustomBlockParts { get; private set; }

        public bool IsSolid { get { return MatterState == BlockTypeMatterState.Solid; } }

        public bool IsLiquid { get { return MatterState == BlockTypeMatterState.Liquid; } }

        // At least one traingle of the block is transparent
        public bool IsTransparent { get; private set; }

        // Uses at least 1 animated texture
        public bool IsAnimated { get; private set; }

        // The block can be destroyed by some way.
        public bool IsDestroyable { get; private set; }

        // System block, player cannot hold it or place it in any way.
        public bool IsSystem { get; private set; }

        // Cubical block fills the whole available zone, and only has 6 sides.
        public bool IsCubical { get; private set; }

        public short Health { get; private set; }

        public List<SpawnedItemRule> ItemDropRules { get; set; }

        public int Key { get { return BlockTypeHelper.InverseBlockTypeCache[Name]; } }

        public byte LightLevel { get; set; }

        public IBlockBehaviour BlockBehaviour { get; private set; }

        internal BlockType(string name, BlockTypeMatterState matterState = BlockTypeMatterState.Solid,
            Dictionary<CubeFaceDirection, BlockPart> blockParts = null, List<CustomEntityPart> customBlockParts = null,
            bool isTransparent = false, bool isAnimated = false, bool isDestroyable = false, bool isSystem = false, bool isCubical = true, 
            short health = 100, List<SpawnedItemRule> itemDropRules = null)
        {
            Name = name;
            MatterState = matterState;
            IsTransparent = isTransparent;
            IsAnimated = isAnimated;
            IsDestroyable = isDestroyable;
            IsSystem = isSystem;
            IsCubical = isCubical;
            Health = health;

            BlockParts = blockParts ?? new Dictionary<CubeFaceDirection, BlockPart>();
            CustomBlockParts = customBlockParts;// ?? new List<CustomBlockPart>();

            ItemDropRules = itemDropRules ?? new List<SpawnedItemRule>();
            BlockBehaviour = null; // new EmptyBlockBehavour();
        }

        public void Update100(GameTime gameTime, World world, Vector3i position)
        {
            if (BlockBehaviour != null)
            {
                BlockBehaviour.Update100(gameTime, world, position);
            }
        }

        public bool IsAirType()
        {
            return this == BlockTypeHelper.AIR_BLOCK_TYPE;
        }

        public float GetMaxHeight()
        {
            return 1f;
        }
    }
}