using Microsoft.Xna.Framework;

using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.Types;
using WorldsLib;

namespace WorldsGame.Playing.Players
{
    internal class SelectionBlock
    {
        internal PositionedBlock CurrentSelection { get; set; }

        internal PositionedBlock CurrentSelectedAdjacent { get; set; }

        private readonly Player _player;

        public SelectionBlock(Player player)
        {
            _player = player;
        }

        internal void Update()
        {
            SetPlayerSelectedBlock();
        }

        //sets player currentSelection (does nothing if no selection available, like looking ath the sky)
        // returns x float where selection was found for further selection processing (eg finding adjacent block where to add a new block)
        private void SetPlayerSelectedBlock()
        {
            var broken = false;
            for (float x = 0; x < 8f; x += 0.1f)
            {
                Vector3 targetPoint = _player.FacePosition + (_player.LookVector * x);

                BlockType block = _player.World.GetBlock(targetPoint);

                if (block != null && !block.IsAirType() && !block.IsLiquid)
                {
                    CurrentSelection = new PositionedBlock(new Vector3i(targetPoint), block);

                    float xStart = x;
                    SetPlayerAdjacentSelectedBlock(xStart);

                    broken = true;
                    break;
                }
            }

            if (!broken && _player.UseSelectionBlockWithAir)
            {
                Vector3 targetPoint = _player.Position + (_player.LookVector * 8f);
                BlockType block = _player.World.GetBlock(targetPoint);
                CurrentSelection = new PositionedBlock(new Vector3i(targetPoint), block);
                CurrentSelectedAdjacent = CurrentSelection;
            }
            else if (!broken)
            {
                CurrentSelection = null;
                CurrentSelectedAdjacent = null;
            }
        }

        //Sets the adjacent block of current selected block. Adjacent is the only one block, from the side of player view.
        private void SetPlayerAdjacentSelectedBlock(float xStart)
        {
            for (float x = xStart; x > 0.5f; x -= 0.1f)
            {
                Vector3 targetPoint = _player.Position + (_player.LookVector * x);

                BlockType blockDescriber = _player.World.GetBlock(targetPoint);

                if (blockDescriber == null || blockDescriber.IsAirType())
                {
                    CurrentSelectedAdjacent = new PositionedBlock(new Vector3i(targetPoint), null);
                    break;
                }
            }
        }
    }
}