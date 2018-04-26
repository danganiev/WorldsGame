using System;

namespace WorldsGame.Playing.Items.Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public string Name { get; set; }

        public int Quantity { get; set; }
    }
}