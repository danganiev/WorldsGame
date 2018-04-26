using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class DetectPlayer : AIAction
    {
        internal AIAction ActionOnPlayerFound { get; private set; }

        internal DetectPlayer(ActionList actionList, AIAction actionOnPlayerFound = null)
            : base(actionList)
        {
            ActionOnPlayerFound = actionOnPlayerFound ?? new Flee(actionList);
        }

        internal override void Update(GameTime gameTime)
        {
            if (IsPlayerNearby())
            {
                InsertInFrontOfMe(ActionOnPlayerFound);
            }
        }

        private bool IsPlayerNearby()
        {
            throw new NotImplementedException();
        }
    }
}