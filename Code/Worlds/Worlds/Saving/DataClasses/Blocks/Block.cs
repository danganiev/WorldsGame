using System;
using System.Collections.Generic;
using System.Linq;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Block : ISaveDataSerializable<Block>
    {
        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Blocks"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public bool IsFullBlock { get; set; }

        // This is for recreating the block in block editor
        public List<Cuboid> Cuboids { get; set; }

        // This is for block editor too, shouldn't be in compiled block
        public int HeightInBlocks { get; set; }

        public int LengthInBlocks { get; set; }

        public int WidthInBlocks { get; set; }

        public bool IsUnbreakable { get; set; }

        public bool IsLiquid { get; set; }

        public short Health { get; set; }

        public List<SpawnedItemRule> ItemDropRules { get; set; }

        public byte LightLevel { get; set; }

        public bool IsTransparent { get; set; }

        public bool IsAnimated { get; set; }

        public Block()
        {
            Health = 100;
            ItemDropRules = new List<SpawnedItemRule>();
        }

        internal static SaverHelper<Block> SaverHelper(string name)
        {
            return new SaverHelper<Block>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Block> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        public void Save()
        {
            DetectTransparency();
            DetectAnimatedTextures();
            SaverHelper().Save(this);
        }

        public void Delete()
        {
            SaverHelper().Delete(Name);

            DeleteRelatedItem(WorldSettingsName, Name);
        }

        private static void DeleteRelatedItem(string worldSettingsName, string name)
        {
            Item relatedItem = (from item in Item.SaverHelper(worldSettingsName).LoadList()
                                where item.Name == string.Format("{0} Block", name) && item.IsBlock
                                select item).FirstOrDefault();

            if (relatedItem != null)
            {
                relatedItem.Delete();
            }
        }

        public static void Delete(string worldSettingsName, string name)
        {
            SaverHelper(worldSettingsName).Delete(name);

            DeleteRelatedItem(worldSettingsName, name);
        }

        public bool ContainsTexture(string name)
        {
            for (int i = 0; i < Cuboids.Count; i++)
            {
                Cuboid cuboid = Cuboids[i];
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
            for (int i = 0; i < Cuboids.Count; i++)
            {
                Cuboid cuboid = Cuboids[i];
                textureNames.AddRange(cuboid.GetTextureNames());
            }

            return textureNames.Distinct();
        }

        public void DetectTransparency()
        {
            var textureNames = new List<string>();
            for (int i = 0; i < Cuboids.Count; i++)
            {
                Cuboid cuboid = Cuboids[i];

                textureNames.AddRange(cuboid.GetTextureNames());
            }

            foreach (string textureName in textureNames.Distinct())
            {
                if (Texture.Load(WorldSettingsName, textureName).IsTransparent)
                {
                    IsTransparent = true;
                    return;
                }
            }

            IsTransparent = false;
        }

        public void DetectAnimatedTextures()
        {
            var textureNames = new List<string>();
            for (int i = 0; i < Cuboids.Count; i++)
            {
                Cuboid cuboid = Cuboids[i];

                textureNames.AddRange(cuboid.GetTextureNames());
            }

            foreach (string textureName in textureNames.Distinct())
            {
                if (Texture.Load(WorldSettingsName, textureName).IsAnimated)
                {
                    IsAnimated = true;
                    return;
                }
            }

            IsTransparent = false;
        }
    }
}