using System.Collections.Generic;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

//using Character = WorldsGame.Saving.DataClasses.Character;

namespace WorldsGame.GUI
{
    internal class CharacterEditorPauseMenuGUI : View.GUI.GUI
    {
        private readonly CharacterEditorState _characterEditorState;
        private const int LIST_HEIGHT = 150;
        private const int LIST_WIDTH = 280;

        private ButtonControl _backButton;
        private ButtonControl _toMainMenuButton;

        private BaseWorldsTextControl _nameInput;
        private ButtonControl _saveButton;
        private ListControl _characterList;
        private List<string> _characterNamesList;

        internal CharacterEditorPauseMenuGUI(WorldsGame game, CharacterEditorState characterEditorState)
            : base(game)
        {
            _characterEditorState = characterEditorState;
        }

        protected override void LoadData()
        {
            base.LoadData();
            LoadCharacterList();
        }

        protected override void CreateControls()
        {
            CreateBackButton();
            CreateToMainMenuButton();
            CreateQuitButton();

            CreateSaveInput();
            CreateCharacterList();
        }

        private void CreateBackButton()
        {
            _backButton = new ButtonControl
                              {
                                  Text = "Back",
                                  Bounds = new UniRectangle(ButtonDistanceFromRight, ButtonDistanceFromBottom, 100, 32)
                              };
            _backButton.Pressed += (sender, arguments) => _characterEditorState.Resume();
            Screen.Desktop.Children.Add(_backButton);
        }

        //        protected virtual void CreateToMainMenuButton()
        //        {
        //            _toMainMenuButton = new ButtonControl
        //            {
        //                Text = "Exit to Main Menu",
        //                Bounds = new UniRectangle(_backButton.Bounds.Left - 165, ButtonDistanceFromBottom, 155, 32)
        //            };
        //            _toMainMenuButton.Pressed += (sender, arguments) => ToMainMenu();
        //            Screen.Desktop.Children.Add(_toMainMenuButton);
        //        }

        private void CreateQuitButton()
        {
            var quitButton = new ButtonControl
                                 {
                                     Text = "Exit to Desktop",
                                     Bounds = new UniRectangle(_toMainMenuButton.Bounds.Left - 160, ButtonDistanceFromBottom, 150, 32)
                                 };
            quitButton.Pressed += (sender, arguments) => Game.Exit();
            Screen.Desktop.Children.Add(quitButton);
        }

        internal virtual void Stop()
        {
            Screen.Desktop.Children.Clear();
        }

        protected void CreateToMainMenuButton()
        {
            _toMainMenuButton = new ButtonControl
                                    {
                                        Text = "Back to world menu",
                                        Bounds = new UniRectangle(_backButton.Bounds.Left - 185, ButtonDistanceFromBottom, 175, 32)
                                    };
            _toMainMenuButton.Pressed += (sender, arguments) => ToMainMenu();
            Screen.Desktop.Children.Add(_toMainMenuButton);
        }

        private void CreateSaveInput()
        {
            _saveButton = new ButtonControl
                              {
                                  Text = "Save character",
                                  Bounds = new UniRectangle(ButtonDistanceFromRight - 35, ButtonDistanceFromBottom - 50, ButtonWidth + 70, 32)
                              };
            _saveButton.Pressed += (sender, arguments) => SaveCurrentCharacter();

            _nameInput = new BaseWorldsTextControl
                             {
                                 Bounds = new UniRectangle(_saveButton.Bounds.Left - 310, ButtonDistanceFromBottom - 50, 300, 32)
                             };

            Screen.Desktop.Children.Add(_nameInput);
            Screen.Desktop.Children.Add(_saveButton);
        }

