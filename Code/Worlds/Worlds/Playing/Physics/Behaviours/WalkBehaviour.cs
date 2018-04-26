using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players.Entity;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;
using WorldsLib;

namespace WorldsGame.Playing.Physics.Behaviours
{
    internal interface IMovementBehaviour
    {
        void Jump(Entity entity);

        void Jump(Entity entity, bool quiet);
    }

    internal class WalkBehaviour : EntityBehaviour, IMovementBehaviour
    {
        private const float MOVEMENT_SPEED = 3.5f;
        private const float JUMP_VELOCITY = 6f;

        // Used to calculate difference if entity is standing
        private static readonly Vector3 FOOT_DIFF = new Vector3(0, 0.1f, 0);

        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entity owner)
        {
            var physicsComponent = owner.GetComponent<PhysicsComponent>();

            Move(gameTime, owner, physicsComponent);

            var animationComponent = owner.GetComponent<AnimationComponent>();

            if (physicsComponent.IsMoving)
            {
                animationComponent.PlayAnimation(AnimationType.Walk);
            }
            else
            {
                animationComponent.StopAnimation(AnimationType.Walk);
            }
        }

        internal void Move(GameTime gameTime, Entity entity, PhysicsComponent physicsComponent)
        {
            var moveVector = new Vector3();
            var positionComponent = entity.GetComponent<PositionComponent>();
            var scaleAndRotateComponent = entity.GetComponent<ScaleAndRotateComponent>();

            //            CheckBounds(entity, positionComponent);
            moveVector = ProcessSidesMovement(entity, physicsComponent, moveVector);

            moveVector = SpeedAndRotateMoveVector(
                (float)gameTime.ElapsedGameTime.TotalSeconds, moveVector, scaleAndRotateComponent);

            MoveTo(moveVector, entity, positionComponent);
        }

        private Vector3 SpeedAndRotateMoveVector(float totalSeconds, Vector3 moveVector, ScaleAndRotateComponent scaleAndRotate)
        {
            moveVector = Vector3.Transform(moveVector, Matrix.CreateRotationY(scaleAndRotate.LeftRightRotation));

            moveVector *= MOVEMENT_SPEED * totalSeconds;

            return moveVector;
        }

        private void MoveTo(Vector3 moveVector, Entity entity, PositionComponent positionComponent)
        {
            // Attempt to move, doing collision stuff.
            if (TryToMoveTo(moveVector, entity, positionComponent))
            {
            }
            //            else if (!TryToMoveTo(new Vector3(0, 0, moveVector.Z), positionComponent, physicsComponent, isReconciled))
            //            {
            //            }
            //            else if (!TryToMoveTo(new Vector3(moveVector.X, 0, 0), positionComponent, physicsComponent, isReconciled))
            //            {
            //            }
            //            else if (!TryToMoveTo(new Vector3(0, moveVector.Y, 0), positionComponent, physicsComponent, isReconciled))
            //            {
            //            }
        }

        private bool TryToMoveTo(Vector3 moveVector, Entity entity, PositionComponent positionComponent)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            World world = entity.GetConstantComponent<WorldComponent>().World;

            if (moveLength > 0)
            {
                Vector3 testVector = moveVector;
                testVector.Normalize();
                testVector = testVector * (moveLength + 0.3f);

                // Apply this test vector.
                Vector3 movePosition = positionComponent.Position + testVector;

                try
                {
                    if (!world.GetBlock(movePosition).IsSolid)
                    {
                        positionComponent.Position += moveVector;

                        return true;
                    }
                }
                catch (NullReferenceException)
                {
                    // Entity is falling and we're on the edge of chunks or chunks just didn't generated yet.
                    //                    throw;
                }
            }

