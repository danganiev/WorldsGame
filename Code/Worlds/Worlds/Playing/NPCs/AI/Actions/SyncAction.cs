using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.NPCs.AI
{
    /// <summary>
    /// A useful type of action is one that blocks until it is the first action in the list.
    /// This is useful when a few different non-blocking actions are being run, but you aren't sure what order they will finish in.
    /// The synchronize action ensures that no previous non-blocking actions are currently running before continuing.
    /// </summary>
    internal class SyncAction : AIAction
    {
        internal SyncAction(ActionList actionList)
            : base(actionList)
        {
            isBlocking = true;
        }

        internal override void Update(GameTime gameTime)
        {
            if (ownerList.GetFirstAction() == this)
            {
                isFinished = true;
            }
        }
    };
}