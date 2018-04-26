using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities
{
    /// <summary>The Entity Manager.</summary>
    internal sealed class EntityManager
    {
        /// <summary>The components by type.</summary>
        //        private readonly List<List<IEntityBehaviour>> behavioursByType;

        /// <summary>The removed and available.</summary>
        private readonly List<Entity> removedAndAvailable;

        /// <summary>Map unique id to entities</summary>
        private readonly Dictionary<long, Entity> uniqueIdToEntities;

        /// <summary>The entity world.</summary>
        private readonly EntityWorld _entityWorld;

        /// <summary>The next available id.</summary>
        private int nextAvailableId;

        /// <summary>Gets all active Entities.</summary>
        /// <value>The active entities.</value>
        /// <returns>List of active entities.</returns>

        internal Dictionary<int, Entity> ActiveEntities { get; private set; }

        // Entities to be added on the next update cycle
        internal List<Entity> NewEntities { get; private set; }

        internal Object lock_ = new Object();

        /// <summary>Initializes a new instance of the <see cref="EntityManager" /> class.</summary>
        /// <param name="entityWorld">The entity world.</param>
        internal EntityManager(EntityWorld entityWorld)
        {
            Debug.Assert(entityWorld != null, "EntityWorld must not be null.");

            uniqueIdToEntities = new Dictionary<long, Entity>();
            removedAndAvailable = new List<Entity>();
            //            behavioursByType = new List<List<IEntityBehaviour>>();
            ActiveEntities = new Dictionary<int, Entity>();
            NewEntities = new List<Entity>();
            RemovedEntitiesRetention = 100;
            _entityWorld = entityWorld;
        }

        /// <summary>Occurs when [added entity event].</summary>
        internal event Action<Entity> AddedEntityEvent;

        /// <summary>Occurs when [removed entity event].</summary>
        internal event Action<Entity> RemovedEntityEvent;

#if DEBUG

        /// <summary>Gets how many entities are currently active. Only available in debug mode.</summary>
        /// <value>The active entities count.</value>
        /// <returns>How many entities are currently active.</returns>
        internal int EntitiesRequestedCount { get; private set; }

#endif

        /// <summary>Gets or sets the removed entities retention.</summary>
        /// <value>The removed entities retention.</value>
        internal int RemovedEntitiesRetention { get; set; }

#if DEBUG

        /// <summary>Gets how many entities have been created since start. Only available in debug mode.</summary>
        /// <value>The total created.</value>
        /// <returns>The total number of entities created.</returns>
        internal long TotalCreated { get; private set; }

        /// <summary>Gets how many entities have been removed since start. Only available in debug mode.</summary>
        /// <value>The total removed.</value>
        /// <returns>The total number of removed entities.</returns>
        internal long TotalRemoved { get; private set; }

#endif

        /// <summary>Create a new, "blank" entity.</summary>
        /// <param name="uniqueid">The unique id.</param>
        /// <returns>New entity.</returns>
        internal Entity Create(long? uniqueid = null)
        {
            long id = uniqueid.HasValue ? uniqueid.Value : BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);

            Entity result = removedAndAvailable.Count > 0 ? removedAndAvailable.Last() : null;
            removedAndAvailable.Remove(result);

            if (result == null)
            {
                result = new Entity(_entityWorld, nextAvailableId++);
            }
            else
            {
                result.Reset();
            }

            result.UniqueId = id;

            uniqueIdToEntities[result.UniqueId] = result;
            //            ActiveEntities[result.Id] = result;
            NewEntities.Add(result);
#if DEBUG
            ++EntitiesRequestedCount;

            if (TotalCreated < long.MaxValue)
            {
                ++TotalCreated;
            }
#endif
            if (AddedEntityEvent != null)
            {
                AddedEntityEvent(result);
            }

            return result;
        }

        internal void Update(GameTime gameTime)
        {
            if (NewEntities.Count > 0)
            {
                for (int i = 0; i < NewEntities.Count; i++)
                {
                    Entity newEntity = NewEntities[i];

                    lock (lock_)
                    {
                        ActiveEntities[newEntity.Id] = newEntity;
                    }
                }
            }
        }

        /// <summary>Get the entity for the given entityId</summary>
        /// <param name="entityId">Desired EntityId</param>
        /// <returns>The specified Entity.</returns>
        internal Entity GetEntity(int entityId)
        {
            Debug.Assert(entityId >= 0, "Id must be at least 0.");

            return ActiveEntities[entityId];
        }

        /// <summary>Gets the entity by unique ID. Note: that UniqueId is different from Id.</summary>
        /// <param name="entityUniqueId">The entity unique id.</param>
        /// <returns>The Entity.</returns>
        internal Entity GetEntityByUniqueId(long entityUniqueId)
        {
            Debug.Assert(entityUniqueId != -1, "Id must != -1");
            Entity entity;
            uniqueIdToEntities.TryGetValue(entityUniqueId, out entity);
            return entity;
        }

        /// <summary>Check if this entity is active, or has been deleted, within the framework.</summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns><see langword="true" /> if the specified entity is active; otherwise, <see langword="false" />.</returns>
        internal bool IsActive(int entityId)
        {
            return ActiveEntities[entityId] != null;
        }

        /// <summary>Remove an entity from the entityWorld.</summary>
        /// <param name="entity">Entity you want to remove.</param>
        internal void Remove(Entity entity)
        {
            Debug.Assert(entity != null, "Entity must not be null.");

            ActiveEntities.Remove(entity.Id);
#if DEBUG
            --EntitiesRequestedCount;

            if (TotalRemoved < long.MaxValue)
            {
                ++TotalRemoved;
            }
#endif
            if (removedAndAvailable.Count < RemovedEntitiesRetention)
            {
                removedAndAvailable.Add(entity);
            }

            if (RemovedEntityEvent != null)
            {
                RemovedEntityEvent(entity);
            }

            uniqueIdToEntities.Remove(entity.UniqueId);
        }

        internal void Clear()
        {
            for (int index = 0; index < ActiveEntities.Count; index++)
            {
                lock (lock_)
                {
                    Entity activeEntity = ActiveEntities[index];
                    Remove(activeEntity);
                }
            }

            NewEntities.Clear();
            uniqueIdToEntities.Clear();

            AddedEntityEvent = null;
            RemovedEntityEvent = null;
        }
    }
}