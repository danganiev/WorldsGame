using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Inventory;

namespace WorldsGame.Playing.Entities
{
    internal class TakeableItemBehaviour : EntityBehaviour
    {
        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entity owner)
        {
            var scaleAndRotateComponent = owner.GetComponent<ScaleAndRotateComponent>();

            scaleAndRotateComponent.LeftRightRotation += 0.05f;

            if (scaleAndRotateComponent.LeftRightRotation > MathHelper.TwoPi)
            {
                scaleAndRotateComponent.LeftRightRotation -= MathHelper.TwoPi;
            }
        }
    }
}