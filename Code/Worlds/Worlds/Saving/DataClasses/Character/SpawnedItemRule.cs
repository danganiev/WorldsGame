using System;
using Microsoft.Xna.Framework;

namespace WorldsGame.Saving.DataClasses
{
    /// <summary>
    /// The rule for spawning an item in characters inventory on his spawn
    /// </summary>
    [Serializable]
    public class SpawnedItemRule : IItemLike
    {
        public string Name { get; set; }

        public Color[] IconColors { get; set; }

        public int MinQuantity { get; set; }

        public int MaxQuantity { get; set; }

        public int Probability { get; set; }

        public string Description
        {
            get
            {
                string quantity = MinQuantity == MaxQuantity ? MinQuantity.ToString() :
                    string.Format("{0}-{1}", MinQuantity, MaxQuantity);

                return string.Format("Quantity: {0}, Probability: {1}", quantity, Probability);
            }
        }
    }
}