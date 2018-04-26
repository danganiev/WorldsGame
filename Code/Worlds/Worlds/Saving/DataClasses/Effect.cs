using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Saving.DataClasses
{
    public enum EffectType
    {
        ChangeAttribute = 0,
        DamageBlock = 1,
        DamageAllBlocksExcept = 2,
        DestroyBlock = 3,
        // This is for the future
        // SpawnCharacter
        // DestroyEntity
    }

    public enum PossibleBlockEffect
    {
        DamageBlock = 1,
        DamageAllBlocksExcept = 2
    }

    public enum PossibleCreatureEffect
    {
        ChangeAttribute = 0
    }

    public enum EffectTarget
    {
        Block,
        Self, // this might need to go into the EffectArea enum
        Creature // this one includes self
    }

    // This is for the future

    // public enum EffectTiming
    //{
    //  Once
    //}

    // The effect of the item, passive character ability, or (in the future) block with aura, or something
    [Serializable]
    public class Effect
    {
        public EffectType EffectType { get; set; }

        public EffectTarget EffectTarget { get; set; }

        public int EffectArea { get; set; }

        public byte EffectRaduis { get; set; }

        // Name of attribute or block which is to be changed
        public string ApplicantName { get; set; }

        public float Value { get; set; }
    }

    public static class EffectNaming
    {
        private static Dictionary<int, string> _effectNames;
        private static Dictionary<EffectTarget, string> _effectTargetNames;        

        public static Dictionary<int, string> GetEffectNames()
        {
            if (_effectNames == null)
            {
                _effectNames = new Dictionary<int, string>
                {
                    {(int)EffectType.ChangeAttribute, "Change attribute"},
                    {(int)EffectType.DamageBlock, "Damage block"},
                    {(int)EffectType.DamageAllBlocksExcept, "Damage all blocks except"},
                };
            }
            return _effectNames;
        }

        public static Dictionary<EffectTarget, string> GetEffectTargetNames()
        {
            if (_effectTargetNames == null)
            {
                _effectTargetNames = new Dictionary<EffectTarget, string>
                {
                    {EffectTarget.Block, "Block"},
                    {EffectTarget.Self, "Self"},
                    {EffectTarget.Creature, "Creature"},
                };
            }
            return _effectTargetNames;
        }
    }
}