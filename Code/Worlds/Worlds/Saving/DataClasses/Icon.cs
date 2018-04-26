using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Icon : ISaveDataSerializable<Icon>
    {
        public const int SIZE = 24;

        public static readonly Color[] EMPTY_COLORS = new Color[SIZE * SIZE];

        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Icons"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public Color[] Colors { get; set; }

        public int Width { get { return SIZE; } }

        public int Height { get { return SIZE; } }

        public static SaverHelper<Icon> SaverHelper(string name)
        {
            return new SaverHelper<Icon>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Icon> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        public void Save()
        {
            SaverHelper().Save(this);
        }

        public void Delete()
        {
            Delete(false);
        }

        public void Delete(bool deleteBlocks)
        {
            if (deleteBlocks)
            {
                IEnumerable<Block> blocks =
                    Block.SaverHelper(WorldSettingsName).LoadList().Where(block => block.ContainsTexture(Name));

                foreach (var block in blocks)
                {
                    block.Delete();
                }
            }

            SaverHelper().Delete(Name);
        }

        public static Icon Load(string worldName, string name)
        {
            return SaverHelper(worldName).Load(name);
        }

        public Texture2D GetIcon(GraphicsDevice graphicsDevice)
        {
            var result = new Texture2D(graphicsDevice, Width, Height);
            result.SetData(Colors);
            return result;
        }
    }
}