using System;
using System.Collections.Generic;
using System.Linq;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledEffect
    {
        public List<Effect> Effects { get; private set; }

//        public Dictionary<EffectType, List<Effect>> EffectsByType { get; private set; }

        public CompiledEffect()
        {
//            EffectsByType = new Dictionary<EffectType, List<Effect>>();
            Effects = new List<Effect>();
        }

        public void AddEffect(Effect effect)
        {
//            if (!AffectsType(effect.EffectType))
//            {
//                EffectsByType.Add(effect.EffectType, new List<Effect>());
//                Effects.Add(effect);
//            }

            effect.ApplicantName = effect.ApplicantName.ToLower();
//            EffectsByType[effect.EffectType].Add(effect);
            Effects.Add(effect);
        }

//        public bool AffectsType(EffectType effectType)
//        {
//            return EffectsByType.ContainsKey(effectType);
//        }
    }
}