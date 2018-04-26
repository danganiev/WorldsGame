using System.Collections.Generic;
using WorldsGame.Playing.DataClasses;

namespace WorldsGame.Terrain.Blocks.Types
{
    internal static class BlockTypeHelper
    {
        // The blocks which player can hold and place
        internal static Dictionary<string, BlockType> AvailableBlockTypes { get; private set; }

        // System block types
        internal static Dictionary<string, BlockType> SystemBlockTypes { get; private set; }

        // Caches for sending blocks over network
        internal static Dictionary<string, int> InverseBlockTypeCache { get; private set; }

        internal static Dictionary<int, string> BlockTypeCache { get; private set; }

        static BlockTypeHelper()
        {
            AvailableBlockTypes = new Dictionary<string, BlockType>();
            SystemBlockTypes = new Dictionary<string, BlockType>();
            InverseBlockTypeCache = new Dictionary<string, int>();
            BlockTypeCache = new Dictionary<int, string>();
        }

        internal static void AddToCache(int key, string name)
        {
            InverseBlockTypeCache.Add(name, key);
            BlockTypeCache.Add(key, name);
        }

        // This one is constant because it has no textures
        internal static readonly BlockType AIR_BLOCK_TYPE = new BlockType(
            CompiledBlock.AIR_CUBE, BlockTypeMatterState.Gas, isTransparent: true, isSystem: true);

        internal const int AIR_BLOCK_INDEX = -1;

        internal static BlockType WaitBlockType { get { return SystemBlockTypes[CompiledBlock.WAIT_CUBE]; } }

        internal static void Clear()
        {
            AvailableBlockTypes.Clear();
            SystemBlockTypes.Clear();
            InverseBlockTypeCache.Clear();
            BlockTypeCache.Clear();
        }

        internal static BlockType Get(string name)
        {
            if (AvailableBlockTypes.ContainsKey(name))
            {
                return AvailableBlockTypes[name];
            }

            if (name == CompiledBlock.AIR_CUBE)
            {
                return AIR_BLOCK_TYPE;
            }

            if (SystemBlockTypes.ContainsKey(name))
            {
                return SystemBlockTypes[name];
            }

            return AIR_BLOCK_TYPE;
        }

        internal static BlockType Get(int key)
        {
            return Get(BlockTypeCache[key]);
        }

        internal static void Initialize(CompiledGameBundle compiledGameBundle)
        {
            Clear();
            AddToCache(AIR_BLOCK_INDEX, AIR_BLOCK_TYPE.Name);

            for (int index = 0; index < compiledGameBundle.Blocks.Count; index++)
            {
                CompiledBlock compiledBlock = compiledGameBundle.Blocks[index];

                BlockTypeMatterState matterState = compiledBlock.IsLiquid ? BlockTypeMatterState.Liquid : BlockTypeMatterState.Solid;

                BlockType blockType = compiledBlock.IsCubical
                                          ? new BlockType(compiledBlock.Name, blockParts: compiledBlock.BlockParts,
                                                          isDestroyable: compiledBlock.IsDestroyable, matterState: matterState,
                                                          isSystem: compiledBlock.IsSystem, itemDropRules: compiledBlock.ItemDropRules, 
                                                          isTransparent: compiledBlock.IsTransparent, isAnimated: compiledBlock.IsAnimated)
                                          : new BlockType(compiledBlock.Name, customBlockParts: compiledBlock.Parts,
                                                          isDestroyable: compiledBlock.IsDestroyable, matterState: matterState,
                                                          isSystem: compiledBlock.IsSystem, isCubical: false, itemDropRules: compiledBlock.ItemDropRules,
                                                          isTransparent: compiledBlock.IsTransparent); // I guess I can make custom objects support animation too by redefining their vertex data, just later

                if (blockType.IsSystem)
                {
                    SystemBlockTypes.Add(blockType.Name, blockType);
                }
                else
                {
                    AvailableBlockTypes.Add(blockType.Name, blockType);
                }

                AddToCache(index, blockType.Name);
            }
        }
    }
}