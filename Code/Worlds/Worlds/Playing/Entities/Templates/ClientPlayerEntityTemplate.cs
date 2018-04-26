using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Behaviours;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.Physics.Behaviours;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.Players.Entity;

namespace WorldsGame.Playing.Entities.Templates
{
    internal sealed class ClientPlayerEntityTemplate : IEntityTemplate
    {
        private static readonly Lazy<ClientPlayerEntityTemplate> INSTANCE =
            new Lazy<ClientPlayerEntityTemplate>(() => new ClientPlayerEntityTemplate());

        public static ClientPlayerEntityTemplate Instance { get { return INSTANCE.Value; } }

        private ClientPlayerEntityTemplate()
        {
        }

        public Entity BuildEntity(EntityWorld entityWorld, params object[] args)
        {
            Entity entity = entityWorld.CreateEntity();

            if (args.Length != 2)
            {
                throw new ArgumentException();
            }

            var player = (Player)args[0];
            var position = (Vector3)args[1];

            var buffersComponent = new CustomModelBuffersComponent();

            CustomModelBuffersComponent.CreateBuffers(entityWorld, buffersComponent, CharacterHelper.PlayerCharacter);

            entity.AddComponent(buffersComponent);

            entity.AddComponent(new PositionComponent(position));
            entity.AddComponent(new ScaleAndRotateComponent());
            entity.AddComponent(new CustomModelComponent());
            entity.AddComponent(new PlayerComponent(player));
            entity.AddComponent(new PhysicsComponent());
            entity.AddComponent(new CharacterActorComponent(entity));
            entity.AddComponent(new BoundingBoxComponent(CharacterHelper.PlayerCharacter.MinVertice, CharacterHelper.PlayerCharacter.MaxVertice));
            entity.AddComponent(new AnimationComponent(CharacterHelper.BasicAnimations[CharacterHelper.PlayerCharacter.Name.ToLowerInvariant()]));
            entity.AddComponent(new InventoryComponent(entity, player.Inventory));

            entity.AddBehaviour(typeof(PlayerBehaviour));
            entity.AddBehaviour(typeof(GravityBehaviour));
            entity.AddBehaviour(typeof(WalkBehaviour));

            // Since this is the client player, we don't draw him for himself while in FPS mode, but we draw him to the others with the help of OtherPlayerEntityTemplate
            entity.AddBehaviour(typeof(CustomModelBehaviour));
            entity.AddBehaviour(typeof(ModelAnimationBehaviour));
            entity.AddBehaviour(typeof(CharacterActorBehaviour));
            entity.GetBehaviour<CharacterActorBehaviour>().Initialize(entity);
            entity.AddBehaviour(typeof(SinglePlayerInventoryBehaviour));

            return entity;
        }

        public Entity ResetEntity(Entity entity)
        {
            entity.GetComponent<PositionComponent>().Position = new Vector3(0, 1, 0);
            CharacterActorBehaviour.ResetAttributesToDefault(entity);

            return entity;
        }
    }
}