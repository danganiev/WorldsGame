using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Weighted_Randomizer;
using WorldsGame.Playing.Entities;

namespace WorldsGame.Playing.NPCs.AI
{
    /// <summary>
    /// Main action loop, where AI decides what to do with its life
    /// </summary>
    internal class Think : AIAction
    {
        private bool _foundSomethingToDo;

        //        internal List<AIAction> PossibleActions { get; private set; }
        //
        //        // Must be 100 in sum
        //        internal List<int> PossibleActionsPriorities { get; private set; }

        private StaticWeightedRandomizer<AIAction> _actionRandomizer;

        internal Think(ActionList actionList)
            : base(actionList)
        {
            //            PossibleActions = new List<AIAction>();
            //            PossibleActionsPriorities = new List<int>();
            Lanes = Lanes.Lane1 | Lanes.Lane2 | Lanes.Lane3;
            _actionRandomizer = new StaticWeightedRandomizer<AIAction>();

            _actionRandomizer.Add(new Roam(ownerList));
        }

        internal override void Update(GameTime gameTime)
        {
            if (!_foundSomethingToDo)
            {
                ChooseWhatToDo();
            }
        }

        private void ChooseWhatToDo()
        {
            AIAction actionToDo = _actionRandomizer.NextWithReplacement();

            ownerList.InsertAtStart(actionToDo);

            _foundSomethingToDo = true;
        }
    }
}