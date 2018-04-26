using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Players
{
    internal class EmptyMovementInterpolator : IMovementInterpolator
    {
        public void Interpolate()
        {
        }

        public void UpdateInterpolationData(Vector3 newPosition)
        {
        }
    }
}