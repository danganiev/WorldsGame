using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.ExtensionMethods;
using WorldsGame.Utils.UIControls;

namespace WorldsGame.GUI
{
    internal class RuleEditorGUI : View.GUI.GUI
    {
        private const string SYSTEM_AIR_NAME = "System: Air";
        private const string ACTION_BLOCK = "Add block";
        private const string ACTION_SUBRULE = "Use subrules";
        private const string ACTION_OBJECT = "Add object";
        private const string ACTION_SPAWN = "Add spawn data";
        private const int CONTROL_WIDTH = 340;

        private BaseWorldsTextControl _nameInput;
        private BaseWorldsTextControl _conditionInput;

        private ListControl _actionSelectList;
        private ListControl _subrulesList;
        private ListControl _blocksList;
        private ListControl _objectsList;
        private ListControl _npcList;

        private Task _changeActionTask;

        private readonly Dictionary<string, Rule> _loadedSubrules = new Dictionary<string, Rule>();
        private readonly List<Control> _subrulesControlsGroup = new List<Control>();
        private readonly List<Control> _blockControlsGroup = new List<Control>();
        private readonly List<Control> _objectControlsGroup = new List<Control>();
        private readonly List<Control> _spawnControlsGroup = new List<Control>();
        private NumberInputControl _spawnRateControl;

        internal WorldSettings WorldSettings { get; private set; }

        internal IRuleHolder Parent { get; private set; }

        internal Rule Rule { get; set; }

        internal int HierarchyLevel { get; set; }

        private int ListButtonHeight { get { return 25; } }

        private int DistanceBetweenListButtons { get { return 5; } }

        internal SaverHelper<Rule> RuleSaverHelper { get { return new SaverHelper<Rule>(Rule.StaticContainerName) { DirectoryRelativePath = WorldSettings.Name }; } }

        internal bool IsNew
        {
            get { return Rule == null; }
        }

        private bool IsSubruleSelected
        {
            get { return _subrulesList.SelectedItems.Count != 0; }
        }

        private Rule SelectedSubrule
        {
            get
            {
                string selectedSubruleName = _subrulesList.Items[_subrulesList.SelectedItems[0]];

                if (!_loadedSubrules.ContainsKey(selectedSubruleName))
                {
                    return null;
                }

                Rule rule = _loadedSubrules[selectedSubruleName];

                return rule;
            }
        }

        private bool IsBlockSelected
        {
            get { return _blocksList.IsSelected(); }
        }

        private bool IsObjectSelected
        {
            get { return _objectsList.IsSelected(); }
        }

        private bool IsCharacterSelected
        {
            get { return _npcList.IsSelected(); }
        }

        private string SelectedAction
        {
            get { return _actionSelectList.SelectedName(); }
        }

        protected override string LabelText { get { return IsNew ? "Create rule" : "Edit rule"; } }

        protected override bool IsSaveable { get { return true; } }

        protected override bool IsBackable { get { return true; } }

        internal RuleEditorGUI(WorldsGame game, WorldSettings world)
            : base(game)
        {
            WorldSettings = world;
            HierarchyLevel = 1;
            Parent = world;
        }

        internal RuleEditorGUI(WorldsGame game, WorldSettings world, IRuleHolder parent)
            : base(game)
        {
            WorldSettings = world;
            HierarchyLevel = parent.HierarchyLevel + 1;
            Parent = parent;
        }

        internal RuleEditorGUI(WorldsGame game, WorldSettings world, IRuleHolder parent, Rule rule)
            : base(game)
        {
            WorldSettings = world;
            Rule = rule;
            HierarchyLevel = parent.HierarchyLevel + 1;
            Parent = parent;
        }

        internal RuleEditorGUI(WorldsGame game, WorldSettings world, Guid guid)
            : base(game)
        {
            WorldSettings = world;

            Rule = Rule.Load(world.Name, guid);
            HierarchyLevel = Rule.HierarchyLevel;

            if (HierarchyLevel > 1)
            {
                Rule parentRule = Rule.Load(world.Name, Rule.ParentRuleGuid);
                Parent = parentRule;
            }
            else
            {
                Parent = world;
            }
        }

        protected override void CreateControls()
        {
            base.CreateControls();

            AddName();
            AddCondition();
            AddActionsList();
            AddBlockSelectionList();
            AddSubrulesList();
            AddObjectList();
            AddSpawnList();
        }

