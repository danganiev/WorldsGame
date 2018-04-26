using System;
using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.Entities
{
    public interface IEntityBehaviour
    {
        bool IsUpdateable { get; }

        bool IsDrawable { get; }

        void Update(GameTime gameTime, Entity owner);

        // updates every 50 ms to decrease load
        void Update50(GameTime gameTime, Entity owner);

        void Draw(GameTime gameTime, Entity owner);
    }

    internal abstract class EntityBehaviour : IEntityBehaviour, IDisposable
    {
        public virtual bool IsUpdateable { get { return true; } }

        public virtual bool IsDrawable { get { return true; } }

        public virtual void Update(GameTime gameTime, Entity owner)
        {
        }

        public virtual void Update50(GameTime gameTime, Entity owner)
        {
        }

        public virtual void Draw(GameTime gameTime, Entity owner)
        {
        }

        public virtual void Dispose()
        {
            
        }
    }
}