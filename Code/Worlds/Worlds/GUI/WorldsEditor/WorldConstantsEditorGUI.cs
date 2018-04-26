using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using WorldsGame.Saving;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class WorldConstantsEditorGUI : View.GUI.GUI
    {
        private readonly WorldSettings _worldSettings;
        private NumberInputControl _sunlitHeightInput;

        protected override int LabelWidth { get { return 100; } }

        protected override string LabelText { get { return "World constants"; } }

        protected override bool IsBackable { get { return true; } }

        protected override bool IsSaveable { get { return true; } }

        public WorldConstantsEditorGUI(WorldsGame game, WorldSettings worldSettings)
            : base(game)
        {
            _worldSettings = worldSettings;
            Game = game;
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddSunlitHeight();
        }

        protected override void LoadData()
        {
        }

        //        private void AddTitle()
        //        {
        //            var titleLabel = new LabelControl("World constants")
        //            {
        //                Bounds = new UniRectangle(FirstRowLabelX, TitleY, LabelWidth, LabelHeight)
        //            };
        //
        //            Screen.Desktop.Children.Add(titleLabel);
        //        }

        private void AddSunlitHeight()
        {
            var label = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, titleLabel.Bounds.Bottom + 50, 150, LabelHeight),
                Text = "Default sunlit height:",
                PopupText = "Average height above which the sun is guaranteed to shine"
            };

            _sunlitHeightInput = new NumberInputControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + 10, titleLabel.Bounds.Bottom + 50, 50, LabelHeight),
                IsIntegerOnly = true,
                Text = _worldSettings.SunlitHeight.ToString()
            };

            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_sunlitHeightInput);
        }

        protected override void Save()
        {
            _worldSettings.SunlitHeight = _sunlitHeightInput.GetInt();
            Back();
        }

        protected override void Back()
        {
            var worldEditorGUI = new WorldEditorGUI(Game, _worldSettings);
            MenuState.SetGUI(worldEditorGUI);
        }
    }
}