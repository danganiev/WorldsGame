using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Terrain;
using WorldsGame.Utils;
using WorldsLib;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class DeterminePath : AIAction
    {
        private readonly Vector3i _targetPosition;

        //        private readonly AIAction nextAction;
        private AStar _aStar;

        internal DeterminePath(ActionList actionList, Vector3 position, AIAction nextAction)
            : base(actionList)
        {
            _targetPosition = WorldBlockOperator.GetBlockPosition(position);
            this.nextAction = nextAction;
            Lanes = Lanes.Lane1;
            isBlocking = true;
        }

        internal override void Update(GameTime gameTime)
        {
            CheckForAPath();
        }

        private void CheckForAPath()
        {
            Vector3i oldPosition = WorldBlockOperator.GetBlockPosition(entity.GetComponent<PositionComponent>().Position);

            var path = new List<Vector3>();

            if (_aStar.GetRoute(oldPosition, _targetPosition, path, AStar.BASIC_POSITION_ESTIMATOR))
            {
                InsertInFrontOfMe(new MoveByWaypoints(ownerList, path, nextAction));
                Finish();
                return;
            }

            InsertInFrontOfMe(nextAction);
            Finish();
        }

        internal override void OnStart(Entity entity)
        {
            base.OnStart(entity);

            World world = entity.GetConstantComponent<WorldComponent>().World;

            _aStar = new AStar(AStar.MANHATTAN_DISTANCE, world);
        }
    }
}