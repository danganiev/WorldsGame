using System;
using System.Collections.Generic;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Playing.Effects
{
    internal class EffectTargetDetector
    {
        internal Dictionary<Entity, List<Effect>> GetEffectTargets(CompiledEffect effect, Entity effectStarter)
        {
            var resultDict = new Dictionary<Entity, List<Effect>>();

//            if (effect.AffectsType(EffectType.ChangeAttribute))
//            {
//                ProcessAttributeChange(effect, effectStarter, resultDict);
//            }

            foreach (Effect e in effect.Effects)
            {
                if (e.EffectArea > 0)
                {
                    ProcessAreaEffect(e, effectStarter, resultDict);
                }
                else
                {
                    ProcessLocalEffect(e, effectStarter, resultDict);
                }
            }

            return resultDict;
        }

        private void ProcessLocalEffect(Effect effect, Entity effectStarter, Dictionary<Entity, List<Effect>> resultDict)
        {
            if (effect.EffectType == EffectType.ChangeAttribute)
            {
                ProcessAttributeChange(effect, effectStarter, resultDict);
            }
        }

        private void ProcessAreaEffect(Effect effect, Entity effectStarter, Dictionary<Entity, List<Effect>> resultDict)
        {
//            if (effect.EffectType == EffectType.ChangeAttribute)
//            {
//                ProcessAttributeChange(effect, effectStarter, resultDict);
//            }
        }

//        private static void ProcessAttributeChange(CompiledEffect effect, Entity effectStarter, Dictionary<Entity, List<Effect>> resultDict)
//        {
//            foreach (Effect ef in effect.EffectsByType[EffectType.ChangeAttribute])
//            {
//                if (ef.EffectTarget == EffectTarget.Self)
//                {
//                    if (!resultDict.ContainsKey(effectStarter))
//                    {
//                        resultDict[effectStarter] = new List<Effect>();
//                    }
//                    resultDict[effectStarter].Add(ef);
//                }
//            }
//        }

        private static void ProcessAttributeChange(Effect effect, Entity effectStarter, Dictionary<Entity, List<Effect>> resultDict)
        {
            if (effect.EffectTarget == EffectTarget.Self)
            {
                if (!resultDict.ContainsKey(effectStarter))
                {
                    resultDict[effectStarter] = new List<Effect>();
                }
                resultDict[effectStarter].Add(effect);
            }
            
        }
    }
}