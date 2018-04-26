using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Playing.Entities.Components;

namespace WorldsGame.Playing.Entities
{
    /// <summary>
    /// Basic entity class.  Consists of an id and a list of components.  Ideally it
    /// should only be an id, but it's useful to be able to know what components it
    /// is built of and to find them quickly.
    /// </summary>

    /// <summary>
    /// Basic entity class.  Consists of an id and functions to interact with the manager.
    ///  - This provides an interface to interact with an individual entity
    /// </summary>
    public sealed class Entity : IEquatable<Entity>, IComparable<Entity>
    {
        private readonly int _id;

        /// <summary>
        /// Sets and toggles the entity as active or not
        /// </summary>
        internal bool IsActive { get; set; }

        /// <summary>
        /// Entity Id
        /// </summary>
        internal int Id
        {
            get
            {
                return _id;
            }
        }

        // I think, I decided this is for saving only, but I need to google about that.
        // Oh, and this is probably the choice for multiplayer
        internal long UniqueId { get; set; }

        /// <summary>
        /// Mister manager.
        /// </summary>
        internal readonly EntityWorld World;

        /// <summary>
        /// Type of the creator template class of the entity. Can be null
        /// </summary>
        internal Type TemplateType;

        /// <summary>
        /// Construct empty entity
        /// </summary>
        internal Entity(EntityWorld world, int id)
        {
            IsActive = true;
            if (world == null)
            {
                throw new ArgumentNullException("Entity world cannot be null.");
            }

            _id = id;
            World = world;
        }

        /// <summary>
        /// Add a new behaviour to the entity
        /// </summary>
        internal void AddBehaviour(Type behaviour)
        {
            World.BehaviourManager.Add(Id, behaviour);
        }

        /// <summary>
        /// Remove a behaviour from the entity
        /// </summary>
        internal void RemoveBehaviour<T>() where T : IEntityBehaviour
        {
            World.BehaviourManager.Remove<T>(Id);
        }

        /// <summary>
        /// Add a new component to the entity
        /// </summary>
        internal void AddComponent(IEntityComponent component)
        {
            World.ComponentManager.Add(Id, component);
        }

        internal bool HasComponent(Type componentType)
        {
            return World.ComponentManager.Contains(Id, componentType);
        }

        /// <summary>
        /// Remove an component from the entity
        /// </summary>
        internal void RemoveComponent<T>() where T : IEntityComponent
        {
            World.ComponentManager.Remove<T>(Id);
        }

        /// <summary>
        /// Return all behaviours belonging to this entity
        /// </summary>
        internal IEnumerable<IEntityBehaviour> Behaviours()
        {
            return World.BehaviourManager.All(Id);
        }

        internal void RemoveSelf()
        {
            World.RemoveEntity(this);

            var onDie = GetComponent<OnDieComponent>();
            if (onDie != null)
            {
                onDie.ToggleOnDie(this);
            }
        }

        internal void Die()
        {
            RemoveSelf();
        }

        internal void PseudoDie()
        {
            IsActive = false;
        }

        internal void Reset()
        {
            IsActive = true;
        }

        #region IEquatable and IComparable

        /// <summary>
        /// IEquatable
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Equals(Entity other)
        {
            return other.Id == Id;
        }

        /// <summary>
        /// IComparable
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Entity other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion IEquatable and IComparable

        public T GetComponent<T>() where T : IEntityComponent
        {
            return World.ComponentManager.Get<T>(Id);
        }

        public T GetConstantComponent<T>() where T : IEntityComponent
        {
            return World.ComponentManager.GetConstant<T>();
        }

        public T GetBehaviour<T>() where T : IEntityBehaviour
        {
            return World.BehaviourManager.Get<T>();
        }

        /// <summary>
        /// Gets all behaviour implementations of interface T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetChildBehaviour<T>() where T : IEntityBehaviour
        {
            return World.BehaviourManager.GetChild<T>();
        }
    }
}