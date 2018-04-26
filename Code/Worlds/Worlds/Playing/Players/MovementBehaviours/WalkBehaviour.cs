using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Players;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.ExtensionMethods;
using WorldsLib;

namespace WorldsGame.Players.MovementBehaviours
{
    //    internal class WalkBehaviour : MovementBehaviour
    //    {
    //        // Used to calculate difference if player is standing
    //        internal static readonly Vector3 FOOT_DIFF = new Vector3(0, 0.1f, 0);
    //
    //        internal WalkBehaviour(Player player)
    //            : base(player)
    //        {
    //        }
    //
    //        internal override Vector3 UpdateMoveVector(float totalSeconds, Vector3 moveVector, float cameraLeftRightRotation = NULL_CAMERA_LEFT_RIGHT_VALUE)
    //        {
    //            moveVector *= PLAYERMOVESPEED * totalSeconds;
    //
    //            if (cameraLeftRightRotation == NULL_CAMERA_LEFT_RIGHT_VALUE)
    //            {
    //                cameraLeftRightRotation = _player.LeftRightRotation;
    //            }
    //
    //            return Vector3.Transform(moveVector, Matrix.CreateRotationY(cameraLeftRightRotation));
    //        }
    //
    //        internal override void ProcessVerticalVelocity(float totalSeconds)
    //        {
    //            if (GetBottomType())
    //            {
    //                _player.YVelocity = 0;
    //            }
    //            else
    //            {
    //                _player.YVelocity = _player.YVelocity - EntityConstants.GRAVITY * totalSeconds;
    //            }
    //
    //            Vector3 difference = _player.VerticalVelocityAsVector3 * totalSeconds;
    //
    //            if (difference.Y > -1)
    //            {
    //                _player.Position += difference;
    //            }
    //            else
    //            {
    //                while (difference.Y < -1)
    //                {
    //                    _player.Position += Vector3.Down;
    //                    difference.Y = difference.Y + 1;
    //
    //                    if (GetBottomType())
    //                    {
    //                        _player.YVelocity = 0;
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //
    //        private bool GetBottomType()
    //        {
    //            // NOTE: could be specifically improved for 1-blocked entities (i.e. items)
    //
    //            BoundingBox boundingBox = _player.BoundingBox.GetBoundingBox(_player.Position, _player.LeftRightRotation);
    //
    //            float minX = MathHelper.Min(boundingBox.Max.X, boundingBox.Min.X);
    //            float maxX = MathHelper.Max(boundingBox.Max.X, boundingBox.Min.X);
    //
    //            float minZ = MathHelper.Min(boundingBox.Max.Z, boundingBox.Max.Z);
    //            float maxZ = MathHelper.Max(boundingBox.Max.Z, boundingBox.Min.Z);
    //
    //            var blockPositions = new HashSet<Vector3i>
    //            {
    //                WorldBlockOperator.GetBlockPosition(new Vector3(minX, _player.Position.Y, minZ)),
    //                WorldBlockOperator.GetBlockPosition(new Vector3(minX, _player.Position.Y, maxZ)),
    //                WorldBlockOperator.GetBlockPosition(new Vector3(maxX, _player.Position.Y, minZ)),
    //                WorldBlockOperator.GetBlockPosition(new Vector3(maxX, _player.Position.Y, maxZ))
    //            };
    //
    //            float x = minX;
    //            while (x <= maxX)
    //            {
    //                float z = minZ;
    //                while (z <= maxZ)
    //                {
    //                    blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(x, _player.Position.Y, z)));
    //                    blockPositions.Add(WorldBlockOperator.GetBlockPosition(new Vector3(x, _player.Position.Y, z) - FOOT_DIFF));
    //                    z += 1;
    //                }
    //                x += 1;
    //            }
    //
    //            foreach (Vector3i blockPosition in blockPositions)
    //            {
    //                bool result = _player.World.GetBlock(blockPosition).IsSolid;
    //
    //                if (result)
    //                {
    //                    return true;
    //                }
    //            }
    //
    //            return false;
    //        }
    //
    //        internal override Vector3 MoveForward(Vector3 moveVector)
    //        {
    //            moveVector += Vector3.Forward * 2;
    //            return moveVector;
    //        }
    //
    //        internal override Vector3 MoveBackward(Vector3 moveVector)
    //        {
    //            moveVector += Vector3.Backward * 2;
    //            return moveVector;
    //        }
    //
    //        internal override void Jump()
    //        {
    //            Jump(false);
    //        }
    //
    //        internal override void Jump(bool quiet)
    //        {
    //            if (GetBottomType() && Math.Abs(_player.VerticalVelocityAsVector3.Y) < 0.001)
    //            {
    //                _player.YVelocity = PLAYERJUMPVELOCITY;
    //                _player.YPosition = _player.YPosition + 0.11f;
    //
    //                if (!quiet)
    //                {
    //                    Messenger.Invoke("PlayerJumped");
    //                }
    //            }
    //        }
    //    }
}