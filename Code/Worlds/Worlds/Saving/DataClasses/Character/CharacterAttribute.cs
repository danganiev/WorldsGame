using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Gamestates;
using WorldsGame.GUI;

namespace WorldsGame.Saving
{
    [Serializable]
    public class CharacterAttribute : ISaveDataSerializable<CharacterAttribute>
    {
        public const int MAX_ATTRIBUTES = 8;
        public const int ICON_PIXEL_SIZE = 16;

        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "CharacterAttributes"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public Color[] IconFull { get; set; }

        public Color[] IconHalf { get; set; }

        public const float DefaultMinValue = 0;

        public float DefaultMaxValue { get; set; }

        public float DefaultValue { get; set; }

        internal static SaverHelper<CharacterAttribute> SaverHelper(string name)
        {
            return new SaverHelper<CharacterAttribute>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<CharacterAttribute> SaverHelper()
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

        internal static CharacterAttributeListGUI GetListGUI(
            WorldsGame game, WorldSettings worldSettings, MenuState menuState, CharacterAttribute characterAttribute = null)
        {
            var gui = new CharacterAttributeListGUI(
                game, worldSettings, SaverHelper(worldSettings.Name),
                preselectedValue: characterAttribute == null ? "" : characterAttribute.Name)
                          {
                              Title = "Edit character attributes",
                              DeleteBoxText = "Delete character attribute?",
                              CreateAction = (game_, worldSettings_) =>
                                                 {
                                                     var noiseEditorGUI = new CharacterAttributeEditorGUI(game_, worldSettings_);
                                                     menuState.SetGUI(noiseEditorGUI);
                                                 },
                              EditAction = (game_, worldSettings_, selectedElement) =>
                                               {
                                                   var noiseEditorGUI = new CharacterAttributeEditorGUI(game_, worldSettings_, selectedElement);
                                                   menuState.SetGUI(noiseEditorGUI);
                                               }
                          };

            return gui;
        }

        internal static void CreateHealthAttribute(Game game, string worldSettingsName)
        {
            using (var content = new ContentManager(game.Services, "Content"))
            {
                Texture2D heartFull = content.Load<Texture2D>("Textures\\HeartFull");
                Texture2D heartHalf = content.Load<Texture2D>("Textures\\HeartHalf");
                var heartFullColors = new Color[16 * 16];
                var heartHalfColors = new Color[16 * 16];
                heartFull.GetData(heartFullColors);
                heartHalf.GetData(heartHalfColors);

                var health = new CharacterAttribute
                {
                    Name = "Health",
                    DefaultValue = 100,
                    DefaultMaxValue = 100,
                    //                                     MinValue = 0,
                    WorldSettingsName = worldSettingsName,
                    IconFull = heartFullColors,
                    IconHalf = heartHalfColors
                };

                health.Save();

                content.Unload();
            }
        }
    }
}