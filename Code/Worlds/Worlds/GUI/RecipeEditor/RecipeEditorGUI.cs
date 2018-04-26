using System.Collections.Generic;
using System.Linq;
using LibNoise.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.GUI.RecipeEditor;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class RecipeEditorGUI : View.GUI.GUI
    {
        private const int LIST_BUTTON_WIDTH = 70;
        private const int LIST_WIDTH = 230;

        private BaseWorldsTextControl _nameInput;
        private LabelControl _nameLabel;
        private RecipeMakerSourcePlayerControl _recipeMaker;
        private ListControl _recipeList;
        private BaseWorldsTextControl _filterInput;
        private ButtonControl _newButton;
        private ButtonControl _loadButton;
        private ButtonControl _deleteButton;
        private ItemListControl _itemsControl;

        private int _itemCellAmount;

        private int _itemRowAmount;
        private readonly ItemMousePicker _itemMousePicker;

        // All items in the world
        private List<IItemLike> _items;

        private List<string> _fullList;

        internal Recipe Recipe { get; set; }

        internal WorldSettings WorldSettings { get; set; }

        protected override string LabelText { get { return ""; } }

        protected override bool IsSaveable { get { return false; } }

        protected override bool IsBackable { get { return true; } }

        protected override int LabelWidth { get { return 100; } }

        protected override int TitleY { get { return 50; } }

        private int SecondRowX { get { return (int)Screen.Width - 100 - LIST_WIDTH - LIST_BUTTON_WIDTH - 10; } }

        private int BottomButtonY { get { return (int)Screen.Height - 100; } }

        private int FirstRowWidth { get { return SecondRowX - FirstRowLabelX; } }

        //        private int RecipeAreaLeft { get { return FirstRowLabelX + FirstRowWidth / 2 - RECIPE_AREA_WIDTH; } }

        private int ItemsAreaTop { get { return IconCellControl.ICON_CELL_SIZE * 3 + 10 + TitleY + 30 + 10 + 30 + 50; } }

        private bool IsRecipeSelected { get { return _recipeList.SelectedItems.Count > 0 && _recipeList.Items.Count > 0; } }

        private string SelectedRecipeName
        {
            get
            {
                if (!IsRecipeSelected)
                {
                    return "";
                }

                return _recipeList.Items[_recipeList.SelectedItems[0]];
            }
        }

        internal bool IsNew
        {
            get { return Recipe == null; }
        }

        internal RecipeEditorGUI(WorldsGame game, WorldSettings world)
            : base(game)
        {
            WorldSettings = world;
            _itemMousePicker = new ItemMousePicker(Game.GraphicsDevice, Game.Content);
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddRecipeAndItemPanel();
            AddRecipeList();
            AddSaveInputPanel();
        }

        protected override void LoadData()
        {
            LoadList(_recipeList, Recipe.SaverHelper(WorldSettings.Name));

            _fullList = new List<string>(_recipeList.Items);
        }

        private void LoadItems()
        {
            _items = new List<IItemLike>();
            _items.AddRange(from item in WorldSettings.Items
                            where !item.IsSystem
                            select item);
        }

        private void AddSaveInputPanel()
        {
            _nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, ButtonDistanceFromBottom, 50, LabelHeight),
                Text = "Name:"
            };

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(_nameLabel.Bounds.Right + 10, ButtonDistanceFromBottom, (_itemCellAmount - 1) * IconCellControl.ICON_CELL_SIZE - 50 - 70 - 20, 30),
            };

            if (!IsNew)
                _nameInput.Text = Recipe.Name;

            saveButton = new ButtonControl
            {
                Text = "Save",
                Bounds = new UniRectangle(_nameInput.Bounds.Right + 10, ButtonDistanceFromBottom, 70, 30)
            };
            saveButton.Pressed += (sender, args) => Save();

            pressableControls.Add(saveButton);
            Screen.Desktop.Children.Add(saveButton);

            BaseWorldsTextControls.Add(_nameInput);

            Screen.Desktop.Children.Add(_nameLabel);
            Screen.Desktop.Children.Add(_nameInput);
        }

        private void AddRecipeAndItemPanel()
        {
            var recipeLabel = new LabelControl("Recipe")
            {
                Bounds = new UniRectangle(FirstRowLabelX, TitleY, LabelWidth, LabelHeight)
            };

            int height = BottomButtonY - ItemsAreaTop - IconCellControl.ICON_CELL_SIZE * 2;

            ItemListControl.GetIconAmount(FirstRowWidth, height, out _itemCellAmount, out _itemRowAmount);

            var itemsLabel = new LabelControl("Items")
            {
                Bounds = new UniRectangle(FirstRowLabelX, ItemsAreaTop - 50, LabelWidth, LabelHeight)
            };

            LoadItems();

            _itemsControl = new ItemListControl(
                WorldSettings, this, FirstRowLabelX, ItemsAreaTop, FirstRowWidth - 50, height, _items)
            {
                HasFilterInput = true,
                HasPaging = true
            };

            _itemsControl.Initialize();
            _itemsControl.OnItemLeftClick += OnItemListClick;
            _itemsControl.OnFilterInput += FilterItems;

            _recipeMaker = new RecipeMakerSourcePlayerControl(this, FirstRowLabelX, TitleY + LabelHeight + 10, _items);
            _recipeMaker.Initialize();
            _recipeMaker.OnItemLeftClick += OnRecipeClick;

            Screen.Desktop.Children.Add(recipeLabel);
            Screen.Desktop.Children.Add(itemsLabel);
        }

        private void FilterItems(string text)
        {
            if (text == "")
            {
                _itemsControl.UpdateItems(_items);
                return;
            }

            IEnumerable<IItemLike> filteredItems = from itemLike in _items
                                                   where itemLike.Name.Contains(text)
                                                   select itemLike;

            _itemsControl.UpdateItems(filteredItems);
        }

        private void AddRecipeList()
        {
            var recipesLabel = new LabelControl("Saved recipes")
            {
                Bounds = new UniRectangle(SecondRowX, TitleY, LabelWidth, LabelHeight)
            };

            var filterLabel = new LabelControl
            {
                Bounds = new UniRectangle(SecondRowX, recipesLabel.Bounds.Bottom + 10, 40, LabelHeight),
                Text = "Filter:"
            };

            _filterInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(filterLabel.Bounds.Right + 10, recipesLabel.Bounds.Bottom + 10, LIST_WIDTH - 40 - 10, 30)
            };
            _filterInput.OnTextChanged += OnRecipeFilter;

            _recipeList = new ListControl
            {
                Bounds = new UniRectangle(SecondRowX, _filterInput.Bounds.Bottom + 20, LIST_WIDTH, 370),
                SelectionMode = ListSelectionMode.Single
            };

            _newButton = new ButtonControl
            {
                Text = "New",
                Bounds = new UniRectangle(backButton.Bounds.Left, _recipeList.Bounds.Top, LIST_BUTTON_WIDTH, 30)
            };
            _newButton.Pressed += (sender, args) => OnNew();

            _loadButton = new ButtonControl
            {
                Text = "Load",
                Bounds = new UniRectangle(backButton.Bounds.Left, _newButton.Bounds.Bottom + 10, LIST_BUTTON_WIDTH, 30)
            };
            _loadButton.Pressed += (sender, args) => OnLoad();

            _deleteButton = new ButtonControl
            {
                Text = "Delete",
                Bounds = new UniRectangle(backButton.Bounds.Left, _loadButton.Bounds.Bottom + 10, LIST_BUTTON_WIDTH, 30)
            };
            _deleteButton.Pressed += (sender, args) => OnDelete();

            Screen.Desktop.Children.Add(recipesLabel);
            Screen.Desktop.Children.Add(filterLabel);
            Screen.Desktop.Children.Add(_filterInput);
            Screen.Desktop.Children.Add(_recipeList);
            Screen.Desktop.Children.Add(_newButton);
            Screen.Desktop.Children.Add(_loadButton);
            Screen.Desktop.Children.Add(_deleteButton);
        }

        private void OnItemListClick(string name)
        {
            _itemMousePicker.PickedItem = (_itemMousePicker.PickedItem == null ? _items.Find(item => item.Name == name) : null);

            _itemsControl.UnclickEverything();
        }

        private void OnRecipeClick(string name)
        {
            if (_itemMousePicker.PickedItem != null)
            {
                _recipeMaker.SetClickedItem(_itemMousePicker.PickedItem.Name);
                _itemMousePicker.PickedItem = null;
            }
            else if (name != null)
            {
                _itemMousePicker.PickedItem = _items.Find(item => item.Name == name);
                _recipeMaker.UnsetSelected();
            }

            _recipeMaker.UnclickEverything();
        }

        private void OnRecipeFilter(string filterText)
        {
            _recipeList.Items.Clear();
            IEnumerable<string> newList = from element in _fullList where element.ToLowerInvariant().Contains(filterText.ToLowerInvariant()) select element;

            foreach (string s in newList)
            {
                _recipeList.Items.Add(s);
            }
        }

        private void OnNew()
        {
            _recipeMaker.Clear();
            _nameInput.Text = "";
        }

        private void OnLoad()
        {
            if (SelectedRecipeName != "")
            {
                _recipeMaker.Clear();

                Recipe = Recipe.SaverHelper(WorldSettings.Name).Load(SelectedRecipeName);

                foreach (KeyValuePair<int, string> item in Recipe.Items)
                {
                    int x = item.Key % Recipe.XSize;
                    int y = item.Key / Recipe.XSize;

                    _recipeMaker.SetItem(x + y * 3, item.Value);
                }

                _recipeMaker.SetResultItem(Recipe.ResultItem, Recipe.ResultItemQuantity);
            }
        }

        private void OnDelete()
        {
            if (SelectedRecipeName != "")
            {
                ShowDeletionAlertBox(DeleteSelectedRecipe, "Are you sure you want to delete selected recipe?");
            }
        }

        private void DeleteSelectedRecipe()
        {
            Recipe.Delete(WorldSettings.Name, SelectedRecipeName);
            LoadData();
            CancelAlertBox();
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Recipe needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in recipe name.");
                return false;
            }

            return true;
        }

        private Dictionary<int, string> GetRelativeItems()
        {
            return _recipeMaker.GetRelativeItems();
        }

        private string GetResultItem()
        {
            return _recipeMaker.GetResultItem();
        }

        private int GetResultQuantity()
        {
            return _recipeMaker.GetResultQuantity();
        }

        private bool IsRecipeEmpty()
        {
            if (GetRelativeItems().Count == 0)
            {
                ShowAlertBox("Recipe is empty.");
                return true;
            }

            if (string.IsNullOrEmpty(GetResultItem()))
            {
                ShowAlertBox("Recipe must produce a result");
                return true;
            }

            return false;
        }

        protected override void Save()
        {
            if (!IsNameInputOK() || IsRecipeEmpty())
            {
                return;
            }

            //            if (!IsNew && Recipe.Name != null)
            //            {
            //                Recipe.Delete();
            //            }

            Recipe = new Recipe(WorldSettings.Name)
            {
                Name = _nameInput.Text,
                Items = GetRelativeItems(),
                XSize = _recipeMaker.XSize,
                YSize = _recipeMaker.YSize,
                ResultItem = GetResultItem(),
                ResultItemQuantity = GetResultQuantity()
            };

            Recipe.Save();

            LoadData();
        }

        internal override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            _itemsControl.Draw(gameTime, spriteBatch);
            _recipeMaker.Draw(gameTime, spriteBatch);
        }

        internal override void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.DrawAfterGUI(gameTime, spriteBatch);

            _itemMousePicker.Draw(gameTime, spriteBatch);
        }

        protected override void Back()
        {
            var worldEditorGUI = new WorldEditorGUI(Game, WorldSettings);
            MenuState.SetGUI(worldEditorGUI);
        }

        public override void Dispose()
        {
            base.Dispose();
            _recipeMaker.Dispose();
            _itemsControl.Dispose();
            _itemMousePicker.Dispose();
        }
    }
}