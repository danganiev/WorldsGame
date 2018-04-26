using System;
using System.Collections.Generic;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Effects;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.Players;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Terrain;

namespace WorldsGame.Models.Tools
{
    // This is a basic "player action class". It's called Tool due to
    // development history, and I like the name for it's essence capability.

    /// <summary>
    /// Basic tool
    /// </summary>
    internal abstract class EntityTool : IDisposable
    {
        internal Entity Entity { get; private set; }

        internal virtual void DoPrimaryAction()
        {
        }

        internal virtual void DoSecondaryAction()
        {
        }

        internal virtual void ChangeItem(string itemName)
        {
        }

        protected EntityTool(Entity entity)
        {
            Entity = entity;
        }

        public virtual void Dispose()
        {
        }
    }

    // Basic tool that works with entities
    internal class BasicEntityTool : EntityTool
    {
        protected string itemName;
        protected Entity toolEntity;

        internal BasicEntityTool(Entity entity)
            : base(entity)
        {
        }

        internal override void DoPrimaryAction()
        {
            if (itemName == null)
            {
                return;
            }

            var effect = ItemHelper.Action1Effects[itemName.ToLower()];

            World world = Entity.GetConstantComponent<WorldComponent>().World;

            Dictionary<Entity, List<Effect>> effectTargets = world.GetEffectTargets(effect, Entity);

            if (ItemHelper.Get(itemName).ItemQuality == ItemQuality.Consumable)
            {
                // Inventory should be possible to get from entity
                Entity.GetComponent<InventoryComponent>().Inventory.SubtractQuantityFromSelected();
            }

            try
            {
                foreach (KeyValuePair<Entity, List<Effect>> effectTarget in effectTargets)
                {
                    EffectApplier.ApplyEffects(effectTarget.Key, effectTarget.Value);
                }
            }
            catch (Exception)
            {
                if (ItemHelper.Get(itemName).ItemQuality == ItemQuality.Consumable)
                {
                    Entity.GetComponent<InventoryComponent>().Inventory.IncreaseQuantityOnSelected(itemName);
                }
                throw;
            }
        }

        internal override void DoSecondaryAction()
        {
        }

        internal override void ChangeItem(string itemName_)
        {
            //            if (itemName != itemName_)
            //            {
            //                if (toolEntity != null)
            //                {
            //                    toolEntity.RemoveSelf();
            //                }
            //
            //                if (itemName_ != null)
            //                {
            //                    itemName = itemName_;
            //                    toolEntity = PlayerToolEntityTemplate.Instance.BuildEntity(
            //                        Entity.GetComponent<WorldComponent>().World.EntityWorld, Player, itemName_);
            //                }
            //                else
            //                {
            //                    itemName = null;
            //                    toolEntity = null;
            //                }
            //            }
        }
    }

    /// <summary>
    /// Player-specific tool class
    /// </summary>
    internal class PlayerTool : BasicEntityTool
    {
        internal Player Player { get; private set; }

        internal PlayerTool(Player player)
            : base(player.PlayerEntity)
        {
            Player = player;
        }

        internal override void ChangeItem(string itemName_)
        {
            if (itemName != itemName_)
            {
                if (toolEntity != null)
                {
                    toolEntity.RemoveSelf();
                }

                if (itemName_ != null)
                {
                    itemName = itemName_;
                    toolEntity = PlayerToolEntityTemplate.Instance.BuildEntity(
                       Entity.GetConstantComponent<WorldComponent>().World.EntityWorld, Player, itemName_);
                }
                else
                {
                    itemName = null;
                    toolEntity = null;
                }
            }
        }
    }
}