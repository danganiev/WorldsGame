using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors.Blocks;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.GeometricPrimitives;

namespace WorldsGame.Saving.DataClasses
{
    // Position and other 3d stuff of items\particles\whatever in the animation\model

    [Serializable]
    public class Item : ISaveDataSerializable<Item>, IItemLike
    {
        public const string DEFAULT_ITEM_NAME = "System__Default";

        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Items"; } }

        public string Name { get; set; }

        public string Description { get { return null; } }

        public string WorldSettingsName { get; set; }

        public Color[] IconColors { get; set; }

        // This is for recreating the item in model editor
        public List<Cuboid> Cuboids { get; set; }

        // Default position of FP item looks
        public Cuboid FirstPersonCuboid { get; set; }

        // Only for model editor.
        // X
        public int LengthInBlocks { get; set; }

        // Y
        public int HeightInBlocks { get; set; }

        // Z
        public int WidthInBlocks { get; set; }

        public int MaxStackCount { get; set; }

        public Vector3 MinVertice { get; set; }

        public Vector3 MaxVertice { get; set; }

        public List<Effect> Action1Effects { get; set; }

        public List<Effect> Action2Effects { get; set; }

        public bool IsBlock { get; set; }

        public bool IsSystem { get; set; }

        public Dictionary<AnimationType, CompiledAnimation> FirstPersonAnimations { get; set; }

        public ItemQuality ItemQuality { get; set; }

        // This is for the future
        // float Cooldown // Cooldown in seconds at which item can be used (I want cooldown on effect, but this is too hard to implement, so it's better here I guess)

        internal static SaverHelper<Item> SaverHelper(string name)
        {
            return new SaverHelper<Item>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public Item()
        {
            Action1Effects = new List<Effect>();
            Action2Effects = new List<Effect>();
        }

        public SaverHelper<Item> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        public void Save()
        {
            SaverHelper().Save(this);
        }

        public void Delete()
        {
            SaverHelper().Delete(Name);
        }

        public static void Delete(string worldSettingsName, string name)
        {
            SaverHelper(worldSettingsName).Delete(name);
        }

        public bool ContainsTexture(string name)
        {
            foreach (Cuboid cuboid in Cuboids)
            {
                bool result = cuboid.ContainsTexture(name);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<string> GetTextureNames()
        {
            var textureNames = new List<string>();
            foreach (Cuboid cuboid in Cuboids)
            {
                textureNames.AddRange(cuboid.GetTextureNames());
            }

            return textureNames.Distinct();
        }

        public void PrepareAnimations(EditedModel itemModel = null, bool isNew = false, bool forceAnimations = false)
        {
            if (FirstPersonAnimations == null && forceAnimations)
            {
                FirstPersonAnimations = new Dictionary<AnimationType, CompiledAnimation>();

                foreach (AnimationType at in EnumUtils.GetValues<AnimationType>())
                {
                    FirstPersonAnimations.Add(at, new CompiledAnimation(Cuboids.Count));
                }
            }

            if (!isNew)
            {
                RefreshAnimationsPerCuboids(itemModel);
            }
        }

        // Updates data on animations if cuboids were added/removed on model
        private void RefreshAnimationsPerCuboids(EditedModel itemModel = null)
        {
            if (itemModel != null && FirstPersonAnimations != null)
            {
                foreach (KeyValuePair<AnimationType, CompiledAnimation> compiledAnimation in FirstPersonAnimations)
                {
                    compiledAnimation.Value.RefreshAnimationPerCuboids(itemModel);
                }
            }
        }

        public static void CreateDefaultItem(string worldSettingsName)
        {
            var defaultItem = SaverHelper(worldSettingsName).Load(DEFAULT_ITEM_NAME);

            if (defaultItem == null)
            {
                defaultItem = new Item
                {
                    Name = DEFAULT_ITEM_NAME,
                    WorldSettingsName = worldSettingsName,
                    Cuboids = new List<Cuboid>(),
                    MaxStackCount = 0,
                    LengthInBlocks = 1,
                    HeightInBlocks = 1,
                    WidthInBlocks = 1,
                    IsSystem = true
                };

                defaultItem.PrepareAnimations(null, isNew: true, forceAnimations: true);
            }

            if (defaultItem.FirstPersonCuboid == null)
            {
                defaultItem.FirstPersonCuboid = new Cuboid(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1f, 1f, 1f), isItem: true);
            }

            defaultItem.Save();
        }
    }
}