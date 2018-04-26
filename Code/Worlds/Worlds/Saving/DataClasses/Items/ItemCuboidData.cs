using System;
using Microsoft.Xna.Framework;

namespace WorldsGame.Saving.DataClasses
{
    [Serializable]
    public class ItemCuboidData
    {
        public Cuboid Cuboid { get; set; }

        //        public Matrix Rotation { get; set; }

        // The cuboid child is sticked to if such. (if not sticked this == -1)
        public int StickedCuboidID { get; set; }

        public ItemCuboidData()
        {
            //            Rotation = Matrix.Identity;
            StickedCuboidID = -1;
        }
    }
}