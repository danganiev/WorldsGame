using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Players;

namespace WorldsGame.Playing.Entities
{
    internal class ModelAnimationBehaviour : EntityBehaviour
    {
        public override bool IsDrawable
        {
            get
            {
                return false;
            }
        }

        public override void Update(GameTime gameTime, Entity owner)
        {
            var animationComponent = owner.GetComponent<AnimationComponent>();

            // This should not work right when frames will be skipped, 100%
            animationComponent.UpdateAnimations(gameTime);
        }
    }
}