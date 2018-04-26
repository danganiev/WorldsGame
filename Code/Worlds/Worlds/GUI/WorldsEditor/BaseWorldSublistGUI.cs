using System;
using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class BaseWorldSublistGUI<T> : BaseWorldSettingsGUI where T : class, ISaveDataSerializable<T>
    {
        protected ListControl elementList;
        protected BaseWorldsTextControl _filterInput;
        protected ButtonControl _deleteButton;

        internal Action<WorldsGame, WorldSettings> CreateAction { get; set; }

        internal Action<WorldsGame, WorldSettings, T> EditAction { get; set; }

        internal Action<WorldsGame, WorldSettings, T> DeleteAction { get; set; }

        internal Action<WorldSettings, ListControl, SaverHelper<T>> LoadListAction { get; set; }

        internal View.GUI.GUI BackGUI { get; set; }

        protected bool IsElementSelected
        {
            get { return elementList.SelectedItems.Count != 0; }
        }

        internal string Title { get; set; }

        internal string DeleteBoxText { get; set; }

        protected override string LabelText { get { return Title; } }

        protected override string DeletionText { get { return DeleteBoxText; } }

        protected override bool IsBackable { get { return true; } }

        protected override bool IsSaveable { get { return false; } }

        protected SaverHelper<T> SaverHelper { get; set; }

        protected string PreselectedValue { get; set; }

        protected virtual bool IsFilterNeeded { get { return true; } }

        private string SelectedName { get { return elementList.SelectedName(); } }

        internal IList<string> FullList { get; set; }

        internal BaseWorldSublistGUI(WorldsGame game, WorldSettings worldSettings, SaverHelper<T> saverHelper = null,
            string preselectedValue = "")
            : base(game, worldSettings)
        {
            WorldSettings = worldSettings;
            SaverHelper = saverHelper;
            PreselectedValue = preselectedValue;
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddListPanel();
        }

        internal void LoadDataFromOutside()
        {
            LoadData();
        }

        protected override void LoadData()
        {
            elementList.SelectedItems.Clear();
            if (LoadListAction == null)
            {
                LoadList(elementList, SaverHelper, preselectedValue: PreselectedValue);
            }
            else
            {
                LoadListAction(WorldSettings, elementList, SaverHelper);
            }

            FullList = new List<string>(elementList.Items);
        }

        protected virtual void LoadDataAndLeaveSelected()
        {
            if (IsElementSelected)
            {
                int selectedIndex = elementList.SelectedItems[0];
                LoadData();
                elementList.SelectedItems.Add(selectedIndex);
            }
            else
            {
                LoadData();
            }
        }

        protected void FilterData(string filterText)
        {
            elementList.Items.Clear();
            IEnumerable<string> newList = from element in FullList where element.ToLowerInvariant().Contains(filterText.ToLowerInvariant()) select element;

            foreach (string s in newList)
            {
                elementList.Items.Add(s);
            }
        }

        protected virtual void AddListPanel()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            var filterLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, 40, LabelHeight),
                Text = "Filter:"
            };

            _filterInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(filterLabel.Bounds.Right + 10, Y, 230 - 40 - 10, 30)
            };
            _filterInput.OnTextChanged += FilterData;

            elementList = new ListControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, _filterInput.Bounds.Bottom + 10, 230, 370),
                SelectionMode = ListSelectionMode.Single
            };

            listControls.Add(elementList);

            const int halfFilterWidth = 75;
            var createButton = new ButtonControl
            {
                Bounds = new UniRectangle(elementList.Bounds.Right + 10, elementList.Bounds.Top, halfFilterWidth, ButtonHeight),
                Text = "Create"
            };
            createButton.Pressed += (sender, args) => Create();
            pressableControls.Add(createButton);

            var buttonX = elementList.Bounds.Right + 10;

            var editButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, createButton.Bounds.Bottom + 10, halfFilterWidth, ButtonHeight),
                Text = "Edit"
            };
            editButton.Pressed += (sender, args) => Edit();
            pressableControls.Add(editButton);

            _deleteButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, editButton.Bounds.Bottom + 10, halfFilterWidth, ButtonHeight),
                Text = "Delete"
            };
            _deleteButton.Pressed += (sender, args) => ShowDeletionAlertBox();
            pressableControls.Add(_deleteButton);

            Screen.Desktop.Children.Add(elementList);
            Screen.Desktop.Children.Add(createButton);
            Screen.Desktop.Children.Add(editButton);
            Screen.Desktop.Children.Add(_deleteButton);

            if (IsFilterNeeded)
            {
                Screen.Desktop.Children.Add(filterLabel);
                Screen.Desktop.Children.Add(_filterInput);
            }
        }

        protected virtual void Create()
        {
            CreateAction(Game, WorldSettings);
        }

        protected virtual void Edit()
        {
            if (!IsElementSelected)
                return;

            T selectedElement = SaverHelper.Load(SelectedName);

            EditAction(Game, WorldSettings, selectedElement);
        }

        protected override void ShowDeletionAlertBox(Action deleteAction = null, string deletionText = "")
        {
            if (!IsElementSelected)
                return;

            base.ShowDeletionAlertBox(deleteAction);
        }

        protected override void Delete()
        {
            if (!IsElementSelected)
                return;

            T element = SaverHelper.Load(SelectedName);
            if (DeleteAction == null)
            {
                element.Delete();
            }
            else
            {
                DeleteAction(Game, WorldSettings, element);
            }

            CancelAlertBox();
            LoadData();
        }

        protected override void Back()
        {
            if (_filterInput != null)
            {
                _filterInput.OnTextChanged -= FilterData;
            }

            var worldEditorGUI = new WorldEditorGUI(Game, WorldSettings);
            MenuState.SetGUI(worldEditorGUI);
        }
    }
}