using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using Plane = WorldsGame.Saving.Plane;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledItem : IItemLike, ICustomModelHolder
    {
        [NonSerialized]
        private CompiledGameBundle _gameBundle;

        public CompiledGameBundle GameBundle
        {
            get { return _gameBundle; }
            set { _gameBundle = value; }
        }

        public string Name { get; set; }

        public string Description { get { return null; } }

        public int MaxStackCount { get; set; }

        // TODO: Needs to be renamed to IconColors in the interface
        public Color[] IconColors { get; set; }

        public int CuboidCount { get; set; }

        public Dictionary<int, List<CustomEntityPart>> Cuboids { get; set; }

        public Vector3 MinVertice { get; set; }

        public Vector3 MaxVertice { get; set; }

        public List<Effect> Action1Effects { get; set; }

        public List<Effect> Action2Effects { get; set; }

        public Dictionary<AnimationType, CompiledAnimation> FirstPersonAnimations { get; private set; }

        public ItemQuality ItemQuality { get; set; }

        // Default item holder cuboid is in default item, because it would be the same for all items
        public CompiledCuboid ItemHolderCuboid { get; set; }

        public bool IsSystem { get; set; }

        //For serialization only!
        public CompiledItem()
        {
        }

        public CompiledItem(CompiledGameBundle gameBundle, Item item)
        {
            GameBundle = gameBundle;
            Name = item.Name;
            MaxStackCount = item.MaxStackCount;

            var naturalNumberSpaceDiff = new Vector3(
                item.LengthInBlocks * 16, item.HeightInBlocks * 16, item.WidthInBlocks * 16);

            IconColors = item.IconColors;
            MinVertice = item.MinVertice + naturalNumberSpaceDiff;
            MaxVertice = item.MaxVertice + naturalNumberSpaceDiff;

            CuboidCount = item.Cuboids.Count;

            Action1Effects = item.Action1Effects ?? new List<Effect>();
            Action2Effects = item.Action2Effects ?? new List<Effect>();

            ItemQuality = item.ItemQuality;

            IsSystem = item.IsSystem;

            InitializeParts(item);
            InitializeItemHolderCuboid(item);
            InitializeAnimations(item);
        }

        private void InitializeParts(Item item)
        {
//            Vector3 minMaxDiff = MaxVertice - MinVertice;

            Cuboids = CustomEntityPart.GetPartsFromCuboids(item.Cuboids, /*minMaxDiff,*/ GameBundle, item.HeightInBlocks);
        }

        private void InitializeItemHolderCuboid(Item item)
        {
            ItemHolderCuboid = item.FirstPersonCuboid != null ? new CompiledCuboid(item.FirstPersonCuboid, isResized: false) : null;
        }

        private void InitializeAnimations(Item item)
        {
            if (item.FirstPersonAnimations == null)
            {
                FirstPersonAnimations = null;
                return;
            }

            FirstPersonAnimations = new Dictionary<AnimationType, CompiledAnimation>();

            foreach (KeyValuePair<AnimationType, CompiledAnimation> animation in item.FirstPersonAnimations)
            {
                FirstPersonAnimations[animation.Key] = animation.Value;
            }
        }

        public InventoryItem ToInventoryItem(int quantity = 1)
        {
            return new InventoryItem
            {
                Name = Name,
                Quantity = quantity
            };
        }

        public Dictionary<AnimationType, ComputedAnimation> ComputeAnimations()
        {
            if (FirstPersonAnimations == null)
            {
                return null;
            }

            var result = new Dictionary<AnimationType, ComputedAnimation>();

            foreach (KeyValuePair<AnimationType, CompiledAnimation> animation in FirstPersonAnimations)
            {
                result[animation.Key] = ComputedAnimation.CreateFromCompiledAnimation(animation.Value, isScaled: false, isFPSItem: true);
            }

            return result;
        }
    }
}