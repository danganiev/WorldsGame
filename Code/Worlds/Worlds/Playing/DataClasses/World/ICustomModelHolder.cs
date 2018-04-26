using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.DataClasses
{
    public interface ICustomModelHolder
    {
        Vector3 MinVertice { get; }

        Vector3 MaxVertice { get; }

        int CuboidCount { get; }

        Dictionary<int, List<CustomEntityPart>> Cuboids { get; }
    }

    public class CustomModelHolder : ICustomModelHolder
    {
        public Vector3 MinVertice { get; set; }

        public Vector3 MaxVertice { get; set; }

        public int CuboidCount { get; set; }

        public Dictionary<int, List<CustomEntityPart>> Cuboids { get; set; }

        public CustomModelHolder()
        {
            Cuboids = new Dictionary<int, List<CustomEntityPart>>();
        }
    }
}