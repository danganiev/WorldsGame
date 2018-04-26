using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities
{
    internal class TakeableItemComponent : IEntityComponent
    {
        internal string Name { get; set; }

        internal int Quantity { get; set; }

        internal TakeableItemComponent(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }

        public void Dispose()
        {
        }
    }
}