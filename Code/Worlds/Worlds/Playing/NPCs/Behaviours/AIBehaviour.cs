using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Players.Entity;

namespace WorldsGame.Playing.NPCs.Behaviours
{
    internal class AIBehaviour : EntityBehaviour
    {
        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entity owner)
        {
            var actionListComponent = owner.GetComponent<ActionListComponent>();

            actionListComponent.ActionList.Update(gameTime);
        }
    }
}