        protected override void LoadData()
        {
            if (IsNew)
            {
                _actionSelectList.SelectedItems.Add(0);
            }
            else
            {
                switch (Rule.ActionType)
                {
                    case RuleActionsEnum.PlaceBlock:
                        _actionSelectList.SelectedItems.Add(0);
                        break;

                    case RuleActionsEnum.UseSubrules:
                        _actionSelectList.SelectedItems.Add(1);
                        break;

                    case RuleActionsEnum.PlaceObject:
                        _actionSelectList.SelectedItems.Add(2);
                        break;
                    case RuleActionsEnum.AddSpawnData:
                        _actionSelectList.SelectedItems.Add(3);
                        break;
                }
            }

            LoadSubrules();
        }

        private void AddName()
        {
            var Y = titleLabel.Bounds.Bottom + 50;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Name:"
            };

            _nameInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, CONTROL_WIDTH, 30),
            };

            if (!IsNew)
                _nameInput.Text = Rule.Name;

            BaseWorldsTextControls.Add(_nameInput);

            Screen.Desktop.Children.Add(nameLabel);
            Screen.Desktop.Children.Add(_nameInput);
        }

        private void AddCondition()
        {
            UniScalar Y = _nameInput.Bounds.Bottom + 10;

            var conditionLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Rule condition:",
            };

            // TODO: There is no TextArea control right now
            _conditionInput = new BaseWorldsTextControl
            {
                Bounds = new UniRectangle(conditionLabel.Bounds.Right + 10, Y, CONTROL_WIDTH, 30),
            };

            if (!IsNew)
                _conditionInput.Text = Rule.ConditionText;

            BaseWorldsTextControls.Add(_conditionInput);

            Screen.Desktop.Children.Add(conditionLabel);
            Screen.Desktop.Children.Add(_conditionInput);
        }

        private void AddActionsList()
        {
            var Y = _conditionInput.Bounds.Bottom + 10;

            var label = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Action:"
            };

            _actionSelectList = new ListControl
            {
                Bounds = new UniRectangle(label.Bounds.Right + 10, Y, CONTROL_WIDTH, 70),
                SelectionMode = ListSelectionMode.Single
            };

            _actionSelectList.Items.Add(ACTION_BLOCK);
            _actionSelectList.Items.Add(ACTION_SUBRULE);
            _actionSelectList.Items.Add(ACTION_OBJECT);
            _actionSelectList.Items.Add(ACTION_SPAWN);

            _actionSelectList.SelectionChanged += (sender, args) =>
            {
                if (_changeActionTask == null || _changeActionTask.Status == TaskStatus.RanToCompletion)
                {
                    _changeActionTask = Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(THREAD_LIST_WAIT_TIME);

                        switch (SelectedAction)
                        {
                            case ACTION_BLOCK:
                                ClearControlGroup(_subrulesControlsGroup);
                                ClearControlGroup(_objectControlsGroup);
                                ClearControlGroup(_spawnControlsGroup);
                                AddControlGroup(_blockControlsGroup);
                                break;

                            case ACTION_SUBRULE:
                                ClearControlGroup(_blockControlsGroup);
                                ClearControlGroup(_objectControlsGroup);
                                ClearControlGroup(_spawnControlsGroup);
                                AddControlGroup(_subrulesControlsGroup);
                                break;

                            case ACTION_OBJECT:
                                ClearControlGroup(_blockControlsGroup);
                                ClearControlGroup(_subrulesControlsGroup);
                                ClearControlGroup(_spawnControlsGroup);
                                AddControlGroup(_objectControlsGroup);
                                break;
                            case ACTION_SPAWN:
                                ClearControlGroup(_blockControlsGroup);
                                ClearControlGroup(_subrulesControlsGroup);
                                ClearControlGroup(_objectControlsGroup);
                                AddControlGroup(_spawnControlsGroup);
                                break;
                        }
                    });
                }
            };

            Screen.Desktop.Children.Add(label);
            Screen.Desktop.Children.Add(_actionSelectList);
        }

        private void ClearControlGroup(IEnumerable<Control> group)
        {
            foreach (Control control in group)
            {
                Screen.Desktop.Children.Remove(control);
            }
        }

        private void AddControlGroup(IEnumerable<Control> group)
        {
            foreach (Control control in group)
            {
                if (control.Parent == null)
                {
                    Screen.Desktop.Children.Add(control);
                }
            }
        }

        private void AddBlockSelectionList()
        {
            _blockControlsGroup.Clear();

            var Y = _actionSelectList.Bounds.Bottom + 10;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Select block:"
            };

            _blocksList = new ListControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, CONTROL_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            string preselectedBlockName = "";

            if (!IsNew && Rule.ActionType == RuleActionsEnum.PlaceBlock)
            {
                preselectedBlockName = Rule.BlockName;

                if (preselectedBlockName == BlockTypeHelper.AIR_BLOCK_TYPE.Name)
                {
                    preselectedBlockName = SYSTEM_AIR_NAME;
                }
            }

            LoadList(_blocksList, Block.SaverHelper(WorldSettings.Name), preselectedValue: preselectedBlockName,
                additionalItems: new List<string> { SYSTEM_AIR_NAME });

            _blockControlsGroup.Add(nameLabel);
            _blockControlsGroup.Add(_blocksList);
        }

        private void AddSubrulesList()
        {
            var Y = _actionSelectList.Bounds.Bottom + 10;
            _subrulesControlsGroup.Clear();

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Subrules:"
            };

            _subrulesList = new ListControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, CONTROL_WIDTH, ListHeight + 30),
                SelectionMode = ListSelectionMode.Single
            };

            var buttonX = _subrulesList.Bounds.Right + 10;

            var createButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, Y, 65f, ListButtonHeight),
                Text = "Create"
            };
            createButton.Pressed += (sender, args) => NewSubrule();
            pressableControls.Add(createButton);

            var editButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, createButton.Bounds.Bottom + DistanceBetweenListButtons, 65, ListButtonHeight),
                Text = "Edit"
            };
            editButton.Pressed += (sender, args) => EditSubrule();
            pressableControls.Add(editButton);

            var deleteButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, editButton.Bounds.Bottom + DistanceBetweenListButtons, 65, ListButtonHeight),
                Text = "Delete"
            };
            deleteButton.Pressed += (sender, args) =>
            {
                if (IsSubruleSelected)
                {
                    ShowDeletionAlertBox(deleteAction: DeleteSubrule, deletionText: "Delete subrule?");
                }
            };
            pressableControls.Add(deleteButton);

            var upButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, deleteButton.Bounds.Bottom + DistanceBetweenListButtons, 65, ListButtonHeight),
                Text = "Up"
            };
            upButton.Pressed += (sender, args) => RuleUp();
            pressableControls.Add(upButton);

            var downButton = new ButtonControl
            {
                Bounds = new UniRectangle(buttonX, upButton.Bounds.Bottom + DistanceBetweenListButtons, 65, ListButtonHeight),
                Text = "Down"
            };
            downButton.Pressed += (sender, args) => RuleDown();
            pressableControls.Add(downButton);

            _subrulesControlsGroup.Add(nameLabel);
            _subrulesControlsGroup.Add(_subrulesList);
            _subrulesControlsGroup.Add(createButton);
            _subrulesControlsGroup.Add(editButton);
            _subrulesControlsGroup.Add(deleteButton);
            _subrulesControlsGroup.Add(upButton);
            _subrulesControlsGroup.Add(downButton);
        }

        private void AddObjectList()
        {
            _objectControlsGroup.Clear();

            var Y = _actionSelectList.Bounds.Bottom + 10;

            var nameLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Select object:"
            };

            _objectsList = new ListControl
            {
                Bounds = new UniRectangle(nameLabel.Bounds.Right + 10, Y, CONTROL_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            string preselectedObjectName = "";

            if (!IsNew && Rule.ActionType == RuleActionsEnum.PlaceObject)
            {
                preselectedObjectName = Rule.ObjectName;
            }

            LoadList(_objectsList, GameObject.SaverHelper(WorldSettings.Name), preselectedValue: preselectedObjectName);

            _objectControlsGroup.Add(nameLabel);
            _objectControlsGroup.Add(_objectsList);
        }

        private void AddSpawnList()
        {
            _spawnControlsGroup.Clear();

            var Y = _actionSelectList.Bounds.Bottom + 10;

            var npcLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, Y, LabelWidth, LabelHeight),
                Text = "Select NPC:"
            };

            _npcList = new ListControl
            {
                Bounds = new UniRectangle(npcLabel.Bounds.Right + 10, Y, CONTROL_WIDTH, ListHeight),
                SelectionMode = ListSelectionMode.Single
            };

            string preselectedName = "";

            if (!IsNew && Rule.ActionType == RuleActionsEnum.PlaceObject)
            {
                preselectedName = Rule.CharacterName;
            }

            IEnumerable<string> npcs = Character.SaverHelper(WorldSettings.Name).LoadList().Where(character => character.Name != "Player").Select(character => character.Name);

            LoadList(_npcList, npcs, preselectedValue: preselectedName);

            var spawnRateLabel = new LabelControl
            {
                Bounds = new UniRectangle(FirstRowLabelX, _npcList.Bounds.Bottom + 10, LabelWidth, LabelHeight),
                Text = "Spawn rate:",
                PopupText = "Probability of NPC to spawn every second (0-100%)"
            };

            _spawnRateControl = new NumberInputControl
            {
                Bounds = new UniRectangle(spawnRateLabel.Bounds.Right + 10,
                                            _npcList.Bounds.Bottom + 10, LabelWidth, LabelHeight),
                MaxValue = 100,
                MinValue = 0,
                IsPositiveOnly = true
            };

            _spawnControlsGroup.Add(npcLabel);
            _spawnControlsGroup.Add(_npcList);
            _spawnControlsGroup.Add(spawnRateLabel);
            _spawnControlsGroup.Add(_spawnRateControl);
        }

        private void LoadSubrules()
        {
            if (IsNew)
            {
                return;
            }

            _subrulesList.Items.Clear();
            _loadedSubrules.Clear();

            foreach (Rule subrule in Rule.Subrules)
            {
                if (!_loadedSubrules.ContainsKey(subrule.Name))
                {
                    _loadedSubrules.Add(subrule.Name, subrule);
                    _subrulesList.Items.Add(subrule.Name);
                }
                else
                {
                    Rule.DeleteSubrule(subrule);
                }
            }
        }

        private void NewSubrule()
        {
            if (!IsNew || IsNew && IsSaved())
            {
                var ruleGUI = new RuleEditorGUI(Game, WorldSettings, Rule);
                MenuState.SetGUI(ruleGUI);
            }
        }

        private void EditSubrule()
        {
            if (!IsSubruleSelected)
            {
                return;
            }

            var ruleGUI = new RuleEditorGUI(Game, WorldSettings, Rule, SelectedSubrule);
            MenuState.SetGUI(ruleGUI);
        }

        private void DeleteSubrule()
        {
            Rule.DeleteSubrule(SelectedSubrule);

            LoadSubrules();

            CancelAlertBox();
        }

        private void RuleUp()
        {
            if (!IsSubruleSelected)
            {
                return;
            }
            int currentPriority = Rule.SubrulePriorities[SelectedSubrule.Guid];

            if (currentPriority == 1)
            {
                return;
            }

            Rule upperRule = (from l in _loadedSubrules
                              where Rule.SubrulePriorities[l.Value.Guid] == currentPriority - 1
                              select l.Value).First();

            Rule.SubruleGuids[currentPriority] = upperRule.Guid;
            Rule.SubruleGuids[currentPriority - 1] = SelectedSubrule.Guid;

            Rule.SubrulePriorities[upperRule.Guid] = currentPriority;
            Rule.SubrulePriorities[SelectedSubrule.Guid] = currentPriority - 1;

            Rule.Save();

            _subrulesList.SelectedItems[0] = _subrulesList.SelectedItems[0] - 1;

            LoadData();
        }

        private void RuleDown()
        {
            if (!IsSubruleSelected)
            {
                return;
            }
            int currentPriority = Rule.SubrulePriorities[SelectedSubrule.Guid];

            if (currentPriority == _loadedSubrules.Count)
            {
                return;
            }

            Rule lowerRule = (from l in _loadedSubrules
                              where Rule.SubrulePriorities[l.Value.Guid] == currentPriority + 1
                              select l.Value).First();

            Rule.SubruleGuids[currentPriority] = lowerRule.Guid;
            Rule.SubruleGuids[currentPriority + 1] = SelectedSubrule.Guid;

            Rule.SubrulePriorities[lowerRule.Guid] = currentPriority;
            Rule.SubrulePriorities[SelectedSubrule.Guid] = currentPriority + 1;

            lowerRule.Save();
            SelectedSubrule.Save();
            Rule.Save();

            _subrulesList.SelectedItems[0] = _subrulesList.SelectedItems[0] + 1;

            LoadData();
        }

        private bool IsSaved()
        {
            if (string.Empty == _nameInput.Text)
            {
                ShowAlertBox("Rule needs a name!");
                return false;
            }

            Save();

            return true;
        }

        protected override void Save()
        {
            if (!PreSave()) return;

            bool isNew = IsNew;

            int priority = 0;

            if (isNew)
            {
                priority = Parent.SubruleGuids.Count > 0 ? Parent.SubruleGuids.Keys.Max() + 1 : 1;

                Rule = new Rule(WorldSettings.Name)
                {
                    HierarchyLevel = HierarchyLevel,
                };
            }

            Rule.ConditionText = _conditionInput.Text;
            Rule.Name = _nameInput.Text;

            switch (SelectedAction)
            {
                case ACTION_SUBRULE:
                    Rule.ActionType = RuleActionsEnum.UseSubrules;
                    Rule.BlockName = null;
                    break;

                case ACTION_BLOCK:
                    Rule.ActionType = RuleActionsEnum.PlaceBlock;

                    if (!IsBlockSelected)
                    {
                        ShowAlertBox("Please select a block.");
                        return;
                    }

                    string selectedName = _blocksList.SelectedName();

                    if (selectedName == SYSTEM_AIR_NAME)
                    {
                        selectedName = BlockTypeHelper.AIR_BLOCK_TYPE.Name;
                    }

                    Rule.BlockName = selectedName;
                    break;

                case ACTION_OBJECT:
                    Rule.ActionType = RuleActionsEnum.PlaceObject;

                    if (!IsObjectSelected)
                    {
                        ShowAlertBox("Please select an object");
                        return;
                    }

                    Rule.ObjectName = _objectsList.SelectedName();
                    break;
                case ACTION_SPAWN:
                    Rule.ActionType = RuleActionsEnum.AddSpawnData;

                    if (!IsCharacterSelected)
                    {
                        ShowAlertBox("Please select a character");
                        return;
                    }

                    Rule.CharacterName = _npcList.SelectedName();
                    Rule.SpawnRate = _spawnRateControl.GetFloat();
                    break;
            }

            if (HierarchyLevel > 1)
            {
                Rule.ParentRuleGuid = ((Rule)Parent).Guid;
            }

            if (isNew)
            {
                Parent.SubruleGuids.Add(priority, Rule.Guid);
                Parent.SubrulePriorities.Add(Rule.Guid, priority);
                Parent.Save();
            }

            Rule.Save();

            Back();
        }

        private bool IsNameInputOK()
        {
            if (_nameInput.Text == "")
            {
                ShowAlertBox("Rule needs a name!");
                return false;
            }

            if (!IsFileNameOK(_nameInput.Text))
            {
                ShowAlertBox("Only latin characters, numbers, underscore and whitespace \n" +
                             "are allowed in rule name.");
                return false;
            }

            return true;
        }

        private bool PreSave()
        {
            if (!IsNameInputOK())
            {
                return false;
            }

            switch (SelectedAction)
            {
                case ACTION_SUBRULE:
                    break;

                case ACTION_BLOCK:
                    if (!IsBlockSelected)
                    {
                        ShowAlertBox("Please select a block.");
                        return false;
                    }

                    break;
            }
            // Empty expressions are now just Expression("true") by default
//            if (string.Empty == _conditionInput.Text)
//            {
//                ShowAlertBox("Condition can not be empty.");
//                return false;
//            }

            string errorMessage = "";
            bool condition = Rule.CheckCondition(WorldSettings, _conditionInput.Text, ref errorMessage);

            if (!condition)
            {
                ShowAlertBox(errorMessage != "" ? errorMessage : "Condition is wrong!");
                return false;
            }

            return true;
        }

        protected override void Back()
        {
            if (HierarchyLevel == 1)
            {
                var ruleSublistGUI = Rule.GetListGUI(Game, WorldSettings, MenuState);
                MenuState.SetGUI(ruleSublistGUI);
            }
            else
            {
                Guid parentGuid = Rule == null ? ((Rule)Parent).Guid : Rule.ParentRuleGuid;
                var ruleEditorGUI = new RuleEditorGUI(Game, WorldSettings, parentGuid);
                MenuState.SetGUI(ruleEditorGUI);
            }
        }
    }
}