using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Playing.Items.Inventory
{
    internal static class ItemHelper
    {
        internal static Dictionary<string, CompiledItem> Items { get; private set; }

        internal static Dictionary<string, CompiledEffect> Action1Effects { get; private set; }

        internal static Dictionary<string, CompiledEffect> Action2Effects { get; private set; }

        internal static CompiledCuboid DefaultItemHolderCuboid { get; private set; }

        // System Items
        internal static Dictionary<string, CompiledItem> SystemItems { get; private set; }

        internal static Dictionary<string, Dictionary<AnimationType, ComputedAnimation>> FirstPersonAnimations { get; private set; }

        internal static Dictionary<AnimationType, ComputedAnimation> DefaultFirstPersonAnimations { get; private set; }

        // Caches for sending blocks over network
        //        internal static Dictionary<string, int> InverseBlockTypeCache { get; private set; }
        //
        //        internal static Dictionary<int, string> BlockTypeCache { get; private set; }

        static ItemHelper()
        {
            Items = new Dictionary<string, CompiledItem>();
            SystemItems = new Dictionary<string, CompiledItem>();
            FirstPersonAnimations = new Dictionary<string, Dictionary<AnimationType, ComputedAnimation>>();
            Action1Effects = new Dictionary<string, CompiledEffect>();
            Action2Effects = new Dictionary<string, CompiledEffect>();
        }

        internal static void Clear()
        {
            Items.Clear();
            SystemItems.Clear();
            FirstPersonAnimations.Clear();
            Action1Effects.Clear();
            Action2Effects.Clear();
        }

        internal static CompiledItem Get(string name)
        {
            name = name.ToLower();

            if (Items.ContainsKey(name))
            {
                return Items[name];
            }

            return null;
        }

        internal static CompiledItem Get(InventoryItem item)
        {
            return Get(item.Name);
        }

        //        internal static BlockType Get(int key)
        //        {
        //            return Get(BlockTypeCache[key]);
        //        }

        internal static void Initialize(CompiledGameBundle compiledGameBundle)
        {
            Clear();

            for (int index = 0; index < compiledGameBundle.Items.Count; index++)
            {
                CompiledItem item = compiledGameBundle.Items[index];

                if (item.IsSystem)
                {
                    SystemItems.Add(item.Name, item);

                    if (item.Name == Item.DEFAULT_ITEM_NAME)
                    {
                        DefaultItemHolderCuboid = item.ItemHolderCuboid;
                    }

                    DefaultFirstPersonAnimations = item.ComputeAnimations();

                    continue;
                }

                Items.Add(item.Name.ToLower(), item);

                var compiledEffect = new CompiledEffect();

                foreach (Effect effect in item.Action1Effects)
                {
                    compiledEffect.AddEffect(effect);
                }

                Action1Effects.Add(item.Name.ToLower(), compiledEffect);

                compiledEffect = new CompiledEffect();

                foreach (Effect effect in item.Action2Effects)
                {
                    compiledEffect.AddEffect(effect);
                }

                Action2Effects.Add(item.Name.ToLower(), compiledEffect);

                FirstPersonAnimations.Add(item.Name.ToLower(), item.ComputeAnimations());

                //                AddToCache(index, blockType.Name);
            }
        }

        public static int GetQuantityDiff(int quantity, string itemName)
        {
            CompiledItem item = Get(itemName);

            return item.MaxStackCount - quantity;
        }
    }
}