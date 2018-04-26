using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.Gamestates;
using WorldsGame.Playing;
using WorldsGame.Saving;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class ObjectEditorPauseMenuGUI : PauseMenuGUI
    {
        // ** <Outdated>
        // Save button should appear but shouldn't work and constant message would hike from up of the screen when:
        // * Player has not selected anything
        // * Player has selected empty volume
        //
        // Save button should appear, but saving should not work and bring error message instead when:
        // * Saving failed
        //
        // Save button should be disabled if player hasn't entered the name of object
        // ** </Outdated>

        private const int OBJECT_LIST_HEIGHT = 150;
        private const int OBJECT_LIST_WIDTH = 320;

        private readonly SelectionGrid _selectionGrid;

        private BaseWorldsTextControl _nameInput;
        private ButtonControl _saveButton;
        private ListControl _objectList;

        internal ObjectEditorPauseMenuGUI(WorldsGame game, SelectionGrid selectionGrid, PlayingState playingState, Screen screen)
            : base(game, playingState, screen)
        {
            _selectionGrid = selectionGrid;
        }

        protected override void LoadData()
        {
            base.LoadData();
            LoadObjectList();
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            CreateSaveInput();
            CreateObjectList();
        }

        protected override void CreateToMainMenuButton()
        {
            toMainMenuButton = new ButtonControl
            {
                Text = "Back to Object Menu",
                Bounds = new UniRectangle(backButton.Bounds.Left - 185, ButtonDistanceFromBottom, 175, 32)
            };
            toMainMenuButton.Pressed += (sender, arguments) => ToMainMenu();
            Screen.Desktop.Children.Add(toMainMenuButton);
        }

        private void CreateSaveInput()
        {
            _saveButton = new ButtonControl
            {
                Text = "Save object",
                Bounds = new UniRectangle(ButtonDistanceFromRight - 30, ButtonDistanceFromBottom - 50, 130, 32)
            };
            _saveButton.Pressed += (sender, arguments) => SaveSelectedObject();

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(_saveButton.Bounds.Left - 320, ButtonDistanceFromBottom - 50, 310, 32)
            };

            Screen.Desktop.Children.Add(_nameInput);
            Screen.Desktop.Children.Add(_saveButton);
        }

        private void CreateObjectList()
        {
            var Y = ButtonDistanceFromTop;

            _objectList = new ListControl
            {
                Bounds = new UniRectangle(new UniScalar(1f, -(OBJECT_LIST_WIDTH + 10 + ButtonWidth + 60 + 50)),
                    Y, OBJECT_LIST_WIDTH, OBJECT_LIST_HEIGHT),
                SelectionMode = ListSelectionMode.Single
            };

            listControls.Add(_objectList);

            var newButton = new ButtonControl
            {
                Bounds = new UniRectangle(_objectList.Bounds.Right + 10, Y, ButtonWidth + 50, ButtonHeight),
                Text = "New object"
            };
            newButton.Pressed += (sender, args) => NewObject();
            pressableControls.Add(newButton);

            var loadButton = new ButtonControl
            {
                Bounds = new UniRectangle(_objectList.Bounds.Right + 10, newButton.Bounds.Bottom + 10, ButtonWidth + 50, ButtonHeight),
                Text = "Load object"
            };
            loadButton.Pressed += (sender, args) => LoadObject();
            pressableControls.Add(loadButton);

            var deleteButton = new ButtonControl
            {
                Bounds = new UniRectangle(_objectList.Bounds.Right + 10, loadButton.Bounds.Bottom + 10, ButtonWidth + 50, ButtonHeight),
                Text = "Delete object"
            };
            deleteButton.Pressed += (sender, args) => ShowDeleteObjectAlertBox();
            pressableControls.Add(deleteButton);

            Screen.Desktop.Children.Add(_objectList);
            Screen.Desktop.Children.Add(newButton);
            Screen.Desktop.Children.Add(loadButton);
            Screen.Desktop.Children.Add(deleteButton);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Object needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in object name.");
                return false;
            }

            return true;
        }

        private void SaveSelectedObject()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            _selectionGrid.SaveSelectedObject(_nameInput.Text);

            LoadObjectList();
        }

        private void LoadObjectList()
        {
            LoadList(_objectList, GameObject.SaverHelper(playingState.WorldSettingsName));
        }

        private void NewObject()
        {
        }

        private void LoadObject()
        {
            if (_objectList.SelectedName() != null)
            {
                ((ObjectCreationState)playingState).LoadObject(_objectList.SelectedName());
            }
        }

        private void ShowDeleteObjectAlertBox()
        {
            ShowDeletionAlertBox(DeleteObject, "Are you sure you want to delete selected object?");
        }

        private void DeleteObject()
        {
            if (_objectList.SelectedName() != null)
            {
                ((ObjectCreationState)playingState).DeleteObject(_objectList.SelectedName());
            }

            CancelAlertBox();
            LoadObjectList();
        }

        protected override void ToMainMenu()
        {
            // To loading state
            Game.GameStateManager.Pop();
            // To menu state
            Game.GameStateManager.Pop();

            var menuState = (MenuState)Game.GameStateManager.ActiveState;
            var gui = new WorldEditorGUI(Game, WorldSettings.Load(playingState.WorldSettingsName));
            menuState.SetGUI(gui);
        }
    }
}