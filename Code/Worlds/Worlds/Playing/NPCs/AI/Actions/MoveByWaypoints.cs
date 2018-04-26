using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class MoveByWaypoints : AIAction
    {
        private readonly List<Vector3> _waypoints;

        internal MoveByWaypoints(ActionList actionList, List<Vector3> waypoints, AIAction nextAction)
            : base(actionList)
        {
            _waypoints = waypoints;
            this.nextAction = nextAction;

            Lanes = Lanes.Lane1;
        }

        internal override void Update(GameTime gameTime)
        {
            Move(gameTime);
        }

        private void Move(GameTime gameTime)
        {
            if (!CheckIfArrived())
            {
                Vector3 nextCheckpoint = _waypoints[0];
                _waypoints.Remove(nextCheckpoint);

                InsertInFrontOfMe(new MoveTo(ownerList, nextCheckpoint, this));
                RemoveSelf();
            }
        }

        private bool CheckIfArrived()
        {
            var positionComponent = entity.GetComponent<PositionComponent>();

            if (_waypoints.Count == 0)
            {
                InsertInFrontOfMe(nextAction);
                Finish();
                return true;
            }

            var destination = _waypoints[_waypoints.Count - 1];

            if (Vector3.Distance(positionComponent.Position, destination) < MoveTo.DESTINATION_DELTA_SQUARED)
            {
                InsertInFrontOfMe(nextAction);
                Finish();
                return true;
            }

            return false;
        }

        //        internal override void OnStart(Entity entity)
        //        {
        //            base.OnStart(entity);
        //
        //
        //        }
    }
}