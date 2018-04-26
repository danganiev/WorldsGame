using System;
using System.Collections.Generic;
using System.Linq;
using WorldsGame.Gamestates;
using WorldsGame.GUI;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Utils;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Rule : ISaveDataSerializable<Rule>, IRuleHolder
    {
        public string Name { get; set; }

        public string WorldSettingsName { get; set; }

        public Dictionary<int, Guid> SubruleGuids { get; set; }

        public Dictionary<Guid, int> SubrulePriorities { get; set; }

        public Guid Guid { get; set; }

        public string BlockName { get; set; }

        public string ObjectName { get; set; }

        public string CharacterName { get; set; }

        public float SpawnRate { get; set; }

        public string ConstantName { get; set; }

        public float ConstantValue { get; set; }

        // If world is the parent then nothing in the next field
        public Guid ParentRuleGuid { get; set; }

        public string ConditionText { get; set; }

        public int HierarchyLevel { get; set; }

        public RuleActionsEnum ActionType { get; set; }

        public string FileName { get { return Guid.ToString() + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Rules"; } }

        public Rule(string worldSettingsName, string name = "", Guid guid = new Guid())
        {
            WorldSettingsName = worldSettingsName;
            Name = name;
            SubruleGuids = new Dictionary<int, Guid>();
            SubrulePriorities = new Dictionary<Guid, int>();
            ParentRuleGuid = new Guid();

            Guid = guid == Guid.Empty ? Guid.NewGuid() : guid;
        }

        internal static SaverHelper<Rule> SaverHelper(string name)
        {
            return new SaverHelper<Rule>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<Rule> SaverHelper()
        {
            return SaverHelper(WorldSettingsName);
        }

        public void Save()
        {
            SaverHelper().Save(this);
        }

        public void Delete()
        {
            DeleteSubrules();

            SaverHelper().Delete(FileName);
        }

        public void DeleteSubrule(Rule subrule)
        {
            int priority = SubrulePriorities[subrule.Guid];

            for (int i = priority; i < SubruleGuids.Keys.Count; i++)
            {
                SubruleGuids[i] = SubruleGuids[i + 1];
                SubrulePriorities[SubruleGuids[i]] = i;
            }

            SubruleGuids.Remove(SubruleGuids.Count);
            SubrulePriorities.Remove(subrule.Guid);

            Save();

            subrule.Delete();
        }

        public void DeleteSubrules()
        {
            foreach (Rule rule in Subrules)
            {
                rule.Delete();
            }
        }

        public static Rule Load(string worldName, Guid guid)
        {
            return SaverHelper(worldName).Load(guid.ToString());
        }

        internal static RuleSublistGUI GetListGUI(WorldsGame game, WorldSettings worldSettings, MenuState menuState)
        {
            var gui = new RuleSublistGUI(game, worldSettings, saverHelper: SaverHelper(worldSettings.Name))
            {
                Title = "Edit rules",
                DeleteBoxText = "Delete rule?",
                CreateAction = (game_, worldSettings_) =>
                {
                    var ruleGUI = new RuleEditorGUI(game_, worldSettings_);
                    menuState.SetGUI(ruleGUI);
                },
                EditAction = (game_, worldSettings_, selectedElement) =>
                {
                    var ruleGUI = new RuleEditorGUI(game_, worldSettings_, worldSettings_, selectedElement);
                    menuState.SetGUI(ruleGUI);
                }
            };

            return gui;
        }

        internal List<Rule> Subrules
        {
            get
            {
                List<string> subruleNames = (from guid in SubruleGuids
                                             select guid.Value.ToString()).ToList();
                return (from subrule in SaverHelper(WorldSettingsName).LoadList(subruleNames)
                        orderby SubrulePriorities[subrule.Guid]
                        select subrule).ToList();
            }
        }

        internal bool CheckCondition(WorldSettings worldSettings, ref List<string> errorsList)
        {
            var condition = new WorldsExpression(ConditionText);
            var excessConditionParameters = condition.Parameters.Except(worldSettings.NoiseNames).ToList();
            bool ruleIsGood = excessConditionParameters.Count == 0 && !condition.HasErrors();

            errorsList.AddRange(excessConditionParameters);

            if (!ruleIsGood)
            {
                return false;
            }

            foreach (Rule subrule in Subrules)
            {
                ruleIsGood = subrule.CheckCondition(worldSettings, ref errorsList);
                if (!ruleIsGood)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CheckCondition(WorldSettings worldSettings, string conditionText, ref string errorMessage)
        {
            errorMessage = "";
            var condition = new WorldsExpression(conditionText);
            var excessConditionParameters = condition.Parameters.Except(worldSettings.NoiseNames).ToList();
            bool ruleIsGood = excessConditionParameters.Count == 0 && !condition.HasErrors();

            if (excessConditionParameters.Count > 0)
            {
                errorMessage = ErrorMessage(excessConditionParameters);
            }

            return ruleIsGood;
        }

        internal static string ErrorMessage(List<string> excessConditionParameters)
        {
            return string.Format("Noises: '{0}' don't exist", string.Join("', '", excessConditionParameters));
        }
    }
}