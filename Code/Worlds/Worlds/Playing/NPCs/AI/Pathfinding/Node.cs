using System;
using Microsoft.Xna.Framework;

namespace WorldsGame.Playing.NPCs.AI
{
    internal class Node : IComparable<Node>
    {
        // Position
        internal int X { get; set; }

        internal int Y { get; set; }

        internal int Z { get; set; }

        // Cost
        internal int FullCost { get; set; } // Full cost. (F)

        internal int CumulativePreviousCost { get; set; } // Cumulative previous cost to the node. (G)

        internal int HeuristicEstimateCost { get; set; } // Heuristic estimate cost (H)

        internal bool IsObstacle { get; set; }

        internal Node Parent { get; set; }

        internal Node(int x, int y, int z, bool isObstacle)
        {
            X = x;
            Y = y;
            Z = z;

            IsObstacle = isObstacle;
        }

        public override String ToString()
        {
            return X + ", " + Y + ", " + Z;
        }

        public int CompareTo(Node other)
        {
            if (FullCost < other.FullCost)
                return -1;
            if (FullCost > other.FullCost)
                return 1;

            return 0;
        }

        internal Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public static bool operator ==(Node a, Node b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Node a, Node b)
        {
            return !(a == b);
        }
    }
}