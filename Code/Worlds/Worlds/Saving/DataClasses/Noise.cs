using System;
using WorldsGame.Gamestates;
using WorldsGame.GUI;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Noise : ISaveDataSerializable<Noise>
    {
        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Noises"; } }

        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public string NoiseFunction { get; set; }

        public Noise(string worldSettingsName)
        {
            WorldSettingsName = worldSettingsName;
        }

        internal static SaverHelper<Noise> SaverHelper(string name)
        {
            return new SaverHelper<Noise>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Noise> SaverHelper()
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

        internal static BaseWorldSublistGUI<Noise> GetListGUI(WorldsGame game, WorldSettings worldSettings, MenuState menuState, Noise noise = null)
        {
            var gui = new BaseWorldSublistGUI<Noise>(game, worldSettings, SaverHelper(worldSettings.Name),
                preselectedValue: noise == null ? "" : noise.Name)
            {
                Title = "Edit noises",
                DeleteBoxText = "Delete noise?",
                CreateAction = (game_, worldSettings_) =>
                {
                    var noiseEditorGUI = new NoiseEditorGUI(game_, worldSettings_);
                    menuState.SetGUI(noiseEditorGUI);
                },
                EditAction = (game_, worldSettings_, selectedElement) =>
                {
                    var noiseEditorGUI = new NoiseEditorGUI(game_, worldSettings_, selectedElement);
                    menuState.SetGUI(noiseEditorGUI);
                }
            };

            return gui;
        }
    }
}