            return false;
        }

        // Should be changed to normal AABB collisions
        private void CheckBounds(Entity entity, PositionComponent positionComponent)
        {
            //            bool isHeadBlockSolid = _player.HeadBlock != null && _player.HeadBlock.IsSolid;
            World world = entity.GetConstantComponent<WorldComponent>().World;
            BlockType footBlock = world.GetBlock(positionComponent.Position);
            bool isFootBlockSolid = footBlock == null || footBlock.IsSolid;

            if (isFootBlockSolid)
            {
                CheckBottom(world, positionComponent);
            }

            //            if (isHeadBlockSolid)
            //            {
            //                CheckHeadStuckAbove();
            //            }
        }

        private Vector3 ProcessSidesMovement(Entity entity, PhysicsComponent physicsComponent, Vector3 moveVector)
        {
            bool isClientPlayer = IsClientPlayer(entity);

            Vector3 oldMoveVector = moveVector;

            if (physicsComponent.IsMovingForward)
            {
                moveVector = MoveForward(moveVector);
            }
            if (physicsComponent.IsMovingBackward)
            {
                moveVector = MoveBackward(moveVector);
            }
            if (physicsComponent.IsStrafingLeft)
            {
                moveVector = StrafeLeft(moveVector);
            }
            if (physicsComponent.IsStrafingRight)
            {
                moveVector = StrafeRight(moveVector);
            }

            if (isClientPlayer && oldMoveVector != moveVector)
            {
                Messenger.Invoke("PlayerMovementChanged");
            }

            return moveVector;
        }

        private static bool IsClientPlayer(Entity entity)
        {
            return entity.HasComponent(typeof(PlayerComponent)) &&
                   entity.GetComponent<PlayerComponent>().IsClientPlayer;
        }

        private Vector3 MoveForward(Vector3 moveVector)
        {
            moveVector += Vector3.Forward;
            return moveVector;
        }

        private Vector3 MoveBackward(Vector3 moveVector)
        {
            moveVector += Vector3.Backward;
            return moveVector;
        }

        private Vector3 StrafeLeft(Vector3 moveVector)
        {
            return moveVector + Vector3.Left;
        }

        private Vector3 StrafeRight(Vector3 moveVector)
        {
            return moveVector + Vector3.Right;
        }

        public void Jump(Entity entity)
        {
            Jump(entity, false);
        }

        public void Jump(Entity entity, bool quiet)
        {
            if (IsClientPlayer(entity) && IsBottomSolid(entity) && Math.Abs(entity.GetComponent<PhysicsComponent>().VerticalVelocityAsVector3.Y) < 0.001)
            {
                entity.GetComponent<PhysicsComponent>().YVelocity = JUMP_VELOCITY;
                entity.GetComponent<PositionComponent>().YPosition = entity.GetComponent<PositionComponent>().YPosition + 0.11f;

                if (!quiet && IsClientPlayer(entity))
                {
                    Messenger.Invoke("PlayerJumped");
                }
            }
        }

        private void CheckBottom(Entity entity)
        {
            CheckBottom(entity.GetComponent<WorldComponent>().World, entity.GetComponent<PositionComponent>());
        }

        private void CheckBottom(World world, PositionComponent positionComponent)
        {
            // If the player is stuck in the ground, bring them out.
            // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
            try
            {
                int index = 1;
                BlockType footBlock = world.GetBlock(positionComponent.Position);
                while (footBlock.IsSolid && index <= 10)
                {
                    positionComponent.Position += new Vector3(0, 0.1f, 0);
                    index++;
                    footBlock = world.GetBlock(positionComponent.Position);
                }
            }
            catch (NullReferenceException)
            {
                // Somehow FootBlock changed to null
            }
        }

        private bool IsBottomSolid(Entity entity)
        {
            return entity.GetBehaviour<GravityBehaviour>().GetBottomType(
                entity, entity.GetComponent<PositionComponent>()) == BottomBlockType.Solid;
        }

        //
        //        private void CheckHeadStuckAbove()
        //        {
        //            // If the player has their head stuck in a block, push them down.
        //            try
        //            {
        //                if (_player.HeadBlock.IsSolid)
        //                {
        //                    var blockIn = (int)(_player.HeadPosition.Y);
        //                    _player.YPosition = blockIn - 0.15f;
        //                }
        //            }
        //            catch (NullReferenceException)
        //            {
        //                // Somehow HeadBlock changed to null
        //            }
        //        }
    }
}