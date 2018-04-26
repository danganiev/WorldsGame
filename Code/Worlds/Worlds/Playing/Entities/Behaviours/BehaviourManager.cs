using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldsGame.Models.Tools;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.Items.Behaviours;
using WorldsGame.Playing.NPCs.Behaviours;
using WorldsGame.Playing.Physics.Behaviours;
using WorldsGame.Playing.Players.Entity;

namespace WorldsGame.Playing.Entities
{
    internal class BehaviourManager
    {
        /// <summary>
        /// Collection of all behaviours, separated by type.
        /// </summary>
        private readonly Dictionary<Type, IEntityBehaviour> _behaviours;

        private readonly Dictionary<int, List<Type>> _updateableBehaviours;
        private readonly Dictionary<int, List<Type>> _drawableBehaviours;
        private readonly Dictionary<Type, List<int>> _behaviourToEntityMap;

        internal BehaviourManager()
        {
            _behaviours = new Dictionary<Type, IEntityBehaviour>();
            _updateableBehaviours = new Dictionary<int, List<Type>>();
            _drawableBehaviours = new Dictionary<int, List<Type>>();
            _behaviourToEntityMap = new Dictionary<Type, List<int>>();

            InitializeBehaviours();
        }

        private void InitializeBehaviours()
        {
            // Hard coding is hard
            _behaviours[typeof(CustomModelBehaviour)] = new CustomModelBehaviour();
            _behaviours[typeof(FirstPersonItemModelBehaviour)] = new FirstPersonItemModelBehaviour();
            _behaviours[typeof(TakeableItemBehaviour)] = new TakeableItemBehaviour();
            _behaviours[typeof(PlayerBehaviour)] = new PlayerBehaviour();
            _behaviours[typeof(GravityBehaviour)] = new GravityBehaviour();
            _behaviours[typeof(AIBehaviour)] = new AIBehaviour();
            _behaviours[typeof(WalkBehaviour)] = new WalkBehaviour();
            _behaviours[typeof(ModelAnimationBehaviour)] = new ModelAnimationBehaviour();
            _behaviours[typeof(ToolBehaviour)] = new ToolBehaviour();
            _behaviours[typeof(PhysicsBehaviour)] = new PhysicsBehaviour();
            _behaviours[typeof(CharacterActorBehaviour)] = new CharacterActorBehaviour();
            _behaviours[typeof(SinglePlayerInventoryBehaviour)] = new SinglePlayerInventoryBehaviour();

            foreach (Type entityBehaviour in _behaviours.Keys)
            {
                _behaviourToEntityMap[entityBehaviour] = new List<int>();
            }
        }

        public List<int> GetEntities(Type behaviourType)
        {
            return _behaviourToEntityMap[behaviourType];
        }

        public void Add(int id, Type behaviourType)
        {
            var behaviour = _behaviours[behaviourType];
            _behaviourToEntityMap[behaviourType].Add(id);

            if (behaviour.IsDrawable)
            {
                if (!_drawableBehaviours.ContainsKey(id))
                {
                    _drawableBehaviours[id] = new List<Type>();
                }
                _drawableBehaviours[id].Add(behaviourType);
            }
            if (behaviour.IsUpdateable)
            {
                if (!_updateableBehaviours.ContainsKey(id))
                {
                    _updateableBehaviours[id] = new List<Type>();
                }
                _updateableBehaviours[id].Add(behaviourType);
            }
        }

        internal void Remove(Entity entity)
        {
            Remove(entity.Id);
        }

        public void Remove(int id)
        {
            if (_drawableBehaviours.ContainsKey(id))
            {
                _drawableBehaviours[id].Clear();
                _drawableBehaviours.Remove(id);
            }

            if (_updateableBehaviours.ContainsKey(id))
            {
                _updateableBehaviours[id].Clear();
                _updateableBehaviours.Remove(id);
            }
        }

        public void Remove<T>(int id) where T : IEntityBehaviour
        {
            _behaviourToEntityMap[typeof(T)].Remove(id);

            if (_drawableBehaviours.ContainsKey(id))
            {
                _drawableBehaviours[id].Remove(typeof(T));
            }
            if (_updateableBehaviours.ContainsKey(id))
            {
                _updateableBehaviours[id].Remove(typeof(T));
            }
        }

        public IEntityBehaviour Get(Type type)
        {
            return _behaviours.ContainsKey(type) ? _behaviours[type] : null;
        }

        public T Get<T>() where T : IEntityBehaviour
        {
            Type type = typeof(T);

            IEntityBehaviour behaviour;
            _behaviours.TryGetValue(type, out behaviour);
            return (T)behaviour;
        }

        public T GetChild<T>()
        {
            foreach (KeyValuePair<Type, IEntityBehaviour> keyValuePair in _behaviours)
            {
                if (typeof(T).IsAssignableFrom(keyValuePair.Key))
                {
                    return (T)keyValuePair.Value;
                }
            }

            throw new KeyNotFoundException("No child behaviour of such type has been found");
        }

        public IEnumerable<IEntityBehaviour> All(int id)
        {
            var list = new List<IEntityBehaviour>();

            if (_drawableBehaviours.ContainsKey(id))
            {
                list.AddRange(_drawableBehaviours[id].Select(type => _behaviours[type]));
            }

            if (_updateableBehaviours.ContainsKey(id))
            {
                list.AddRange(_updateableBehaviours[id].Select(type => _behaviours[type]));
            }

            return list.Distinct();
        }

        public void Update(GameTime gameTime, Entity entity)
        {
            if (_updateableBehaviours.ContainsKey(entity.Id))
            {
                foreach (Type behaviour in _updateableBehaviours[entity.Id])
                {
                    _behaviours[behaviour].Update(gameTime, entity);
                }
            }
        }

        public void Update50(GameTime gameTime, Entity entity)
        {
            if (_updateableBehaviours.ContainsKey(entity.Id))
            {
                foreach (Type behaviour in _updateableBehaviours[entity.Id])
                {
                    _behaviours[behaviour].Update50(gameTime, entity);
                }
            }
        }

        public void Draw(GameTime gameTime, Entity entity)
        {
            if (_drawableBehaviours.ContainsKey(entity.Id))
            {
                foreach (Type behaviour in _drawableBehaviours[entity.Id])
                {
                    _behaviours[behaviour].Draw(gameTime, entity);
                }
            }
        }

        public void Clear()
        {
            _behaviours.Clear();
            _drawableBehaviours.Clear();
            _updateableBehaviours.Clear();
        }
    }
}