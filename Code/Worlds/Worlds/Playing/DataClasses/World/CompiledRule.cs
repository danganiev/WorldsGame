using System;
using System.Collections.Generic;
using System.Linq;
using WorldsGame.Saving;
using WorldsGame.Utils;

namespace WorldsGame.Playing.DataClasses
{
    public enum RuleActionsEnum
    {
        UseSubrules,
        PlaceBlock,
        PlaceObject,
        AddSpawnData,
        OverrideConstant
    }

    [Serializable]
    public class CompiledRule
    {
        public string Name { get; set; }

        public RuleActionsEnum ActionType { get; set; }

        public Guid Guid { get; set; }

        [NonSerialized]
        private WorldsExpression _condition;

        public WorldsExpression Condition
        {
            get
            {
                if (_condition == null)
                {
                    Condition = new WorldsExpression(ConditionText);
                }
                return _condition;
            }
            private set { _condition = value; }
        }

        private string _conditionText;

        public string ConditionText
        {
            get { return _conditionText; }
            set
            {
                _conditionText = value;
                Condition = new WorldsExpression(value);
            }
        }

        public List<string> Parameters
        {
            get
            {
                var paramz = new List<string>();
                paramz.AddRange(Condition.Parameters);

                foreach (KeyValuePair<int, CompiledRule> compiledRule in Subrules)
                {
                    paramz.AddRange(compiledRule.Value.Parameters);
                }
                return paramz.Distinct().ToList();
            }
        }

        public string BlockName { get; set; }

        public string ObjectName { get; set; }

        // So the spawn algorithm can go like this:
        // On first chunk generation, run dice 10 times to check how many stuff generates with chunk.
        // For every successful spawn find an eligible location and add to the chunk pool of eligible blocks. Spawn.

        // Then, for in-game spawning
        // If there are more NPCs in game than the NPC cap, do nothing
        // Else go over each chunk, if chunk has possible spawns, check the probability of each spawn.
        // If probability check succeeds, pick a random block inside chunk, then check if it's eligible to spawn.
        // If uneligible, and there are no other eligible blocks, go up in Y until you find an eligible one.
        // If there are other eligible blocks, just use one of them
        // If not found, break.
        // If found, add to the pool of eligible blocks. On any chunk modification clear the pool.
        // If a block is eligible for spawning (is air block with space around), then spawn the NPC.

        public string CharacterName { get; set; }

        public float SpawnRate { get; set; }

        public string ConstantName { get; set; }

        public float ConstantValue { get; set; }

        public Dictionary<int, CompiledRule> Subrules { get; set; }

        [NonSerialized]
        private CompiledGameBundle _gameBundle;

        public CompiledGameBundle GameBundle
        {
            get { return _gameBundle; }
            set { _gameBundle = value; }
        }

        //For serialization only
        public CompiledRule()
        {
        }

        public CompiledRule(CompiledGameBundle gameBundle, Rule rule)
        {
            GameBundle = gameBundle;
            Name = rule.Name;
            ConditionText = rule.ConditionText;
            ActionType = rule.ActionType;
            Guid = rule.Guid;

            Subrules = new Dictionary<int, CompiledRule>();

            switch (ActionType)
            {
                case RuleActionsEnum.UseSubrules:
                    FillSubrules(rule);
                    break;

                case RuleActionsEnum.PlaceBlock:
                    BlockName = rule.BlockName;
                    break;

                case RuleActionsEnum.PlaceObject:
                    ObjectName = rule.ObjectName;
                    break;

                case RuleActionsEnum.AddSpawnData:
                    CharacterName = rule.CharacterName;
                    SpawnRate = rule.SpawnRate;
                    break;

                case RuleActionsEnum.OverrideConstant:
                    ConstantName = rule.ConstantName;
                    ConstantValue = rule.ConstantValue;
                    break;
            }
        }

        private void FillSubrules(Rule rule)
        {
            foreach (Rule subrule in rule.Subrules)
            {
                Subrules.Add(rule.SubrulePriorities[subrule.Guid], new CompiledRule(GameBundle, subrule));
            }
        }
    }
}