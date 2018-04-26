using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities.Components
{
    internal class BoundingBoxComponent : IEntityComponent
    {
        private Vector3 _position;
        private BoundingBox _boundingBox;

        internal Vector3 MinVertice { get; set; }

        internal Vector3 MaxVertice { get; set; }

        internal BoundingBoxComponent(Vector3 minVertice, Vector3 maxVertice)
        {
            MinVertice = new Vector3(minVertice.X, 0, minVertice.Z);
            MaxVertice = new Vector3(maxVertice.X, MaxVertice.Y - MinVertice.Y, maxVertice.Z);
        }

        internal BoundingBox GetBoundingBox(Vector3 position)
        {
            if (position == _position)
            {
                return _boundingBox;
            }

            _position = position;
            _boundingBox = new BoundingBox(MinVertice + position, MaxVertice + position);

            return _boundingBox;
        }

        public void Dispose()
        {
        }
    }
}