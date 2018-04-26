using System;

using Microsoft.Xna.Framework;

using WorldsGame.Playing.Players;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Players.MovementBehaviours
{
    //    internal class MovementBehaviour : IMovementProxy
    //    {
    //        internal const float PLAYERJUMPVELOCITY = 6f;
    //        internal const float PLAYERMOVESPEED = 3.5f;
    //        protected const int NULL_CAMERA_LEFT_RIGHT_VALUE = 512;
    //
    //        protected Player _player;
    //        protected BasicMovementPhysics movementPhysics;
    //
    //        protected bool DoHeadbob { get; set; }
    //
    //        private bool _isMovingForward;
    //
    //        public bool IsMovingForward
    //        {
    //            get { return _isMovingForward; }
    //            set
    //            {
    //                _isMovingForward = value;
    //                Messenger.Invoke("PlayerMovementChanged");
    //            }
    //        }
    //
    //        private bool _isMovingBackward;
    //
    //        public bool IsMovingBackward
    //        {
    //            get { return _isMovingBackward; }
    //            set
    //            {
    //                _isMovingBackward = value;
    //                Messenger.Invoke("PlayerMovementChanged");
    //            }
    //        }
    //
    //        private bool _isStrafingLeft;
    //
    //        public bool IsStrafingLeft
    //        {
    //            get { return _isStrafingLeft; }
    //            set
    //            {
    //                _isStrafingLeft = value;
    //                Messenger.Invoke("PlayerMovementChanged");
    //            }
    //        }
    //
    //        private bool _isStrafingRight;
    //
    //        public bool IsStrafingRight
    //        {
    //            get { return _isStrafingRight; }
    //            set
    //            {
    //                _isStrafingRight = value;
    //                Messenger.Invoke("PlayerMovementChanged");
    //            }
    //        }
    //
    //        protected MovementBehaviour(Player player)
    //        {
    //            _player = player;
    //            movementPhysics = new BasicMovementPhysics(player, this);
    //
    //            DoHeadbob = !_player.World.IsServerWorld;
    //        }
    //
    //        internal void ProcessVerticalVelocity(GameTime gameTime)
    //        {
    //            var totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
    //            ProcessVerticalVelocity(totalSeconds);
    //        }
    //
    //        internal virtual void ProcessVerticalVelocity(float totalSeconds)
    //        {
    //        }
    //
    //        internal bool TryToMoveTo(Vector3 moveVector)
    //        {
    //            // TODO: rewrite with AABBs
    //            // Build a "test vector" that is a little longer than the move vector.
    //            float moveLength = moveVector.Length();
    //            if (moveLength > 0)
    //            {
    //                Vector3 testVector = moveVector;
    //                testVector.Normalize();
    //                testVector = testVector * (moveLength + 0.3f);
    //
    //                // Apply this test vector.
    //                Vector3 movePosition = _player.Position + testVector;
    //                Vector3 headMovePosition = _player.HeadPosition + testVector;
    //                Vector3 footMovePosition = _player.Position + testVector;
    //
    //                try
    //                {
    //                    if (!_player.World.GetBlock(movePosition).IsSolid
    //                        && !_player.World.GetBlock(footMovePosition).IsSolid
    //                        && !_player.World.GetBlock(headMovePosition).IsSolid)
    //                    {
    //                        _player.Position += moveVector;
    //
    //                        if (DoHeadbob)
    //                        {
    //                            if (moveVector != Vector3.Zero)
    //                            {
    //                                _player.HeadBob += 0.2;
    //                            }
    //                        }
    //
    //                        return true;
    //                    }
    //                }
    //                catch (NullReferenceException)
    //                {
    //                    // Player is falling and we're on the edge of chunks or chunks just didn't generated yet.
    //                    //                    throw;
    //                }
    //            }
    //
    //            return false;
    //        }
    //
    //        internal virtual void Update(GameTime gameTime)
    //        {
    //            ProcessVerticalVelocity(gameTime);
    //            movementPhysics.Move(gameTime);
    //        }
    //
    //        internal void Update(float totalSeconds, NetworkPlayerDescription playerDescription)
    //        {
    //            ProcessVerticalVelocity(totalSeconds);
    //            movementPhysics.Move(totalSeconds, playerDescription);
    //        }
    //
    //        internal Vector3 UpdateMoveVector(GameTime gameTime, Vector3 moveVector)
    //        {
    //            return UpdateMoveVector((float)gameTime.ElapsedGameTime.TotalSeconds, moveVector);
    //        }
    //
    //        internal virtual Vector3 UpdateMoveVector(float totalSeconds, Vector3 moveVector, float cameraLeftRightRotation = NULL_CAMERA_LEFT_RIGHT_VALUE)
    //        {
    //            return moveVector;
    //        }
    //
    //        internal virtual void Jump()
    //        {
    //        }
    //
    //        internal virtual void Jump(bool quiet)
    //        {
    //        }
    //
    //        internal virtual Vector3 MoveForward(Vector3 moveVector)
    //        {
    //            return Vector3.Zero;
    //        }
    //
    //        internal virtual Vector3 MoveBackward(Vector3 moveVector)
    //        {
    //            return Vector3.Zero;
    //        }
    //
    //        internal virtual Vector3 StrafeLeft(Vector3 moveVector)
    //        {
    //            return moveVector + Vector3.Left * 2;
    //        }
    //
    //        internal virtual Vector3 StrafeRight(Vector3 moveVector)
    //        {
    //            return moveVector + Vector3.Right * 2;
    //        }
    //    }
}