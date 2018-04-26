using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.Files;

namespace WorldsGame.GUI.ModelEditor
{
    internal class AddSoundPanel : View.GUI.GUI
    {
        private const int PANEL_WIDTH = 400;
        private const int PANEL_HEIGHT = 300;

        private const int LABEL_MARGIN = 50;

        private const int LIST_WIDTH = 250;
        private const int LIST_HEIGHT = 70;

        private readonly View.GUI.GUI _parentGUI;
        private readonly WorldSettings _worldSettings;
        private LabelControl _titleLabel;
        private ListControl _soundList;

        internal int SoundTypeNumber { get; set; }

        internal PanelControl SoundPanel { get; private set; }

        internal event Action OnGoingBack = () => { };

        internal event Action<string, int> OnAddition = (effect, soundNumber) => { };

        internal AddSoundPanel(View.GUI.GUI parentGUI, WorldSettings worldSettings)
        {
            _parentGUI = parentGUI;
            _worldSettings = worldSettings;
            SoundPanel = new PanelControl();
        }

        protected override void CreateControls()
        {
            AddMainPanel();

            AddEffectsTargetList();

            _soundList.SelectedItems.Add(0);
        }

        protected override void LoadData()
        {
            LoadList(_soundList, SoundLoader.SoundList);
        }

        private void AddMainPanel()
        {
            const int Y = 15;
            const int X = 30;

            var panelStartX = (int)_parentGUI.Screen.Width / 2 - PANEL_WIDTH / 2;
            var panelStartY = (int)_parentGUI.Screen.Height / 2 - PANEL_HEIGHT / 2;

            SoundPanel = new PanelControl
            {
                Bounds = new UniRectangle(panelStartX, panelStartY, PANEL_WIDTH, PANEL_HEIGHT)
            };

            _titleLabel = new LabelControl
            {
                Text = "Add sound",
                Bounds = new UniRectangle(X, Y, 130, _parentGUI.LabelHeight),
                IsHeader = true
            };

            var backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 70)), new UniScalar(1f, -(X + 30)), 70, 30)
            };
            backButton.Pressed += (sender, args) => Back();

            var addButton = new ButtonControl
            {
                Text = "Add",
                Bounds = new UniRectangle(new UniScalar(1f, -(X + 150)), new UniScalar(1f, -(X + 30)), 70, 30)
            };
            addButton.Pressed += (sender, args) => Add();

            SoundPanel.Children.Add(_titleLabel);
            SoundPanel.Children.Add(backButton);
            SoundPanel.Children.Add(addButton);

            SoundPanel.BringToFront();
        }

        private void AddEffectsTargetList()
        {
            var Y = _titleLabel.Bounds.Bottom + 50;

            var label = new LabelControl
            {
                Bounds = new UniRectangle(_titleLabel.Bounds.Left, Y, 70, 20),
                Text = "Sounds:"
            };

            _soundList = new ListControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + LABEL_MARGIN, Y, LIST_WIDTH, LIST_HEIGHT),
                SelectionMode = ListSelectionMode.Single
            };

            SoundPanel.Children.Add(label);
            SoundPanel.Children.Add(_soundList);
        }

        protected override void Back()
        {
            OnGoingBack();
        }

        private void Add()
        {
            if (!_soundList.IsSelected())
            {
                _parentGUI.ShowAlertBox("Please select a sound");
                return;
            }

            OnAddition(_soundList.SelectedName(), SoundTypeNumber);
            Back();
        }
    }
}