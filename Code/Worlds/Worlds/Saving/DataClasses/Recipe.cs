using System;
using System.Collections.Generic;
using WorldsGame.Gamestates;
using WorldsGame.GUI;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Recipe : ISaveDataSerializable<Recipe>
    {
        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Recipes"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        // More info about the crafting system here: http://gamedev.stackexchange.com/a/21587
        public int XSize { get; set; }

        public int YSize { get; set; }

        // This should always have XSize * YSize size.
        public Dictionary<int, string> Items { get; set; }

        public string ResultItem { get; set; }

        public int ResultItemQuantity { get; set; }

        public Recipe(string worldSettingsName)
        {
            WorldSettingsName = worldSettingsName;
        }

        internal static SaverHelper<Recipe> SaverHelper(string name)
        {
            return new SaverHelper<Recipe>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Recipe> SaverHelper()
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
    }
}