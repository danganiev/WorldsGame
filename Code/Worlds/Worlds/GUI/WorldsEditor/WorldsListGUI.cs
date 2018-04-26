using System;
using System.Linq;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Arcade;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.GUI.MainMenu;
using WorldsGame.Saving;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.GUI
{
    internal class WorldsListGUI : View.GUI.GUI
    {
        private ListControl _worldsList;

        private static SaverHelper<WorldSettings> WorldsSaverHelper
        {
            get { return WorldSettings.StaticSaverHelper(); }
        }

        private bool IsWorldSelected
        {
            get { return _worldsList.SelectedItems.Count != 0; }
        }

        private WorldSettings SelectedWorld
        {
            get
            {
                string selectedWorldName = _worldsList.Items[_worldsList.SelectedItems[0]];

                WorldSettings world = WorldsSaverHelper.Load(selectedWorldName);
                return world;
            }
        }

        protected override string LabelText { get { return "Worlds"; } }

        protected override string DeletionText
        {
            get
            {
                return "Please confirm world settings deletion.\n Just the settings would be deleted, nothing else.";
            }
        }

        protected override bool IsBackable { get { return true; } }

        internal WorldsListGUI(WorldsGame game)
            : base(game)
        {
            Messenger.On("EscapeKeyPressed", Back);
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddListPanel();
        }

        protected override void LoadData()
        {
            LoadList(_worldsList, WorldsSaverHelper);
        }

        private void AddListPanel()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            _worldsList = new ListControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, 230, 370),
                SelectionMode = ListSelectionMode.Single
            };
            listControls.Add(_worldsList);

            var buttonX = _worldsList.Bounds.Right + 10;

            var createButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, _worldsList.Bounds.Top, 65, 30),
                Text = "Create"
            };
            createButton.Pressed += (sender, args) => NewWorld();
            pressableControls.Add(createButton);

            var editButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, createButton.Bounds.Bottom + 10, 65, 30),
                Text = "Edit"
            };
            editButton.Pressed += (sender, args) => EditWorld();
            pressableControls.Add(editButton);

            var deleteButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, editButton.Bounds.Bottom + 10, 65, 30),
                Text = "Delete"
            };
            deleteButton.Pressed += (sender, args) => ShowDeletionAlertBox();
            pressableControls.Add(deleteButton);

            Screen.Desktop.Children.Add(_worldsList);
            Screen.Desktop.Children.Add(createButton);
            Screen.Desktop.Children.Add(editButton);
            Screen.Desktop.Children.Add(deleteButton);
        }

        private void NewWorld()
        {
            var worldEditorGUI = new WorldEditorGUI(Game);
            MenuState.SetGUI(worldEditorGUI);
        }

        private void EditWorld()
        {
            if (!IsWorldSelected)
                return;

            var worldEditorGUI = new WorldEditorGUI(Game, SelectedWorld);

            MenuState.SetGUI(worldEditorGUI);
        }

        protected override void ShowDeletionAlertBox(Action deleteAction = null, string deletionText = "")
        {
            if (!IsWorldSelected)
                return;

            base.ShowDeletionAlertBox(deleteAction);
        }

        protected override void Delete()
        {
            if (!IsWorldSelected)
                return;

            SelectedWorld.Delete();

            LoadData();

            CancelAlertBox();
        }

        protected override void Back()
        {
            Messenger.Off("EscapeKeyPressed", Back);

            var mainMenuGUI = new MainMenuGUI(Game);
            MenuState.SetGUI(mainMenuGUI);
        }
    }
}