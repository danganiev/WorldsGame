using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Inventory;

namespace WorldsGame.Playing.Players.Entity
{
    internal class PlayerBehaviour : EntityBehaviour
    {
        public override bool IsDrawable
        {
            get { return false; }
        }

        public override void Update(GameTime gameTime, Entities.Entity owner)
        {
            var playerComponent = owner.GetComponent<PlayerComponent>();
            var positionComponent = owner.GetComponent<PositionComponent>();
            var scaleAndRotateComponent = owner.GetComponent<ScaleAndRotateComponent>();

            positionComponent.Position = playerComponent.Player.Position/* + new Vector3(0, playerComponent.Player.Height / 2 - 0.1f, 0)*/;
            scaleAndRotateComponent.LeftRightRotation = playerComponent.Player.LeftRightRotation;
        }

        public override void Update50(GameTime gameTime, Entities.Entity owner)
        {
            SearchForItems(owner);
        }

        private void SearchForItems(Entities.Entity entity)
        {
            var worldItems = entity.GetConstantComponent<DroppedItemsComponent>();
            var playerComponent = entity.GetComponent<PlayerComponent>();
            //            var positionComponent = entity.GetComponent<PositionComponent>();

            List<int> foundItemEntities = worldItems.SearchForItems(entity.GetComponent<PositionComponent>().Position);

            if (foundItemEntities != null)
            {
                for (int i = 0; i < foundItemEntities.Count; i++)
                {
                    int foundItemEntity = foundItemEntities[i];

                    Entities.Entity itemEntity = entity.World.GetEntity(foundItemEntity);
                    var itemComponent = itemEntity.GetComponent<TakeableItemComponent>();

                    var newSlot = playerComponent.Player.Inventory.SmallestIndex;

                    bool isSomethingSelected = playerComponent.Player.Inventory.SelectedItem == null && newSlot < Inventory.MAX_ITEMS_IN_HAND_TRAY;

                    playerComponent.Player.AddItem(itemComponent.Name, itemComponent.Quantity);

                    worldItems.RemoveItem(itemEntity);

                    itemEntity.RemoveSelf();
                    
                    if (isSomethingSelected)
                    {
                        playerComponent.Player.Inventory.SelectedSlot = (byte)newSlot;
//                        playerComponent.Player.ChangeItemInHand(playerComponent.Player.Inventory.SelectedItem);
                    }
                }
            }
        }

        public void OnPlayerCameraToggle(Entities.Entity owner, bool isFPS)
        {
            if (isFPS)
            {
                owner.RemoveBehaviour<CustomModelBehaviour>();
                owner.RemoveBehaviour<ModelAnimationBehaviour>();
            }
            else
            {
                owner.AddBehaviour(typeof(CustomModelBehaviour));
                owner.AddBehaviour(typeof(ModelAnimationBehaviour));
            }
        }
    }
}