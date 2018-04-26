using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Items.Behaviours;

namespace WorldsGame.Playing.Items.Inventory
{
    /// <summary>
    /// Inventory container component
    /// </summary>
    internal class InventoryComponent : IEntityComponent
    {
        private IInventoryBehaviour _inventoryBehaviour;

        internal Inventory Inventory { get; set; }

        internal Entity Entity { get; set; }

        internal InventoryComponent(Entity entity, Inventory inventory)
        {
            Entity = entity;
            Inventory = inventory;

            Inventory.OnInventoryUpdated += InventoryUpdated;
        }

        private void InventoryUpdated(int index)
        {
            if (_inventoryBehaviour == null)
            {
                _inventoryBehaviour = Entity.GetChildBehaviour<IInventoryBehaviour>();
            }

            _inventoryBehaviour.InventoryUpdated(index);
        }

        public void Dispose()
        {
            _inventoryBehaviour = null;
            Inventory.Dispose();
        }
    }
}