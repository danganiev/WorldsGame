using LibNoise.Xna;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;

using WorldsGame.Saving;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class NoiseEditorGUI : View.GUI.GUI
    {
        private BaseWorldsTextControl _nameInput;
        private BaseWorldsTextControl _noiseFunctionInput;
        private LabelControl _nameLabel;

        internal Noise Noise { get; set; }

        internal WorldSettings World { get; set; }

        protected override string LabelText { get { return IsNew ? "Create noise" : "Edit noise"; } }

        protected override bool IsSaveable { get { return true; } }

        protected override bool IsBackable { get { return true; } }

        protected override int LabelWidth { get { return 100; } }

        internal bool IsNew
        {
            get { return Noise == null; }
        }

        internal NoiseEditorGUI(WorldsGame game, WorldSettings world)
            : base(game)
        {
            World = world;
        }

        internal NoiseEditorGUI(WorldsGame game, WorldSettings world, Noise noise)
            : base(game)
        {
            World = world;
            Noise = noise;
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddSaveInputPanel();
            AddBottomPanel();
        }

        private void AddSaveInputPanel()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            _nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Name:"
            };

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(_nameLabel.Bounds.Right + 10, Y, 340, 30),
            };

            if (!IsNew)
                _nameInput.Text = Noise.Name;

            BaseWorldsTextControls.Add(_nameInput);

            Screen.Desktop.Children.Add(_nameLabel);
            Screen.Desktop.Children.Add(_nameInput);
        }

        private void AddBottomPanel()
        {
            var Y = _nameLabel.Bounds.Bottom + 10;

            var noiseFunctionLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Noise Function:"
            };

            _noiseFunctionInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(noiseFunctionLabel.Bounds.Right + 10, Y, 700, 30),
            };

            if (!IsNew)
                _noiseFunctionInput.Text = Noise.NoiseFunction;

            Screen.Desktop.Children.Add(noiseFunctionLabel);
            Screen.Desktop.Children.Add(_noiseFunctionInput);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Noise needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in noise name.");
                return false;
            }

            if (_noiseFunctionInput.Text == "")
            {
                ShowAlertBox("Module function cannot be empty");
                return false;
            }

            try
            {
                StringParser.Parse(_noiseFunctionInput.Text);
            }
            catch (LibNoiseStringParserException e)
            {
                ShowAlertBox(e.Message);
                return false;
            }

            return true;
        }

        protected override void Save()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            if (!IsNew && Noise.Name != null)
            {
                Noise.Delete();
            }

            Noise = new Noise(World.Name) { Name = _nameInput.Text, NoiseFunction = _noiseFunctionInput.Text };

            Noise.Save();

            Back();
        }

        protected override void Back()
        {
            var worldEditorGUI = Noise.GetListGUI(Game, World, MenuState, Noise);
            MenuState.SetGUI(worldEditorGUI);
        }
    }
}