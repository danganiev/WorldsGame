using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs.Behaviours;
using WorldsGame.Playing.Physics.Behaviours;
using WorldsGame.Playing.Physics.Components;

namespace WorldsGame.Playing.Entities.Templates
{
    internal sealed class ItemEntityTemplate : IEntityTemplate
    {
        private static readonly Lazy<ItemEntityTemplate> INSTANCE =
            new Lazy<ItemEntityTemplate>(() => new ItemEntityTemplate());

        public static ItemEntityTemplate Instance { get { return INSTANCE.Value; } }

        private ItemEntityTemplate()
        {
        }

        public Entity BuildEntity(EntityWorld entityWorld, params object[] args)
        {
            Entity entity = entityWorld.CreateEntity();

            entity.TemplateType = GetType();

            if (args.Length < 3)
            {
                return null;
            }

            var name = (string)args[0];
            var quantity = (int)args[1];
            var position = (Vector3)args[2];

            var velocity = new Vector3(0);

            if (args.Length >= 4)
            {
                velocity = (Vector3)args[3];
            }

            AddBuffersComponent(entityWorld, entity, name);
            entity.AddComponent(new PositionComponent(position));
            entity.AddComponent(new TakeableItemComponent(name, quantity));
            entity.AddComponent(new ScaleAndRotateComponent(0.5f, 0));
            entity.AddComponent(new CustomModelComponent());
            entity.AddComponent(new BoundingBoxComponent(new Vector3(-0.125f, 0.25f, -0.125f), new Vector3(0.125f, 0.5f, 0.125f)));

            var physicsComponent = new PhysicsComponent { Velocity = velocity };
            entity.AddComponent(physicsComponent);

            entity.AddBehaviour(typeof(CustomModelBehaviour));
            entity.AddBehaviour(typeof(TakeableItemBehaviour));
            entity.AddBehaviour(typeof(GravityBehaviour));
            entity.AddBehaviour(typeof(PhysicsBehaviour));

            entity.GetConstantComponent<DroppedItemsComponent>().AddItem(entity);

            return entity;
        }

        private static void AddBuffersComponent(EntityWorld entityWorld, Entity entity, string name)
        {
            CompiledItem item = ItemHelper.Get(name);

            var buffersComponent = new CustomModelBuffersComponent();

            CustomModelBuffersComponent.CreateBuffers(entityWorld, buffersComponent, item);

            entity.AddComponent(buffersComponent);
        }
    }

    internal class ItemEntitySaveBehaviour : SaveableBehaviour
    {
        public class ItemEntitySave
        {
            public string Name;
            public long EntityUID;
            public int Quantity;
            public Vector3 Position;
        }

        internal override string GetJSONForSave(Entity entity)
        {
            var itemComponent = entity.GetComponent<TakeableItemComponent>();
            var result = new ItemEntitySave
            {
                EntityUID = entity.UniqueId,
                Name = itemComponent.Name,
                Quantity = itemComponent.Quantity,
                Position = entity.GetComponent<PositionComponent>().Position
            };

            var results = JsonConvert.SerializeObject(result, Formatting.None);

            return results;
        }

        internal override Entity LoadFromJSON(EntityWorld entityWorld, string json)
        {
            ItemEntitySave save = JsonConvert.DeserializeObject<ItemEntitySave>(json);

            Entity entity = ItemEntityTemplate.Instance.BuildEntity(entityWorld, save.Name, save.Quantity, save.Position);
            entity.UniqueId = save.EntityUID;

            return entity;
        }
    }
}