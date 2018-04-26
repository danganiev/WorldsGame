using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using WorldsGame.Gamestates;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Terrain;
using WorldsGame.Utils;

namespace WorldsGame.Playing.Entities
{
    /// <summary><para>The Entity World Class.</para>
    /// <para>Main interface of the Entity System.</para></summary>
    internal sealed class EntityWorld
    {
        private readonly World _world;

        /// <summary>The deleted.</summary>
        private readonly List<Entity> _deleted;

        private double _previousUpdateTimeStep50;

        /// <summary>Gets the entity manager.</summary>
        /// <value>The entity manager.</value>
        internal EntityManager EntityManager { get; private set; }

        internal BehaviourManager BehaviourManager { get; private set; }

        internal EntityComponentManager ComponentManager { get; private set; }

        internal Dictionary<Type, SaveableBehaviour> SaveableBehavioursCache { get; private set; }

        internal World World
        {
            get { return _world; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityWorld" /> class.
        /// </summary>
        internal EntityWorld(World world)
        {
            _world = world;

            _deleted = new List<Entity>();
            EntityManager = new EntityManager(this);
            BehaviourManager = new BehaviourManager();
            ComponentManager = new EntityComponentManager(this);
            SaveableBehavioursCache = new Dictionary<Type, SaveableBehaviour>();

            PopulateSaveableBehavioursCache();
        }

        private void PopulateSaveableBehavioursCache()
        {
            SaveableBehavioursCache[typeof(ItemEntityTemplate)] = new ItemEntitySaveBehaviour();
        }

        /// <summary>Clears this instance.</summary>
        internal void Clear()
        {
            EntityManager.Clear();
            BehaviourManager.Clear();
            ComponentManager.Clear();
        }

        /// <summary>Creates the entity.</summary>
        /// <param name="entityUniqueId">The desired unique id of this Entity. if null, <c>artemis</c> will create an unique ID.
        /// This value can be accessed by using the property uniqueID of the Entity</param>
        /// <returns>A new entity.</returns>
        internal Entity CreateEntity(long? entityUniqueId = null)
        {
            return EntityManager.Create(entityUniqueId);
        }

        /// <summary>Deletes the entity.</summary>
        /// <param name="entity">The entity.</param>
        internal void RemoveEntity(Entity entity)
        {
            Debug.Assert(entity != null, "Entity must not be null.");

            _deleted.Add(entity);
        }

        /// <summary>Gets the entity.</summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns>The specified entity.</returns>
        internal Entity GetEntity(int entityId)
        {
            Debug.Assert(entityId >= 0, "Id must be at least 0.");

            return EntityManager.GetEntity(entityId);
        }

        /// <summary>Loads the state of the entity.</summary>
        /// <param name="behaviours">The components.</param>
        /// <returns>The <see cref="Entity" />.</returns>
        internal Entity LoadEntityState(IEnumerable<Type> behaviours)
        {
            Debug.Assert(behaviours != null, "Behaviours must not be null.");

            Entity entity = CreateEntity();

            foreach (Type behaviour in behaviours)
            {
                entity.AddBehaviour(behaviour);
            }

            return entity;
        }

        /// <summary>Updates the EntityWorld.</summary>
        internal void Update(GameTime gameTime)
        {
            if (_deleted.Count > 0)
            {
                for (int index = _deleted.Count - 1; index >= 0; --index)
                {
                    Entity entity = _deleted[index];

                    BehaviourManager.Remove(entity);
                    EntityManager.Remove(entity);
                    ComponentManager.Remove(entity.Id);
                }

                _deleted.Clear();
            }

            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;
            bool is50MS = totalMilliseconds - _previousUpdateTimeStep50 >= PlayingState.GAME_STEP_50MS;

            if (is50MS)
            {
                _previousUpdateTimeStep50 = totalMilliseconds;
            }

            EntityManager.Update(gameTime);

            foreach (Entity activeEntity in EntityManager.ActiveEntities.Values)
            {
                if (!activeEntity.IsActive)
                {
                    continue;
                }

                BehaviourManager.Update(gameTime, activeEntity);

                if (is50MS)
                {
                    BehaviourManager.Update50(gameTime, activeEntity);
                }
            }
        }

        /// <summary>Draws the EntityWorld.</summary>
        internal void Draw(GameTime gameTime)
        {
            foreach (Entity activeEntity in EntityManager.ActiveEntities.Values)
            {
                BehaviourManager.Draw(gameTime, activeEntity);
            }
        }

        internal void LoadContent()
        {
        }

        internal void UnloadContent()
        {
        }
    }
}