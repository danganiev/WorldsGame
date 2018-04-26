using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Terrain;

namespace WorldsGame.Playing.Entities
{
    internal class DroppedItemsComponent : IEntityComponent
    {
        private readonly List<int> _items;
        private readonly EntityWorld _entityWorld;

        internal DroppedItemsComponent(EntityWorld entityWorld)
        {
            _entityWorld = entityWorld;
            _items = new List<int>();
        }

        internal void AddItem(Entity entity)
        {
            _items.Add(entity.Id);
        }

        internal List<int> SearchForItems(Vector3 position, float radius = 0.5f)
        {
            if (_items.Count == 0)
            {
                return null;
            }

            var result = new List<int>();

            foreach (int item in _items)
            {
                var itemPosition = _entityWorld.GetEntity(item).GetComponent<PositionComponent>().Position;
                // Since radius is < 1 there might be problems here, but simple calculations
                // in calculator prove otherwise at least
                if (Vector3.DistanceSquared(position, itemPosition) < radius * radius)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        internal void RemoveItem(Entity entity)
        {
            _items.Remove(entity.Id);
        }

        public void Dispose()
        {
        }
    }
}