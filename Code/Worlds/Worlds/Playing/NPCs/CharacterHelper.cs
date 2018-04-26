using System.Collections.Generic;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Playing.NPCs
{
    internal static class CharacterHelper
    {
        internal static Dictionary<string, CompiledCharacter> Characters { get; private set; }

        // System Items
        //        internal static Dictionary<string, CompiledItem> SystemItems { get; private set; }

        // Caches for sending blocks over network
        //        internal static Dictionary<string, int> InverseBlockTypeCache { get; private set; }
        //
        //        internal static Dictionary<int, string> BlockTypeCache { get; private set; }

        internal static CompiledCharacter PlayerCharacter { get; private set; }

        internal static Dictionary<string, Dictionary<AnimationType, ComputedAnimation>> BasicAnimations { get; private set; }

        static CharacterHelper()
        {
            Characters = new Dictionary<string, CompiledCharacter>();
            //            SystemBlockTypes = new Dictionary<string, BlockType>();
            //            InverseBlockTypeCache = new Dictionary<string, int>();
            //            BlockTypeCache = new Dictionary<int, string>();
            BasicAnimations = new Dictionary<string, Dictionary<AnimationType, ComputedAnimation>>();
        }

        internal static void Clear()
        {
            Characters.Clear();
            //            SystemBlockTypes.Clear();
            //            InverseBlockTypeCache.Clear();
            //            BlockTypeCache.Clear();
            BasicAnimations.Clear();
        }

        internal static CompiledCharacter Get(string name)
        {
            name = name.ToLower();

            if (Characters.ContainsKey(name))
            {
                return Characters[name];
            }

            //            if (name == CompiledBlock.AIR_CUBE)
            //            {
            //                return AIR_BLOCK_TYPE;
            //            }
            //
            //            if (SystemBlockTypes.ContainsKey(name))
            //            {
            //                return SystemBlockTypes[name];
            //            }
            //
            //            return AIR_BLOCK_TYPE;
            return null;
        }

        //        internal static BlockType Get(int key)
        //        {
        //            return Get(BlockTypeCache[key]);
        //        }

        internal static void Initialize(CompiledGameBundle compiledGameBundle)
        {
            Clear();
            //            AddToCache(AIR_BLOCK_INDEX, AIR_BLOCK_TYPE.Name);

            for (int index = 0; index < compiledGameBundle.Characters.Count; index++)
            {
                CompiledCharacter character = compiledGameBundle.Characters[index];

                Characters.Add(character.Name.ToLower(), character);
                BasicAnimations.Add(character.Name.ToLower(), character.ComputeAnimations());

                if (character.IsPlayerCharacter/* && PlayerCharacter != null*/)
                {
                    PlayerCharacter = character;
                }
            }
        }
    }
}