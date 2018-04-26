using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Utils;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class Roam : AIAction
    {
        internal Roam(ActionList actionList)
            : base(actionList)
        {
            Lanes = Lanes.Lane1;
        }

        internal override void Update(GameTime gameTime)
        {
            ChooseWhereToGo();
            // TODO: add player detection action (probably should be on another lane)
        }

        private void ChooseWhereToGo()
        {
            var positionComponent = entity.GetComponent<PositionComponent>();

            int x = RandomNumberGenerator.GetInt(5, 20);
            int z = RandomNumberGenerator.GetInt(5, 20);

            var newPosition = positionComponent.Position + new Vector3(x, 0, z) * RandomNumberGenerator.GetFloat();

            InsertInFrontOfMe(new DeterminePath(ownerList, newPosition, this));
            RemoveSelf();
        }
    }
}