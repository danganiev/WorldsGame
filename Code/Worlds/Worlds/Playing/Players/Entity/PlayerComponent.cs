using System;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Physics.Behaviours;

namespace WorldsGame.Playing.Players.Entity
{
    internal class PlayerComponent : IEntityComponent
    {
        internal Player Player { get; set; }

        internal bool IsClientPlayer { get { return Player.IsClientPlayer; } }

        internal Type MovementBehaviourType { get; set; }

        internal PlayerComponent(Player player)
        {
            Player = player;
            MovementBehaviourType = typeof(WalkBehaviour);
        }

        public void Dispose()
        {
            Player = null;
        }
    }
}