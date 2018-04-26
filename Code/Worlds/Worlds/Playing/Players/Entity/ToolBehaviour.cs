using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Physics.Components;

namespace WorldsGame.Models.Tools
{
    internal class ToolBehaviour : EntityBehaviour
    {
        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entity tool)
        {
            var toolComponent = tool.GetComponent<ToolComponent>();

            var positionComponent = tool.GetComponent<PositionComponent>();

            var scaleAndRotationComponent = tool.GetComponent<ScaleAndRotateComponent>();

            positionComponent.Position = toolComponent.Owner.GetComponent<PositionComponent>().Position;
            scaleAndRotationComponent.LeftRightRotation = toolComponent.Owner.GetComponent<ScaleAndRotateComponent>().LeftRightRotation;

            var physicsComponent = toolComponent.Owner.GetComponent<PhysicsComponent>();

            var animationComponent = tool.GetComponent<AnimationComponent>();

            if (physicsComponent.IsMoving)
            {
                animationComponent.PlayAnimation(AnimationType.Walk);
            }
            else
            {
                animationComponent.StopAnimation(AnimationType.Walk);
            }
        }
    }
}