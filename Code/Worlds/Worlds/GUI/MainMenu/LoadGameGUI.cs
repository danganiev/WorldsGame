using System;
using System.Collections.Generic;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.Gamestates;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.World;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.GUI.MainMenu
{
    internal class LoadGameGUI : View.GUI.GUI
    {
        private ListControl _savedWorldsList;
        private ButtonControl _loadButton;
        private ButtonControl _deleteButton;

        private readonly List<WorldSave> _savedWorlds;

        protected override string LabelText { get { return "Load game"; } }

        protected override bool IsBackable { get { return true; } }

        private string SelectedWorldFullName
        {
            get { return _savedWorlds[_savedWorldsList.SelectedItems[0]].FullName; }
        }

        internal LoadGameGUI(WorldsGame game)
            : base(game)
        {
            _savedWorlds = new List<WorldSave>();
            Messenger.On("EscapeKeyPressed", Back);
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddWorldTypesList();
            AddLoadButton();
            AddDeleteButton();
        }

        protected override void LoadData()
        {
            _savedWorldsList.Items.Clear();
            _savedWorlds.Clear();

            foreach (WorldSave worldSave in WorldSave.StaticSaverHelper().LoadList())
            {
                _savedWorlds.Add(worldSave);
                _savedWorldsList.Items.Add(worldSave.Name);
            }

            if (_savedWorlds.Count == 0)
            {
                _loadButton.Enabled = false;
            }
        }

        protected override void Back()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            var newGameGUI = new NewGameGUI(Game);
            MenuState.SetGUI(newGameGUI);
        }

        private void AddLoadButton()
        {
            _loadButton = new ButtonControl
            {
                Text = "Load",
                Enabled = false,
                Bounds = new UniRectangle(backButton.Bounds.Left - 110, ButtonDistanceFromBottom, 100, 30)
            };
            _loadButton.Pressed += (sender, args) => LoadGame();

            pressableControls.Add(_loadButton);
            Screen.Desktop.Children.Add(_loadButton);
        }

        private void AddDeleteButton()
        {
            _deleteButton = new ButtonControl
            {
                Text = "Delete",
                Enabled = false,
                Bounds = new UniRectangle(_savedWorldsList.Bounds.Right + 10, _savedWorldsList.Bounds.Top, ButtonWidth, ButtonHeight)
            };
            _deleteButton.Pressed += (sender, args) => ShowDeletionAlertBox(deleteAction: DeleteWorldSave, deletionText: "Delete saved world?");

            pressableControls.Add(_deleteButton);
            Screen.Desktop.Children.Add(_deleteButton);
        }

        private void AddWorldTypesList()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            var label = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, 100, 30),
                Text = "Saved worlds:"
            };

            _savedWorldsList = new ListControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, label.Bounds.Bottom + 10, 230, 320),
                SelectionMode = ListSelectionMode.Single
            };
            _savedWorldsList.SelectionChanged += (sender, args) =>
            {
                _loadButton.Enabled = true;
                _deleteButton.Enabled = true;
            };

            listControls.Add(_savedWorldsList);

            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_savedWorldsList);
        }

        private void DeleteWorldSave()
        {
            WorldSave.DeleteWorldSave(SelectedWorldFullName);

            LoadData();

            CancelAlertBox();
        }

        private void LoadGame()
        {
            if (_savedWorlds.Count == 0)
            {
                return;
            }

            Messenger.Off("EscapeKeyPressed", Back);

            WorldSave worldSave = WorldSave.StaticSaverHelper().Load(SelectedWorldFullName);

            Game.ChangeState(new LoadingState(Game, worldSave));
        }
    }
}