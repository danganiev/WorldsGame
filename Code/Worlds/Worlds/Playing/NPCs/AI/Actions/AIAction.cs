using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class AIAction
    {
        protected ActionList ownerList;

        protected bool isFinished;
        protected bool isStarted;
        protected bool isBlocking;
        protected float elapsed;
        protected float duration;
        protected Entity entity;
        protected AIAction nextAction;

        internal Lanes Lanes { get; set; }

        internal bool IsBlocking
        {
            get { return isBlocking; }
        }

        internal bool IsFinished
        {
            get { return isFinished; }
        }

        internal bool IsStarted
        {
            get { return isStarted; }
        }

        // No inheritance of this constructor allowed
        private AIAction()
        {
        }

        internal AIAction(ActionList ownerList)
        {
            this.ownerList = ownerList;
        }

        internal virtual void Update(GameTime gameTime)
        {
        }

        internal virtual void OnStart(Entity entity)
        {
            isStarted = true;
            this.entity = entity;
        }

        protected void Finish()
        {
            isFinished = true;
        }

        internal virtual void OnFinish()
        {
        }

        protected void InsertInFrontOfMe(AIAction action)
        {
            ownerList.InsertBefore(this, action);
        }

        protected void InsertAfterMe(AIAction action)
        {
            ownerList.InsertAfter(this, action);
        }

        // Use this instead of Finish() if you want to reuse action later
        protected void RemoveSelf()
        {
            ownerList.Remove(this);
        }
    }
}