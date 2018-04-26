using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.NPCs.AI
{
    /// <summary>
    /// An action that does nothing but delay all actions for a specified amount of time is a very useful thing to have.
    /// The idea is to delay all subsequent actions from taking place until a timer has elapsed.
    /// </summary>
    internal class DelayAction : AIAction
    {
        internal DelayAction(ActionList actionList, float elapsed)
            : base(actionList)
        {
            this.elapsed = elapsed;
            isBlocking = true;
        }

        internal override void Update(GameTime gameTime)
        {
            elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsed > duration)
                isFinished = true;
        }
    }
}