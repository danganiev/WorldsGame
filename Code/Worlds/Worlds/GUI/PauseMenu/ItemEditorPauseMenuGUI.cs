using System;
using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Gamestates;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class ItemEditorPauseMenuGUI : View.GUI.GUI
    {
        private readonly ItemEditorState _itemEditorState;
        private const int LIST_HEIGHT = 150;
        private const int LIST_WIDTH = 320;

        private ButtonControl _backButton;
        private ButtonControl _toMainMenuButton;

        private BaseWorldsTextControl _nameInput;
        private ButtonControl _saveButton;
        private ListControl _itemList;

        internal ItemEditorPauseMenuGUI(WorldsGame game, ItemEditorState itemEditorState)
            : base(game)
        {
            _itemEditorState = itemEditorState;
        }

        protected override void LoadData()
        {
            base.LoadData();
            LoadItemList();
        }

        protected override void CreateControls()
        {
            CreateBackButton();
            CreateToMainMenuButton();
            CreateQuitButton();

            CreateSaveInput();
            CreateItemList();
        }

        private void CreateBackButton()
        {
            _backButton = new ButtonControl
                          {
                              Text = "Back",
                              Bounds = new UniRectangle(ButtonDistanceFromRight, ButtonDistanceFromBottom, 100, 32)
                          };
            _backButton.Pressed += (sender, arguments) => _itemEditorState.Resume();
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
                              Text = "Save item",
                              Bounds = new UniRectangle(ButtonDistanceFromRight - 30, ButtonDistanceFromBottom - 50, 130, 32)
                          };
            _saveButton.Pressed += (sender, arguments) => SaveCurrentItem();

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(_saveButton.Bounds.Left - 315, ButtonDistanceFromBottom - 50, 305, 32),
                Text = _itemEditorState.ItemName
            };

            Screen.Desktop.Children.Add(_nameInput);
            Screen.Desktop.Children.Add(_saveButton);
        }

        private void CreateItemList()
        {
            var Y = ButtonDistanceFromTop;

            _itemList = new ListControl
                             {
                                 Bounds = new UniRectangle(new UniScalar(1f, -(LIST_WIDTH + 10 + ButtonWidth + 60 + 50)),
                                                           Y, LIST_WIDTH, LIST_HEIGHT),
                                 SelectionMode = ListSelectionMode.Single
                             };

            listControls.Add(_itemList);

            var newButton = new ButtonControl
                            {
                                Bounds = new UniRectangle(_itemList.Bounds.Right + 10, Y, ButtonWidth + 50, ButtonHeight),
                                Text = "New item"
                            };
            newButton.Pressed += (sender, args) => NewItem();
            pressableControls.Add(newButton);

            var loadButton = new ButtonControl
                             {
                                 Bounds = new UniRectangle(_itemList.Bounds.Right + 10, newButton.Bounds.Bottom + 10, ButtonWidth + 50, ButtonHeight),
                                 Text = "Load item"
                             };
            loadButton.Pressed += (sender, args) => LoadItem();
            pressableControls.Add(loadButton);

            var deleteButton = new ButtonControl
                               {
                                   Bounds = new UniRectangle(_itemList.Bounds.Right + 10, loadButton.Bounds.Bottom + 10, ButtonWidth + 50, ButtonHeight),
                                   Text = "Delete item"
                               };
            deleteButton.Pressed += (sender, args) => ShowDeleteItemAlertBox();
            pressableControls.Add(deleteButton);

            Screen.Desktop.Children.Add(_itemList);
            Screen.Desktop.Children.Add(newButton);
            Screen.Desktop.Children.Add(loadButton);
            Screen.Desktop.Children.Add(deleteButton);
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Item needs a name!");
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

        private void SaveCurrentItem()
        {
            if (!IsNameInputOK())
            {
                return;
            }

            _itemEditorState.SaveCurrentItem(_nameInput.Text);

            LoadItemList();
        }

        private void LoadItemList()
        {
            IEnumerable<string> items = from item in Item.SaverHelper(_itemEditorState.WorldSettingsName).LoadList()
                                        where !item.IsSystem && !item.IsBlock
                                        select item.Name;

            LoadList(_itemList, items);
        }

        private void NewItem()
        {
        }

        private void LoadItem()
        {
            if (_itemList.SelectedName() != null)
            {
                string name = _itemList.SelectedName();
                _itemEditorState.LoadItem(name);

                _nameInput.Text = name;
            }
        }

        private void ShowDeleteItemAlertBox()
        {
            ShowDeletionAlertBox(DeleteItem, "Are you sure you want to delete selected item?");
        }

        private void DeleteItem()
        {
            if (_itemList.SelectedName() != null)
            {
                Item.Delete(_itemEditorState.WorldSettingsName, _itemList.SelectedName());
            }

            CancelAlertBox();
            LoadItemList();
        }

        protected void ToMainMenu()
        {
            // To menu state
            Game.GameStateManager.Pop();

            var menuState = (MenuState)Game.GameStateManager.ActiveState;
            var gui = new WorldEditorGUI(Game, WorldSettings.Load(_itemEditorState.WorldSettingsName));
            menuState.SetGUI(gui);
        }
    }
}