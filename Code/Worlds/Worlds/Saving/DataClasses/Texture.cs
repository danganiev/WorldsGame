using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Gamestates;
using WorldsGame.GUI;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Texture : ISaveDataSerializable<Texture>
    {
        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Textures"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public Color[] Colors { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Weight { get { return Width + Height * 32; } }

        public List<Color[]> FrameColors { get; set; }

        public bool IsAnimated { get; set; }

        public bool IsNew { get { return Name == null; } }

        public bool IsTransparent { get; set; }

        public static SaverHelper<Texture> SaverHelper(string name)
        {
            return new SaverHelper<Texture>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Texture> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        public void Save()
        {
            DetectTransparency();
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

        internal static BaseWorldSublistGUI<Texture> GetListGUI(WorldsGame game, WorldSettings worldSettings, Texture texture = null)
        {
            var gui = new BaseWorldSublistGUI<Texture>(game, worldSettings, SaverHelper(worldSettings.Name),
                preselectedValue: texture == null ? "" : texture.Name)
            {
                Title = "Edit textures",
                DeleteBoxText = "Delete texture?",
                CreateAction = (game_, worldSettings_) => game_.GameStateManager.Push(new TextureDrawingState(game_, worldSettings_)),
                EditAction = (game_, worldSettings_, selectedElement) => game_.GameStateManager.Push(new TextureDrawingState(game_, worldSettings_, selectedElement)),
                DeleteAction = (game_, worldSettings_, element) => element.Delete(true)
            };

            return gui;
        }

        public static Texture Load(string worldName, string name)
        {
            return SaverHelper(worldName).Load(name);
        }

        public Texture2D GetTexture(GraphicsDevice graphicsDevice)
        {
            var result = new Texture2D(graphicsDevice, Width, Height);
            result.SetData(Colors);
            return result;
        }

        public void DetectTransparency()
        {
            for (int i = 0; i < Colors.Length; i++)
            {
                Color color = Colors[i];

                if (color.A < 255)
                {
                    IsTransparent = true;
                    return;
                }
            }

            IsTransparent = false;
        }
    }
}