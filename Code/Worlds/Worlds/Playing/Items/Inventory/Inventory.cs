using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Playing.Players;
using WorldsGame.Utils;

//using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Items.Inventory
{
    internal class Inventory : IDisposable
    {
        internal const int MAX_ITEMS = 40;

        internal const int MAX_ITEMS_IN_HAND_TRAY = 10;
        internal const int MAX_STACK_COUNT = 64;

        internal static CompiledGameBundle CompiledGameBundle { get; set; }

        // Selected hand slot from 0-9
        private byte _selectedSlot;
        internal byte SelectedSlot
        {
            get { return _selectedSlot; }
            set
            {
                _selectedSlot = value;
                OnSelectionChanged();
            }
        }

        internal List<int> EmptySlots { get; private set; }

        internal List<InventoryItem> Items { get; private set; }

        internal bool IsFull
        {
            get { return EmptySlots.Count == 0; }
        }

        internal int SmallestIndex
        {
            get { return EmptySlots.Min(); }
        }

        internal InventoryItem SelectedItem { get { return Items[SelectedSlot]; } }

        internal event Action<int> OnInventoryUpdated = index => { };
        internal event Action OnSelectionChanged = () => { };

        internal Inventory()
        {
            Items = new List<InventoryItem>();
            EmptySlots = new List<int>();
            OnInventoryUpdated += InventoryUpdated;
            
            Clear();
        }

        private void InventoryUpdated(int index)
        {
            if (index == SelectedSlot)
            {
                OnSelectionChanged();
            }
        }
        
        internal void AddItem(string name, int quantity)
        {
            // Find first non-full stack of this type of items
            // If not found then add to empty slot
            // If something left, then repeat enough times
            // If inventory is full then throw or return false, and rollback

            CompiledItem item = CompiledGameBundle.GetItem(name);

            if (item == null)
            {
                throw new InvalidOperationException("Item with this name doesn't exist.");
            }

            int maxStackCount = item.MaxStackCount;

            int index = Items.FindIndex(inventoryItem => (inventoryItem != null && inventoryItem.Name == name));

            if (index == -1)
            {
                AddToEmptySlot(quantity, item);
                if (SelectedItem == null)
                {
                }
            }
            else
            {
                int leftoverQuantity = quantity;

                List<int> stackIndices = Enumerable.Range(0, Items.Count).Where(
                    i => Items[i] != null && Items[i].Name == name).OrderBy(i => i).ToList();

                int alreadyFilledSpace = Items.Where(
                    inventoryItem => inventoryItem != null && inventoryItem.Name == name).Sum(
                    inventoryItem => inventoryItem.Quantity);

                if (alreadyFilledSpace > leftoverQuantity && IsFull)
                {
                    throw new InvalidOperationException("Inventory is full");
                }

                while (leftoverQuantity > 0 && stackIndices.Count > 0)
                {
                    int stackIndex = stackIndices[0];
                    stackIndices.Remove(stackIndex);

                    int newQuantity = maxStackCount - Items[stackIndex].Quantity;

                    if (newQuantity > leftoverQuantity)
                    {
                        newQuantity = leftoverQuantity;
                    }

                    Items[stackIndex].Quantity += newQuantity;

                    leftoverQuantity -= newQuantity;

                    //                    Messenger.Invoke("PlayerInventoryUpdate", stackIndex);
                    OnInventoryUpdated(stackIndex);
                }

                if (leftoverQuantity > 0 && !IsFull)
                {
                    AddToEmptySlot(leftoverQuantity, item);
                }
            }
        }

        private void AddToEmptySlot(int quantity, CompiledItem item)
        {
            if (IsFull)
            {
                throw new InvalidOperationException("Inventory is full");
            }

            InventoryItem newItem;

            if (quantity <= item.MaxStackCount)
            {
                newItem = new InventoryItem
                {
                    Name = item.Name,
                    Quantity = quantity
                };

                int newIndex = SmallestIndex;
                Items[newIndex] = newItem;

                EmptySlots.Remove(newIndex);

                //                Messenger.Invoke("PlayerInventoryUpdate", newIndex);
                OnInventoryUpdated(newIndex);
            }
            else
            {
                int leftoverQuantity = quantity - item.MaxStackCount;
                newItem = new InventoryItem
                {
                    Name = item.Name,
                    Quantity = item.MaxStackCount
                };

                int newIndex = SmallestIndex;
                Items[newIndex] = newItem;

                EmptySlots.Remove(newIndex);

                //                Messenger.Invoke("PlayerInventoryUpdate", newIndex);
                OnInventoryUpdated(newIndex);

                AddToEmptySlot(leftoverQuantity, item);
            }
        }

        public InventoryItem TakeItemFromSlot(int index)
        {
            if (EmptySlots.Contains(index))
            {
                return null;
            }

            InventoryItem item = Items[index];
            Items[index] = null;
            EmptySlots.Add(index);

            //            Messenger.Invoke("PlayerInventoryUpdate", index);
            OnInventoryUpdated(index);

            return item;
        }

        public void ForceChangeItemAtSlot(int index, InventoryItem item)
        {
            Items[index] = item;

            EmptySlots.Remove(index);

            //            Messenger.Invoke("PlayerInventoryUpdate", index);
            OnInventoryUpdated(index);
        }

        // Returns leftovers as InventoryItem or null if nothing left
        public InventoryItem AddItemToSlot(int index, InventoryItem item)
        {
            if (IsFull)
            {
                return item;
            }

            CompiledItem compiledItem = ItemHelper.Get(item.Name);

            int leftoverQuantity = 0;

            // Putting to filled slot
            if (Items[index] != null)
            {
                // Putting to filled with same items
                if (Items[index].Name == item.Name)
                {
                    if (Items[index].Quantity < compiledItem.MaxStackCount)
                    {
                        leftoverQuantity = compiledItem.MaxStackCount - Items[index].Quantity;

                        if (item.Quantity > leftoverQuantity)
                        {
                            Items[index].Quantity += leftoverQuantity;
                        }
                        else
                        {
                            Items[index].Quantity += item.Quantity;
                        }

                        leftoverQuantity = item.Quantity - leftoverQuantity;
                    }
                    else
                    {
                        leftoverQuantity = item.Quantity;
                    }
                }
            }
            else
            // Putting to empty slot
            {
                Items[index] = item;
            }

            EmptySlots.Remove(index);

            //            Messenger.Invoke("PlayerInventoryUpdate", index);
            OnInventoryUpdated(index);

            if (leftoverQuantity > 0)
            {
                return new InventoryItem { Name = item.Name, Quantity = leftoverQuantity };
            }

            return null;
        }

        public void Clear()
        {
            Items.Clear();
            EmptySlots.Clear();

            for (int i = 0; i < MAX_ITEMS; i++)
            {
                Items.Add(null);
                EmptySlots.Add(i);
            }
        }

        public int GetQuantityDiff(int quantity, string itemName)
        {
            return ItemHelper.GetQuantityDiff(quantity, itemName);
        }

        public void RefillItems(List<InventoryItem> items)
        {
            Clear();

            for (int i = 0; i < MAX_ITEMS; i++)
            {
                if (i < items.Count)
                {
                    Items[i] = items[i];

                    if (items[i] != null)
                    {
                        EmptySlots.Remove(i);
                    }

                    //                    Messenger.Invoke("PlayerInventoryUpdate", i);
                    OnInventoryUpdated(i);
                }
            }
        }

        public void DropEverything(Player player)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                InventoryItem inventoryItem = Items[i];

                if (inventoryItem != null)
                {
                    ItemEntityTemplate.Instance.BuildEntity(
                        player.World.EntityWorld, inventoryItem.Name, inventoryItem.Quantity,
                        player.Position + player.LookVector,
                        new Vector3(
                            player.LookVector.X * RandomNumberGenerator.GetInt(0, 2) *
                            (float)Math.Pow(-1, RandomNumberGenerator.GetInt(0, 1)),
                            1,
                            player.LookVector.Z * RandomNumberGenerator.GetInt(0, 2) *
                            (float)Math.Pow(-1, RandomNumberGenerator.GetInt(0, 1))) * RandomNumberGenerator.GetInt(1, 4));
                }
            }

            Clear();

            for (int i = 0; i < MAX_ITEMS_IN_HAND_TRAY; i++)
            {
                //                Messenger.Invoke("PlayerInventoryUpdate", i);
                OnInventoryUpdated(i);
            }
        }

        public void SubtractQuantity(int index)
        {
            if (Items[index] == null)
            {
                return;
            }

            if (Items[index].Quantity == 1 || Items[index].Quantity == 0)
            {
                Items[index].Quantity = 0;
                Items[index] = null;
                EmptySlots.Add(index);
            }
            else
            {
                Items[index].Quantity--;
            }

            //            Messenger.Invoke("PlayerInventoryUpdate", index);
            OnInventoryUpdated(index);
        }

        public void IncreaseQuantity(int index, string name)
        {
            CompiledItem item = CompiledGameBundle.GetItem(name);
            int maxStackCount = item.MaxStackCount;

            if (Items[index] == null || Items[index].Quantity == 0)
            {
                Items[index] = new InventoryItem
                {
                    Name = name,
                    Quantity = 1
                };
            }
            else if (Items[index].Quantity < maxStackCount)
            {
                Items[index].Quantity++;           
            }
            
            OnInventoryUpdated(index);
        }

        public void SubtractQuantityFromSelected()
        {
            SubtractQuantity(SelectedSlot);
        }

        public void IncreaseQuantityOnSelected(string name)
        {
            IncreaseQuantity(SelectedSlot, name);
        }        

        public void Dispose()
        {
            OnInventoryUpdated = null;
            OnSelectionChanged = null;
        }
    }
}