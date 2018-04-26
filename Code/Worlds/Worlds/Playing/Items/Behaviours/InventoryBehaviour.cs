using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Playing.Entities;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Items.Behaviours
{
    internal interface IInventoryBehaviour : IEntityBehaviour
    {
        void InventoryUpdated(int index);
    }

    internal class SinglePlayerInventoryBehaviour : EntityBehaviour, IInventoryBehaviour
    {
        public void InventoryUpdated(int index)
        {
            Messenger.Invoke("PlayerInventoryUpdate", index);
        }
    }
}