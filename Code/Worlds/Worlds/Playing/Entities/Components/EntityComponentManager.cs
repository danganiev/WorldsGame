using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Models.Tools;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.Physics.Components;
using WorldsGame.Playing.Players.Entity;

namespace WorldsGame.Playing.Entities
{
    /// <summary>
    /// Default EntityAttributeManager - maintains all loaded components and
    /// their associations with entities. Basically a big Dictionary
    /// of dictionaries.
    /// </summary>
    public class EntityComponentManager : IEnumerable<Type>
    {
        private readonly EntityWorld _entityWorld;

        // Hard coding is hard
        private static readonly HashSet<Type> _componentTypes = new HashSet<Type>
        {
//            typeof(CharacterCustomComponent),
            typeof(CustomModelBuffersComponent),
            typeof(TakeableItemComponent),

            typeof(PositionComponent),
            typeof(PhysicsComponent),
            typeof(ScaleAndRotateComponent),
            typeof(CustomModelComponent),
            typeof(NPCComponent),
            typeof(BoundingBoxComponent),
            typeof(PlayerComponent),
            typeof(ActionListComponent),
            typeof(CharacterActorComponent),
            typeof(AnimationComponent),
            typeof(ToolComponent),
            typeof(InventoryComponent),

            typeof(OnDieComponent),
        };

        /// <summary>
        /// Collection of all components, separated by type.
        /// </summary>
        private readonly Dictionary<Type, Dictionary<int, IEntityComponent>> _components;

        private readonly Dictionary<Type, IEntityComponent> _constantComponents;

        private Dictionary<int, IEntityComponent> this[Type t]
        {
            get
            {
                return _components[t];
            }
        }

        internal EntityComponentManager(EntityWorld entityWorld)
        {
            _entityWorld = entityWorld;
            _components = new Dictionary<Type, Dictionary<int, IEntityComponent>>();
            _constantComponents = new Dictionary<Type, IEntityComponent>();

            foreach (Type componentType in _componentTypes)
            {
                _components.Add(componentType, new Dictionary<int, IEntityComponent>());
            }

            AddConstant(new WorldComponent(_entityWorld.World));
            AddConstant(new DroppedItemsComponent(_entityWorld));
        }

        /// <summary>
        /// Add a new component to an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="o"></param>
        internal void Add<T>(int id, T o)
            where T : IEntityComponent
        {
            //            o.Owner = id;   // Ensure the component is owned by the entity
            this[o.GetType()].Add(id, o);
        }

        /// <summary>
        /// Add a collection of components to an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="components"></param>
        internal void Add(int id, IEnumerable<IEntityComponent> components)
        {
            foreach (var component in components)
            {
                Add(id, component);
            }
        }

        internal void AddConstant<T>(T component)
            where T : IEntityComponent
        {
            //            o.Owner = id;   // Ensure the component is owned by the entity
            _constantComponents[component.GetType()] = component;
        }

        /// <summary>
        /// Remove a component from the manager
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(int id)
        {
            foreach (var c in _components.Values)
            {
                c.Remove(id);
            }
        }

        /// <summary>
        /// Remove a component from an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool Remove<T>(int id)
            where T : IEntityComponent
        {
            return this[typeof(T)].Remove(id);
        }

        /// <summary>
        /// Try to get a component from an entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        internal T Get<T>(int id) where T : IEntityComponent
        {
            IEntityComponent o;
            this[typeof(T)].TryGetValue(id, out o);
            return (T)o;
        }

        public T GetConstant<T>() where T : IEntityComponent
        {
            return (T)_constantComponents[typeof(T)];
        }

        /// <summary>
        /// Determine if the entity contains an component type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        internal bool Contains(int id, Type t)
        {
            return this[t].ContainsKey(id);
        }

        #region IEnumerable

        public IEnumerator<Type> GetEnumerator()
        {
            return _components.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable

        internal IEnumerable<IEntityComponent> All(int id)
        {
            var list = new List<IEntityComponent>();

            foreach (KeyValuePair<Type, Dictionary<int, IEntityComponent>> collection in _components)
            {
                if (collection.Value.ContainsKey(id))
                {
                    list.Add(collection.Value[id]);
                }
            }

            return list;
        }

        internal void Clear()
        {
            foreach (KeyValuePair<Type, Dictionary<int, IEntityComponent>> keyValuePair in _components)
            {
                keyValuePair.Value.Clear();
            }
            _components.Clear();
        }
    }
}