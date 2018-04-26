using Microsoft.Xna.Framework;

//using NLog;

namespace WorldsGame.Playing.Players
{
    internal class PlayerMovementInterpolator : IMovementInterpolator
    {
        private int _framesLeft;
        private Vector3 _distanceDeltaPerFrame;

        //        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal Vector3 PositionToInterpolateTo { get; set; }

        internal Player Player { get; private set; }

        internal PlayerMovementInterpolator(Player player)
        {
            Player = player;
        }

        public void UpdateInterpolationData(Vector3 newPosition)
        {
            PositionToInterpolateTo = newPosition;

            Vector3 distanceDelta = PositionToInterpolateTo - Player.Position;
            _distanceDeltaPerFrame = distanceDelta / Player.FRAMES_AMOUNT_TO_INTERPOLATE;

            _framesLeft = Player.FRAMES_AMOUNT_TO_INTERPOLATE;
        }

        public void Interpolate()
        {
            if (_framesLeft < 0)
            {
                return;
            }

            Player.Position += _distanceDeltaPerFrame;

            _framesLeft--;
        }
    }
}