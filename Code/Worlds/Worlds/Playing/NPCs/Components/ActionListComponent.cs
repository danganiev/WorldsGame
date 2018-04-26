using WorldsGame.Playing.Entities;
using WorldsGame.Playing.NPCs.AI;

namespace WorldsGame.Playing.NPCs
{
    internal class ActionListComponent : IEntityComponent
    {
        internal ActionList ActionList { get; set; }

        internal ActionListComponent(Entity entity)
        {
            ActionList = new ActionList(entity);

            ActionList.InsertAtStart(new Think(ActionList));
        }

        public void Dispose()
        {
            ActionList = null;
        }
    }
}