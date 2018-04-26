using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.NPCs.Behaviours;
using WorldsGame.Playing.Physics.Behaviours;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players.Entity;

namespace WorldsGame.Playing.Entities.Templates
{
    internal sealed class NPCEntityTemplate : IEntityTemplate
    {
        public const int MAX_NPC_COUNT = 200;

        private static readonly Lazy<NPCEntityTemplate> INSTANCE =
            new Lazy<NPCEntityTemplate>(() => new NPCEntityTemplate());

        public static NPCEntityTemplate Instance { get { return INSTANCE.Value; } }

        public static int NPCCurrentCount { get; set; }

        private NPCEntityTemplate()
        {
        }

        public Entity BuildEntity(EntityWorld entityWorld, params object[] args)
        {
            if (args.Length != 2)
            {
                return null;
            }

            var name = (string)args[0];
            var position = (Vector3)args[1];

            CompiledCharacter character = CharacterHelper.Get(name);

            if (character == null)
            {
                return null;
            }

            Entity entity = entityWorld.CreateEntity();

            AddBuffersComponent(entityWorld, entity, character);

            entity.AddComponent(new NPCComponent(name));
            entity.AddComponent(new PositionComponent(position));
            entity.AddComponent(new ScaleAndRotateComponent(1, 0));
            entity.AddComponent(new CustomModelComponent());
            entity.AddComponent(new PhysicsComponent());
            entity.AddComponent(new BoundingBoxComponent(character.MinVertice, character.MaxVertice));
            entity.AddComponent(new ActionListComponent(entity));
            entity.AddComponent(new AnimationComponent(CharacterHelper.BasicAnimations[name.ToLowerInvariant()]));

            entity.AddComponent(new OnDieComponent(OnNPCEntityDie));

            entity.AddBehaviour(typeof(CustomModelBehaviour));
            entity.AddBehaviour(typeof(GravityBehaviour));
            entity.AddBehaviour(typeof(AIBehaviour));
            entity.AddBehaviour(typeof(WalkBehaviour));
            entity.AddBehaviour(typeof(ModelAnimationBehaviour));            

            NPCCurrentCount += 1;

            return entity;
        }

        private static void AddBuffersComponent(EntityWorld entityWorld, Entity entity, CompiledCharacter character)
        {
            //            CompiledCharacter character = CharacterHelper.Get(name);

            var buffersComponent = new CustomModelBuffersComponent();

            CustomModelBuffersComponent.CreateBuffers(entityWorld, buffersComponent, character);

            entity.AddComponent(buffersComponent);
        }

        private void OnNPCEntityDie(Entity entity)
        {
            NPCCurrentCount -= 1;
        }
    }
}