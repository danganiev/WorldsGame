using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players.Entity;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Physics.Behaviours
{
    // Includes velocity and friction for now, but that should be divided
    // into two/three different behaviours in anything but simplest physics.
    // But I need only the simplest, yay!
    internal class PhysicsBehaviour : EntityBehaviour
    {
        // Mass is 1 for everything for now
        private const float MASS = 1f;

        private const float FRICTION = 1f;

        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entity owner)
        {
            var physicsComponent = owner.GetComponent<PhysicsComponent>();

            UpdateFriction(gameTime, physicsComponent);
            UpdateVelocity(gameTime, physicsComponent, owner);
        }

        private void UpdateFriction(GameTime gameTime, PhysicsComponent physicsComponent)
        {
            if (Math.Abs(physicsComponent.Velocity.Y) > 0.01f)
            {
                var frictionValue = (float)gameTime.ElapsedGameTime.TotalSeconds * FRICTION;

                physicsComponent.Velocity -= new Vector3(frictionValue, 0, frictionValue);

                if (physicsComponent.Velocity.X < 0)
                {
                    physicsComponent.Velocity += new Vector3(-physicsComponent.Velocity.X, 0, 0);
                }
                if (physicsComponent.Velocity.Z < 0)
                {
                    physicsComponent.Velocity += new Vector3(0, 0, -physicsComponent.Velocity.Z);
                }
            }
        }

        private void UpdateVelocity(GameTime gameTime, PhysicsComponent physicsComponent, Entity owner)
        {
            // Doesn't update Y velocity cause there is gravity behaviour for that
            if (physicsComponent.Velocity != Vector3.Zero)
            {
                Vector3 travelledSpace = physicsComponent.Velocity * gameTime.ElapsedGameTime.Seconds;

                owner.GetComponent<PositionComponent>().Position += new Vector3(travelledSpace.X, 0, travelledSpace.Z);
            }
        }
    }
}