using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Terrain;
using WorldsLib;

namespace WorldsGame.Playing.Physics.Behaviours
{
    internal enum BottomBlockType
    {
        Solid,
        Liquid,
        Gas
    }

    internal class GravityBehaviour : EntityBehaviour
    {
        internal const float FOOT_DIFF = 0.1f;
        private float _maxHeight;

        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entity owner)
        {
            ProcessVerticalVelocity((float)gameTime.ElapsedGameTime.TotalSeconds, owner);
        }

        internal void ProcessVerticalVelocity(float totalSeconds, Entity entity)
        {
            var physicsComponent = entity.GetComponent<PhysicsComponent>();
            var positionComponent = entity.GetComponent<PositionComponent>();

            BottomBlockType bottomType = GetBottomType(entity, positionComponent);

            if (bottomType == BottomBlockType.Solid)
            {
                physicsComponent.YVelocity = 0;
                positionComponent.Position = new Vector3(positionComponent.Position.X, _maxHeight, positionComponent.Position.Z);
                return;
            }
            if (bottomType == BottomBlockType.Liquid)
            {
                GetBottomType(entity, positionComponent);
                physicsComponent.YVelocity = -EntityConstants.LIQUID_GRAVITY;
            }
            else
            {
                GetBottomType(entity, positionComponent);
                physicsComponent.YVelocity = physicsComponent.YVelocity - EntityConstants.GRAVITY * totalSeconds;
            }

            Vector3 difference = physicsComponent.VerticalVelocityAsVector3 * totalSeconds;

            if (difference.Y > -1)
            {
                positionComponent.Position += difference;
            }
            else
            {
                while (difference.Y < -1)
                {
                    positionComponent.Position += Vector3.Down;
                    difference.Y = difference.Y + 1;

                    //                    if (IsBottomBlockSolid(entity, positionComponent))
                    if (GetBottomType(entity, positionComponent) == BottomBlockType.Solid)
                    {
                        physicsComponent.YVelocity = 0;
                        positionComponent.Position = new Vector3(positionComponent.Position.X, _maxHeight, positionComponent.Position.Z);

                        break;
                    }
                }
            }
        }

        private bool IsBottomBlockSolid(Entity entity, PositionComponent positionComponent)
        {
            var world = entity.GetConstantComponent<WorldComponent>();

            return world.World.GetBlock(positionComponent.Position).IsSolid ||
                    world.World.GetBlock(positionComponent.Position - new Vector3(0, 0.1f, 0)).IsSolid;
        }

        internal BottomBlockType GetBottomType(Entity entity, PositionComponent positionComponent)
        {
            // NOTE: could be specifically improved for 1-blocked entities (i.e. items)
            var world = entity.GetConstantComponent<WorldComponent>();
            var boundingBoxComponent = entity.GetComponent<BoundingBoxComponent>();
            BoundingBox boundingBox = boundingBoxComponent.GetBoundingBox(positionComponent.Position);

            float minX = MathHelper.Min(boundingBox.Max.X, boundingBox.Min.X);
            float maxX = MathHelper.Max(boundingBox.Max.X, boundingBox.Min.X);

            float minZ = MathHelper.Min(boundingBox.Max.Z, boundingBox.Max.Z);
            float maxZ = MathHelper.Max(boundingBox.Max.Z, boundingBox.Min.Z);

            var blockPositions = new HashSet<Vector3i>();

            blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(minX, positionComponent.Position.Y, minZ)));
            blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(minX, positionComponent.Position.Y, maxZ)));
            blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(maxX, positionComponent.Position.Y, minZ)));
            blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(maxX, positionComponent.Position.Y, maxZ)));

            float x = minX;
            while (x <= maxX)
            {
                float z = minZ;
                while (z <= maxZ)
                {
                    blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(x, positionComponent.Position.Y, z)));
                    blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(x, positionComponent.Position.Y - FOOT_DIFF, z)));
                    z += 1;
                }
                x += 1;
            }

            bool isLiquid = false;

            foreach (Vector3i blockPosition in blockPositions)
            {
                var block = world.World.GetBlock(blockPosition);

                if (block.IsSolid)
                {
                    _maxHeight = Math.Max(blockPosition.Y + block.GetMaxHeight(), _maxHeight);
                    // TODO: Problem here because of not full loop
                    return BottomBlockType.Solid;
                }
                if (block.IsLiquid)
                {
                    isLiquid = true;
                }
            }

            if (isLiquid)
            {
                return BottomBlockType.Liquid;
            }

            return BottomBlockType.Gas;
        }
    }
}