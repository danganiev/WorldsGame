using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.NPCs;

namespace WorldsGame.Playing.Entities.Components
{
    /// <summary>
    /// Contains all data about the actor (player or npc)
    /// </summary>
    internal class CharacterActorComponent : IEntityComponent
    {
        private readonly Entity _owner;

        internal Dictionary<string, float> Attributes { get; private set; }

        internal event Action<Entity, string, float> OnAttributeChange = (owner, name, value) => { };

        internal Vector3 LookVector { get; set; }

        internal CharacterActorComponent(Entity owner)
        {
            _owner = owner;
            Attributes = new Dictionary<string, float>();
        }

        internal void AttributeChanged(string name, float value)
        {
            OnAttributeChange(_owner, name, value);
        }

        public void Dispose()
        {
            OnAttributeChange = null;
        }        
    }
}