        private void CreateCharacterList()
        {
            var Y = ButtonDistanceFromTop;

            _characterList = new ListControl
                             {
                                 Bounds = new UniRectangle(new UniScalar(1f, -(LIST_WIDTH + 10 + ButtonWidth + 80 + 65)),
                                                           Y, LIST_WIDTH, LIST_HEIGHT),
                                 SelectionMode = ListSelectionMode.Single
                             };

            listControls.Add(_characterList);

            var newButton = new ButtonControl
                                {
                                    Bounds = new UniRectangle(_characterList.Bounds.Right + 10, Y, ButtonWidth + 90, ButtonHeight),
                                    Text = "New character"
                                };
            newButton.Pressed += (sender, args) => NewCharacter();
            pressableControls.Add(newButton);

            var loadButton = new ButtonControl
                                 {
                                     Bounds = new UniRectangle(_characterList.Bounds.Right + 10, newButton.Bounds.Bottom + 10, ButtonWidth + 90, ButtonHeight),
                                     Text = "Load character"
                                 };
            loadButton.Pressed += (sender, args) => LoadCharacter();
            pressableControls.Add(loadButton);

            var deleteButton = new ButtonControl
                                   {
                                       Bounds = new UniRectangle(_characterList.Bounds.Right + 10, loadButton.Bounds.Bottom + 10, ButtonWidth + 90, ButtonHeight),
                                       Text = "Delete character"
                                   };
            deleteButton.Pressed += (sender, args) => ShowDeleteCharacterAlertBox();
            pressableControls.Add(deleteButton);

            var defaultPlayerButton = new ButtonControl
            {
                Bounds = new UniRectangle(_characterList.Bounds.Right + 10, deleteButton.Bounds.Bottom + 10, ButtonWidth + 90, ButtonHeight),
                Text = "Create default player"
            };
            defaultPlayerButton.Pressed += (sender, args) => CreateDefaultPlayer();
            pressableControls.Add(defaultPlayerButton);

            Screen.Desktop.Children.Add(_characterList);
            Screen.Desktop.Children.Add(newButton);
            Screen.Desktop.Children.Add(loadButton);
            Screen.Desktop.Children.Add(deleteButton);
            Screen.Desktop.Children.Add(defaultPlayerButton);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Character needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin letters, numbers, underscore and whitespace \n" +
                             "are allowed in the name.");
                return false;
            }

            return true;
        }

        private void SaveCurrentCharacter()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            _characterEditorState.SaveCurrentCharacter(_nameInput.Text);

            LoadCharacterList();
        }

        private void LoadCharacterList()
        {
            _characterNamesList = Character.SaverHelper(_characterEditorState.WorldSettingsName).LoadNames();
            LoadList(_characterList, Character.SaverHelper(_characterEditorState.WorldSettingsName));
        }

        private void NewCharacter()
        {
            _characterEditorState.NewCharacter();
        }

        private void LoadCharacter()
        {
            if (_characterList.SelectedName() != null)
            {
                string name = _characterList.SelectedName();
                _characterEditorState.LoadCharacter(name);
                _nameInput.Text = name;
            }
        }

        private void ShowDeleteCharacterAlertBox()
        {
            ShowDeletionAlertBox(DeleteCharacter, "Are you sure you want to delete selected character?");
        }

        private void CreateDefaultPlayer()
        {
            if (_characterNamesList.Contains("Player"))
            {
                ShowAlertBox("Player character already exists.");
                return;
            }

            Character.CreateDefaultPlayer(
                _characterEditorState.WorldSettingsName, _characterEditorState.GetContentManager(), Game.GraphicsDevice);
        }

        private void DeleteCharacter()
        {
            if (_characterList.SelectedName() != null)
            {
                Character.Delete(_characterEditorState.WorldSettingsName, _characterList.SelectedName());
            }

            CancelAlertBox();
            LoadCharacterList();
        }

        protected void ToMainMenu()
        {
            // To menu state
            Game.GameStateManager.Pop();

            var menuState = (MenuState)Game.GameStateManager.ActiveState;
            var gui = new WorldEditorGUI(Game, WorldSettings.Load(_characterEditorState.WorldSettingsName));
            menuState.SetGUI(gui);
        }
    }
}