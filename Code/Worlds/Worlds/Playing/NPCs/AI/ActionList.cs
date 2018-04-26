using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;

//http://gamedevelopment.tutsplus.com/tutorials/the-action-list-data-structure-good-for-ui-ai-animations-and-more--gamedev-9264

namespace WorldsGame.Playing.NPCs.AI
{
    [Flags]
    internal enum Lanes
    {
        None = 0,
        Lane1 = 1,
        Lane2 = 2,
        Lane3 = 4
    }

    internal class ActionList
    {
        private const Lanes DEFAULT_LANES = Lanes.Lane1 & Lanes.Lane2 & Lanes.Lane3;

        //        private float _duration;
        //        private float _timeElapsed;

        //        private float percentDone;
        //        private bool blocking;

        private readonly List<AIAction> _actions;

        internal Entity Entity { get; private set; }

        internal ActionList(Entity entity)
        {
            _actions = new List<AIAction>();
            Entity = entity;
        }

        internal void Update(GameTime gameTime)
        {
            Lanes lanes = DEFAULT_LANES;

            for (int i = 0; i < _actions.Count; i++)
            {
                AIAction action = _actions[i];

                if ((lanes & action.Lanes) != Lanes.None)
                {
                    continue;
                }

                if (!action.IsStarted)
                {
                    action.OnStart(Entity);
                }

                action.Update(gameTime);
                if (action.IsBlocking)
                {
                    lanes |= action.Lanes;
                }

                if (action.IsFinished)
                {
                    action.OnFinish();
                    Remove(action);
                    // Experimental return because _actions count is broken on removal and addition
                    return;
                }
            }
        }

        internal void InsertAtStart(AIAction action)
        {
            _actions.Insert(0, action);
        }

        internal void InsertAtEnd(AIAction action)
        {
            _actions.Add(action);
        }

        internal void InsertBefore(AIAction beforeAction, AIAction newAction)
        {
            _actions.Insert(_actions.IndexOf(beforeAction), newAction);
        }

        internal void InsertAfter(AIAction afterAction, AIAction newAction)
        {
            _actions.Insert(_actions.IndexOf(afterAction) + 1, newAction);
        }

        internal void Remove(AIAction action)
        {
            _actions.Remove(action);
        }

        internal AIAction GetFirstAction()
        {
            if (_actions.Count > 0)
            {
                return _actions[0];
            }

            return null;
        }

        internal AIAction GetLastAction()
        {
            if (_actions.Count > 0)
            {
                return _actions.Last();
            }

            return null;
        }

        internal bool IsEmpty()
        {
            return _actions.Count == 0;
        }
    }
}