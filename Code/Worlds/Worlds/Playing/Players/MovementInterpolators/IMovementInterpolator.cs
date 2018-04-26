using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Players
{
    internal interface IMovementInterpolator
    {
        void Interpolate();

        void UpdateInterpolationData(Vector3 newPosition);
    }
}