using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.NPCs;

namespace WorldsGame.Playing.Entities.Components
{
    // General behaviour of living creatures
    internal class CharacterActorBehaviour : EntityBehaviour
    {
//        internal event Action<Entity> OnDeath = (owner) => { };

        internal void Initialize(Entity owner)
        {
            foreach (var compiledCharacterAttribute in CharacterAttributeHelper.CompiledAttributes)
            {
                owner.GetComponent<CharacterActorComponent>().Attributes.Add(compiledCharacterAttribute.Key,
                                                                             compiledCharacterAttribute.Value.DefaultValue);
            }

            owner.GetComponent<CharacterActorComponent>().OnAttributeChange += HealthMonitor;
        }

        internal static void ResetAttributesToDefault(Entity owner)
        {
            foreach (var compiledCharacterAttribute in CharacterAttributeHelper.CompiledAttributes)
            {
                owner.GetComponent<CharacterActorComponent>().Attributes[compiledCharacterAttribute.Key] =
                    compiledCharacterAttribute.Value.DefaultValue;
                 owner.GetComponent<CharacterActorComponent>().AttributeChanged(compiledCharacterAttribute.Key, compiledCharacterAttribute.Value.DefaultValue);
            }
        }

        private void HealthMonitor(Entity owner, string attribute, float value)
        {
            if (attribute == "health" && value <= 0)
            {
                Die(owner);
            }
        }

        private void Die(Entity owner)
        {
            owner.PseudoDie();
        }

        internal void SetAttribute(Entity owner, string name, float value)
        {
            if (CheckAttribute(owner, name, value))
            {
                owner.GetComponent<CharacterActorComponent>().Attributes[name] = value;
                owner.GetComponent<CharacterActorComponent>().AttributeChanged(name, value);
            }
        }

        internal float GetAttribute(Entity owner, string name)
        {
            return owner.GetComponent<CharacterActorComponent>().Attributes[name];
        }

        private bool CheckAttribute(Entity owner, string name, float value)
        {
            if (!owner.GetComponent<CharacterActorComponent>().Attributes.ContainsKey(name))
            {
                return false;
            }

            CompiledCharacterAttribute attribute = CharacterAttributeHelper.CompiledAttributes[name];

            return value <= attribute.DefaultMaxValue && value >= attribute.DefaultMinValue;
        }

        public override void Dispose()
        {
            
        }
    }
}