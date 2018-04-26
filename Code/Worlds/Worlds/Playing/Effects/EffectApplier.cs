using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Playing.Effects
{
    internal static class EffectApplier
    {

        internal static void ApplyEffects(Entity entity, List<Effect> effects)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                Effect effect = effects[i];
                switch (effect.EffectType)
                {
                    case EffectType.ChangeAttribute:
                        var attributeComponent = entity.GetBehaviour<CharacterActorBehaviour>();

                        attributeComponent.SetAttribute(entity, effect.ApplicantName,
                                                        attributeComponent.GetAttribute(entity, effect.ApplicantName) +
                                                        effect.Value);
                        break;
                    case EffectType.DestroyBlock:
                        break;

                }
            }
        }
    }
}