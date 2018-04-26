using System.Collections.Generic;
using System.Linq;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls.Desktop;
using WorldsGame.Saving;

namespace WorldsGame.GUI
{
    internal class RuleSublistGUI : BaseWorldSublistGUI<Rule>
    {
        private readonly Dictionary<string, Rule> _loadedRules = new Dictionary<string, Rule>();

        private int ListButtonHeight { get { return 25; } }

        private int DistanceBetweenListButtons { get { return 5; } }

        private Rule SelectedRule
        {
            get
            {
                if (!IsElementSelected)
                {
                    return null;
                }

                string selectedRuleName = elementList.Items[elementList.SelectedItems[0]];

                if (!_loadedRules.ContainsKey(selectedRuleName))
                {
                    return null;
                }

                Rule rule = _loadedRules[selectedRuleName];

                return rule;
            }
        }

        internal RuleSublistGUI(
            WorldsGame game, WorldSettings worldSettings, SaverHelper<Rule> saverHelper = null, string preselectedValue = "")
            : base(game, worldSettings, saverHelper, preselectedValue)
        {
        }

        protected override void AddListPanel()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            elementList = new ListControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, 230f, 370f),
                SelectionMode = ListSelectionMode.Single
            };

            listControls.Add(elementList);

            var createButton = new ButtonControl
            {
                Bounds = new UniRectangle(elementList.Bounds.Right + 10, Y, ButtonWidth, ListButtonHeight),
                Text = "Create"
            };
            createButton.Pressed += (sender, args) => Create();
            pressableControls.Add(createButton);

            var buttonX = elementList.Bounds.Right + 10;

            var editButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, createButton.Bounds.Bottom + DistanceBetweenListButtons, ButtonWidth, ListButtonHeight),
                Text = "Edit"
            };
            editButton.Pressed += (sender, args) => Edit();
            pressableControls.Add(editButton);

            var deleteButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, editButton.Bounds.Bottom + DistanceBetweenListButtons, ButtonWidth, ListButtonHeight),
                Text = "Delete"
            };
            deleteButton.Pressed += (sender, args) => ShowDeletionAlertBox();
            pressableControls.Add(deleteButton);

            var upButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, deleteButton.Bounds.Bottom + DistanceBetweenListButtons, ButtonWidth, ListButtonHeight),
                Text = "Up"
            };
            upButton.Pressed += (sender, args) => RuleUp();
            pressableControls.Add(upButton);

            var downButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, upButton.Bounds.Bottom + DistanceBetweenListButtons, ButtonWidth, ListButtonHeight),
                Text = "Down"
            };
            downButton.Pressed += (sender, args) => RuleDown();
            pressableControls.Add(downButton);

            Screen.Desktop.Children.Add(elementList);
            Screen.Desktop.Children.Add(createButton);
            Screen.Desktop.Children.Add(editButton);
            Screen.Desktop.Children.Add(deleteButton);
            Screen.Desktop.Children.Add(upButton);
            Screen.Desktop.Children.Add(downButton);
        }

        protected override void Edit()
        {
            if (!IsElementSelected)
                return;

            EditAction(Game, WorldSettings, SelectedRule);
        }

        protected override void Delete()
        {
            if (!IsElementSelected)
                return;

            WorldSettings.DeleteRule(SelectedRule);

            CancelAlertBox();
            LoadData();
        }

        private void RuleUp()
        {
            if (!IsElementSelected)
            {
                return;
            }
            int currentPriority = WorldSettings.SubrulePriorities[SelectedRule.Guid];

            if (currentPriority == 1)
            {
                return;
            }

            Rule upperRule = (from l in _loadedRules
                              where WorldSettings.SubrulePriorities[l.Value.Guid] == currentPriority - 1
                              select l.Value).First();

            WorldSettings.SubruleGuids[currentPriority] = upperRule.Guid;
            WorldSettings.SubruleGuids[currentPriority - 1] = SelectedRule.Guid;

            WorldSettings.SubrulePriorities[upperRule.Guid] = currentPriority;
            WorldSettings.SubrulePriorities[SelectedRule.Guid] = currentPriority - 1;

            WorldSettings.Save();

            elementList.SelectedItems[0] = elementList.SelectedItems[0] - 1;

            LoadDataAndLeaveSelected();
        }

        private void RuleDown()
        {
            if (!IsElementSelected)
            {
                return;
            }
            int currentPriority = WorldSettings.SubrulePriorities[SelectedRule.Guid];

            if (currentPriority == _loadedRules.Count)
            {
                return;
            }

            Rule lowerRule = (from l in _loadedRules
                              where WorldSettings.SubrulePriorities[l.Value.Guid] == currentPriority + 1
                              select l.Value).First();

            WorldSettings.SubruleGuids[currentPriority] = lowerRule.Guid;
            WorldSettings.SubruleGuids[currentPriority + 1] = SelectedRule.Guid;

            WorldSettings.SubrulePriorities[lowerRule.Guid] = currentPriority;
            WorldSettings.SubrulePriorities[SelectedRule.Guid] = currentPriority + 1;

            WorldSettings.Save();

            elementList.SelectedItems[0] = elementList.SelectedItems[0] + 1;

            LoadDataAndLeaveSelected();
        }

        protected override void LoadData()
        {
            elementList.Items.Clear();
            elementList.SelectedItems.Clear();
            _loadedRules.Clear();

            foreach (Rule rule in WorldSettings.Rules)
            {
                if (!_loadedRules.ContainsKey(rule.Name))
                {
                    _loadedRules.Add(rule.Name, rule);
                    elementList.Items.Add(rule.Name);
                }
                else
                {
                    // Users can create rules with the same names because of guids
                    WorldSettings.DeleteRule(rule);
                }
            }

            SelectRule();
        }

        private void SelectRule()
        {
            foreach (string elementName in elementList.Items)
            {
                if (elementName == PreselectedValue)
                {
                    int index = elementList.Items.Count - 1;
                    elementList.SelectedItems.Add(index);
                }
            }
        }
    }
}