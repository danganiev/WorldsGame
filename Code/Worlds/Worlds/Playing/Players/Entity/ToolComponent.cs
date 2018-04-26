using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Players;

namespace WorldsGame.Models.Tools
{
    internal class ToolComponent : IEntityComponent
    {
        internal Entity Owner { get; set; }

        internal Vector3 PlayerLookVector { get { return Owner.GetComponent<CharacterActorComponent>().LookVector; } }

        //        internal CompiledEffect Effect { get; private set; }

        internal ToolComponent(Entity owner)
        {
            Owner = owner;
        }

        public void Dispose()
        {
        }
    }
}