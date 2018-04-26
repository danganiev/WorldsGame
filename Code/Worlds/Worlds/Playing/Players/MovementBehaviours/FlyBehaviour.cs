using Microsoft.Xna.Framework;

using WorldsGame.Playing.Players;

namespace WorldsGame.Players.MovementBehaviours
{
    //    internal class FlyBehaviour : MovementBehaviour
    //    {
    //        internal FlyBehaviour(Player player)
    //            : base(player)
    //        {
    //            DoHeadbob = false;
    //        }
    //
    //        internal override Vector3 UpdateMoveVector(float totalSeconds, Vector3 moveVector, float cameraLeftRightRotation = NULL_CAMERA_LEFT_RIGHT_VALUE)
    //        {
    //            moveVector *= PLAYERMOVESPEED * totalSeconds;
    //
    //            return moveVector;
    //        }
    //
    //        internal override void ProcessVerticalVelocity(float totalSeconds)
    //        {
    //            _player.YVelocity = 0;
    //        }
    //
    //        internal override Vector3 MoveForward(Vector3 moveVector)
    //        {
    //            moveVector += _player.LookVector * 2;
    //            return moveVector;
    //        }
    //
    //        internal override Vector3 MoveBackward(Vector3 moveVector)
    //        {
    //            moveVector -= _player.LookVector * 2;
    //            return moveVector;
    //        }
    //
    //        internal override Vector3 StrafeLeft(Vector3 moveVector)
    //        {
    //            return moveVector + Vector3.Transform(Vector3.Left * 2, Matrix.CreateRotationY(_player.LeftRightRotation));
    //        }
    //
    //        internal override Vector3 StrafeRight(Vector3 moveVector)
    //        {
    //            return moveVector + Vector3.Transform(Vector3.Right * 2, Matrix.CreateRotationY(_player.LeftRightRotation));
    //        }
    //    }
}