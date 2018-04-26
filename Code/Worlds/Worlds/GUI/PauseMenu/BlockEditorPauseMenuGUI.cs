using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class BlockEditorPauseMenuGUI : View.GUI.GUI
    {
        private readonly BlockEditorState _blockEditorState;
        private const int LIST_HEIGHT = 150;
        private const int LIST_WIDTH = 320;

        private ButtonControl _backButton;
        private ButtonControl _toMainMenuButton;

        private BaseWorldsTextControl _nameInput;
        private ButtonControl _saveButton;
        private ListControl _blockList;

        internal BlockEditorPauseMenuGUI(WorldsGame game, BlockEditorState blockEditorState)
            : base(game)
        {
            _blockEditorState = blockEditorState;
        }

        protected override void LoadData()
        {
            base.LoadData();
            LoadBlockList();
        }

        protected override void CreateControls()
        {
            CreateBackButton();
            CreateToMainMenuButton();
            CreateQuitButton();

            CreateSaveInput();
            CreateBlockList();
        }

        private void CreateBackButton()
        {
            _backButton = new ButtonControl
            {
                Text = "Back",
                Bounds = new UniRectangle(ButtonDistanceFromRight, ButtonDistanceFromBottom, 100, 32)
            };
            _backButton.Pressed += (sender, arguments) => _blockEditorState.Resume();
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
                                  Text = "Save block",
                                  Bounds = new UniRectangle(ButtonDistanceFromRight - 25, ButtonDistanceFromBottom - 50, 120, 32)
                              };
            _saveButton.Pressed += (sender, arguments) => SaveCurrentBlock();

            _nameInput = new BaseWorldsTextControl
                             {
                                 Bounds = new UniRectangle(_saveButton.Bounds.Left - 320, ButtonDistanceFromBottom - 50, 305, 32)
                             };

            Screen.Desktop.Children.Add(_nameInput);
            Screen.Desktop.Children.Add(_saveButton);
        }

        private void CreateBlockList()
        {
            var Y = ButtonDistanceFromTop;

            _blockList = new ListControl
                              {
                                  Bounds = new UniRectangle(new UniScalar(1f, -(LIST_WIDTH + 10 + ButtonWidth + 60 + 50)),
                                                            Y, LIST_WIDTH, LIST_HEIGHT),
                                  SelectionMode = ListSelectionMode.Single
                              };

            listControls.Add(_blockList);

            var newButton = new ButtonControl
                                {
                                    Bounds = new UniRectangle(_blockList.Bounds.Right + 10, Y, ButtonWidth + 50, ButtonHeight),
                                    Text = "New block"
                                };
            newButton.Pressed += (sender, args) => NewBlock();
            pressableControls.Add(newButton);

            var loadButton = new ButtonControl
                                 {
                                     Bounds = new UniRectangle(_blockList.Bounds.Right + 10, newButton.Bounds.Bottom + 10, ButtonWidth + 50, ButtonHeight),
                                     Text = "Load block"
                                 };
            loadButton.Pressed += (sender, args) => LoadBlock();
            pressableControls.Add(loadButton);

            var deleteButton = new ButtonControl
                                   {
                                       Bounds = new UniRectangle(_blockList.Bounds.Right + 10, loadButton.Bounds.Bottom + 10, ButtonWidth + 50, ButtonHeight),
                                       Text = "Delete block"
                                   };
            deleteButton.Pressed += (sender, args) => ShowDeleteObjectAlertBox();
            pressableControls.Add(deleteButton);

            Screen.Desktop.Children.Add(_blockList);
            Screen.Desktop.Children.Add(newButton);
            Screen.Desktop.Children.Add(loadButton);
            Screen.Desktop.Children.Add(deleteButton);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Block needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in the name.");
                return false;
            }

            return true;
        }

        private void SaveCurrentBlock()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            _blockEditorState.SaveCurrentBlock(_nameInput.Text);

            LoadBlockList();
        }

        private void LoadBlockList()
        {
            LoadList(_blockList, Block.SaverHelper(_blockEditorState.WorldSettingsName));
        }

        private void NewBlock()
        {
            _blockEditorState.NewBlock();
        }

        private void LoadBlock()
        {
            if (_blockList.SelectedName() != null)
            {
                _blockEditorState.LoadBlock(_blockList.SelectedName());
            }
        }

        private void ShowDeleteObjectAlertBox()
        {
            ShowDeletionAlertBox(DeleteBlock, "Are you sure you want to delete selected block?");
        }

        private void DeleteBlock()
        {
            if (_blockList.SelectedName() != null)
            {
                Block.Delete(_blockEditorState.WorldSettingsName, _blockList.SelectedName());
            }

            CancelAlertBox();
            LoadBlockList();
        }

        protected void ToMainMenu()
        {
            // To menu state
            Game.GameStateManager.Pop();

            var menuState = (MenuState)Game.GameStateManager.ActiveState;
            var gui = new WorldEditorGUI(Game, WorldSettings.Load(_blockEditorState.WorldSettingsName));
            menuState.SetGUI(gui);
        }
    }
}