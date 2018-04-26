using WorldsGame.Saving;

namespace WorldsGame.GUI
{
    internal class BaseWorldSettingsGUI : View.GUI.GUI
    {
        internal WorldSettings WorldSettings { get; set; }

        internal BaseWorldSettingsGUI(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            WorldSettings = worldSettings;
        }
    }
}