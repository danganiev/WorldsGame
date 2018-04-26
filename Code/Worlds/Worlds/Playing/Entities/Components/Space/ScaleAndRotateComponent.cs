using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities.Components
{
    internal class ScaleAndRotateComponent : IEntityComponent
    {
        internal float Scale { get; set; }

        // Rotation around the Y axis
        internal float LeftRightRotation { get; set; }

        internal float UpDownRotation { get; set; }

        internal ScaleAndRotateComponent()
            : this(1, 0)
        {
        }

        internal ScaleAndRotateComponent(float scale, float leftRightRotation, float upDownRotation = 0)
        {
            Scale = scale;
            LeftRightRotation = leftRightRotation;
            UpDownRotation = upDownRotation;
        }

        public void Dispose()
        {
        }
    }
}