using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Players;

namespace WorldsGame.Players.MovementBehaviours
{
    // NOTE: This class, and the whole folder is from old days, and applies to players only, other entities have their own movement behaviours

    // This class is somewhat tightly coupled with MovementBehaviour and should be perceived as part of it,
    // but it can't be part of it, because it's in exile for being in conflict with MovementBehaviour children.
    // Family drama.
    //    internal class BasicMovementPhysics
    //    {
    //        private readonly Player _player;
    //        private readonly MovementBehaviour _movementBehaviour;
    //
    //        // I really don't like this being a field, but will keep it like this for a while
    //        private Vector3 _moveVector;
    //
    //        internal BasicMovementPhysics(Player player, MovementBehaviour movementBehaviour)
    //        {
    //            _player = player;
    //            _movementBehaviour = movementBehaviour;
    //        }
    //
    //        internal void Move(GameTime gameTime)
    //        {
    //            ClearMoveVector();
    //            CheckHeadAndFoot();
    //            ProcessSidesMovement(_movementBehaviour);
    //
    //            _moveVector = _movementBehaviour.UpdateMoveVector(gameTime, _moveVector);
    //
    //            TryToMoveTo();
    //        }
    //
    //        // Multiplayer style! Actually used for interpolation and stuff
    //        // Also, doubled to not overuse ifs
    //        internal void Move(float totalSeconds, NetworkPlayerDescription playerDescription)
    //        {
    //            ClearMoveVector();
    //            CheckHeadAndFoot();
    //            ProcessSidesMovement(playerDescription);
    //
    //            _moveVector = _movementBehaviour.UpdateMoveVector(totalSeconds, _moveVector, playerDescription.LeftRightRotation);
    //
    //            TryToMoveTo();
    //        }
    //
    //        private void TryToMoveTo()
    //        {
    //            // Attempt to move, doing collision stuff.
    //            if (_movementBehaviour.TryToMoveTo(_moveVector))
    //            {
    //            }
    //            else if (!_movementBehaviour.TryToMoveTo(new Vector3(0, 0, _moveVector.Z)))
    //            {
    //            }
    //            else if (!_movementBehaviour.TryToMoveTo(new Vector3(_moveVector.X, 0, 0)))
    //            {
    //            }
    //            else if (!_movementBehaviour.TryToMoveTo(new Vector3(0, _moveVector.Y, 0)))
    //            {
    //            }
    //        }
    //
    //        private void ClearMoveVector()
    //        {
    //            _moveVector = new Vector3();
    //        }
    //
    //        private void CheckHeadAndFoot()
    //        {
    //            bool isHeadBlockSolid = _player.HeadBlock != null && _player.HeadBlock.IsSolid;
    //            bool isFootBlockSolid = _player.FootBlock == null || _player.FootBlock.IsSolid;
    //
    //            if (isFootBlockSolid)
    //            {
    //                CheckLegsStuckBelow();
    //            }
    //
    //            if (isHeadBlockSolid)
    //            {
    //                CheckHeadStuckAbove();
    //            }
    //        }
    //
    //        private void ProcessSidesMovement(IMovementProxy movementProxy)
    //        {
    //            if (movementProxy.IsMovingForward)
    //            {
    //                _moveVector = _movementBehaviour.MoveForward(_moveVector);
    //            }
    //            if (movementProxy.IsMovingBackward)
    //            {
    //                _moveVector = _movementBehaviour.MoveBackward(_moveVector);
    //            }
    //            if (movementProxy.IsStrafingLeft)
    //            {
    //                _moveVector = _movementBehaviour.StrafeLeft(_moveVector);
    //            }
    //            if (movementProxy.IsStrafingRight)
    //            {
    //                _moveVector = _movementBehaviour.StrafeRight(_moveVector);
    //            }
    //        }
    //
    //        private void CheckLegsStuckBelow()
    //        {
    //            // If the player is stuck in the ground, bring them out.
    //            // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
    //            try
    //            {
    //                int index = 1;
    //                while (_player.FootBlock.IsSolid && index <= 10)
    //                {
    //                    _player.YPosition += 0.1f;
    //                    index++;
    //                }
    //            }
    //            catch (NullReferenceException)
    //            {
    //                // Somehow FootBlock changed to null
    //            }
    //        }
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
    //    }
}