using System.Collections.Generic;
using WorldsGame.Playing.DataClasses;

namespace WorldsGame.Playing.NPCs
{
    internal static class CharacterAttributeHelper
    {
        internal static Dictionary<string, CompiledCharacterAttribute> CompiledAttributes { get; private set; }

        // System Items
        //        internal static Dictionary<string, CompiledItem> SystemItems { get; private set; }

        // Caches for sending blocks over network
        //        internal static Dictionary<string, int> InverseBlockTypeCache { get; private set; }
        //
        //        internal static Dictionary<int, string> BlockTypeCache { get; private set; }

        static CharacterAttributeHelper()
        {
            CompiledAttributes = new Dictionary<string, CompiledCharacterAttribute>();
        }

        internal static void Clear()
        {
            CompiledAttributes.Clear();
            //            SystemBlockTypes.Clear();
            //            InverseBlockTypeCache.Clear();
            //            BlockTypeCache.Clear();
        }

        internal static CompiledCharacterAttribute Get(string name)
        {
            name = name.ToLower();

            if (CompiledAttributes.ContainsKey(name))
            {
                return CompiledAttributes[name];
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

        internal static void Initialize(CompiledGameBundle compiledGameBundle)
        {
            Clear();

            for (int index = 0; index < compiledGameBundle.CharacterAttributes.Count; index++)
            {
                CompiledCharacterAttribute characterAttribute = compiledGameBundle.CharacterAttributes[index];

                CompiledAttributes.Add(characterAttribute.Name.ToLower(), characterAttribute);
            }
        }

        internal static float GetMinValue(string name)
        {
            return CompiledAttributes[name].DefaultMinValue;
        }

        internal static float GetMaxValue(string name)
        {
            return CompiledAttributes[name].DefaultMaxValue;
        }

        internal static float GetMinMaxDiff(string name)
        {
            return CompiledAttributes[name].MinMaxDiff;
        }

        internal static int GetIndex(string name)
        {
            return CompiledAttributes[name].FullTextureIndex;
        }
    }
}