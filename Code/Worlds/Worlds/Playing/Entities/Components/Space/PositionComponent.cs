using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities.Components
{
    /// <summary>
    /// The classic position component
    /// </summary>
    internal class PositionComponent : IEntityComponent
    {
        private Vector3 _position;

        internal Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        internal float YPosition
        {
            get { return Position.Y; }
            set { _position.Y = value; }
        }

        internal PositionComponent()
            : this(new Vector3())
        {
        }

        internal PositionComponent(Vector3 position)
        {
            Position = position;
        }

        public void Dispose()
        {
        }
    }
}