using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldsGame.Playing.Entities.Components
{
    internal class OnDieComponent : IEntityComponent
    {
        internal event Action<Entity> OnDie;

        internal OnDieComponent(Action<Entity> onDie)
        {
            OnDie = onDie;
        }

        internal void ToggleOnDie(Entity entity)
        {
            OnDie(entity);
        }

        public void Dispose()
        {
        }
    }
}
