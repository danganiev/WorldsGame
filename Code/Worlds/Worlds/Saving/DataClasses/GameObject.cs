using System;
using System.Collections.Generic;

using WorldsLib;

namespace WorldsGame.Saving
{
    internal enum GameObjectDirection
    {
        North,
        East,
        South,
        West
    }

    //Any object made from blocks
    [Serializable]
    public class GameObject : ISaveDataSerializable<GameObject>
    {
        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "GameObjects"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public Dictionary<Vector3i, string> Blocks { get; set; }

        public GameObject()
        {
            Blocks = new Dictionary<Vector3i, string>();
        }

        internal static SaverHelper<GameObject> SaverHelper(string name)
        {
            return new SaverHelper<GameObject>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<GameObject> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        internal static GameObject Load(string worldSettingsName, string name)
        {
            return SaverHelper(worldSettingsName).Load(name);
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