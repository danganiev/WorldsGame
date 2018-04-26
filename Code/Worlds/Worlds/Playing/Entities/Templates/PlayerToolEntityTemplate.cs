using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using NLog;

using WorldsGame.Models.Tools;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.Players;
using WorldsGame.Terrain;

namespace WorldsGame.Playing.Entities.Templates
{
    /// <summary>
    /// The tool in the hands of one of the players
    /// </summary>
    internal sealed class PlayerToolEntityTemplate : IEntityTemplate
    {
        private static readonly Lazy<PlayerToolEntityTemplate> INSTANCE =
            new Lazy<PlayerToolEntityTemplate>(() => new PlayerToolEntityTemplate());

        public static PlayerToolEntityTemplate Instance { get { return INSTANCE.Value; } }

        private PlayerToolEntityTemplate()
        {
        }

        public Entity BuildEntity(EntityWorld entityWorld, params object[] args)
        {
            Entity entity = entityWorld.CreateEntity();            
            
            //            World world = entityWorld.World;

            if (args.Length != 2)
            {
                throw new ArgumentException();
            }

            var player = (Player)args[0];
            var itemName = ((string)args[1]).ToLower();

            CompiledItem item = ItemHelper.Get(itemName);
            var buffersComponent = new CustomModelBuffersComponent();
            CompiledCuboid itemHolder;

            if (player.IsClientPlayer && player.IsFirstPerson)
            {
                itemHolder = ItemHelper.Items[itemName].ItemHolderCuboid ?? ItemHelper.DefaultItemHolderCuboid;
            }
            else
            {
                itemHolder = CharacterHelper.PlayerCharacter.ItemHolderCuboids.ContainsKey(itemName)
                                                ? CharacterHelper.PlayerCharacter.ItemHolderCuboids[itemName]
                                                : CharacterHelper.PlayerCharacter.DefaultItemHolderCuboid;
            }

            if (itemHolder == null)
            {
                WorldsGame.Logger.Log(LogLevel.Warn, "Item holder was null");
                return null;
            }

            CustomModelHolder adjustedItem = itemHolder.Adjust(item);

            CustomModelBuffersComponent.CreateBuffers(entityWorld, buffersComponent, adjustedItem);

            entity.AddComponent(buffersComponent);

            entity.AddBehaviour(typeof(ToolBehaviour));

            entity.AddComponent(new ToolComponent(player.PlayerEntity));
            entity.AddComponent(new PositionComponent(player.Position));
            entity.AddComponent(new ScaleAndRotateComponent());
            entity.AddComponent(new BoundingBoxComponent(item.MinVertice, item.MaxVertice));

            if (player.IsClientPlayer && player.IsFirstPerson)
            {
                entity.AddBehaviour(typeof(FirstPersonItemModelBehaviour));

                Dictionary<AnimationType, ComputedAnimation> animations = ItemHelper.FirstPersonAnimations[itemName];

                if (animations == null)
                {
                    entity.AddComponent(new AnimationComponent(ItemHelper.DefaultFirstPersonAnimations, itemName: itemName));
                }
                else
                {
                    entity.AddComponent(new AnimationComponent(animations, itemName: itemName));
                }
                entity.AddComponent(new CustomModelComponent
                {
                    //                    AdditionalRotationMatrix = Matrix.CreateRotationY((float)Math.PI),
                    AdditionalTranslationMatrix = Matrix.CreateTranslation(itemHolder.Translation)
                });
            }
            else
            {
                entity.AddBehaviour(typeof(CustomModelBehaviour));

                entity.AddComponent(
                    new AnimationComponent(
                        CharacterHelper.BasicAnimations[CharacterHelper.PlayerCharacter.Name.ToLowerInvariant()],
                        itemName: itemName));
                entity.AddComponent(new CustomModelComponent());
            }

            entity.AddBehaviour(typeof(ModelAnimationBehaviour));

            return entity;
        }
    }
}