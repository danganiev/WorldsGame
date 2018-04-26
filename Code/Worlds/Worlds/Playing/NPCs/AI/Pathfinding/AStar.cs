using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Terrain;
using WorldsLib;

namespace WorldsGame.Playing.NPCs.AI
{
    // http://stevephillips.me/blog/implementing-pathfinding-algorithm-xna
    // another option if this doesn't work

    internal interface IPositionAccessibilityEstimator
    {
        bool IsPositionAccessible(int x, int y, int z, World world);
    }

    internal class PositionAccessibilityEstimator : IPositionAccessibilityEstimator
    {
        public bool IsPositionAccessible(int x, int y, int z, World world)
        {
            return world.GetBlock(x, y - 1, z).IsSolid && !world.GetBlock(x, y, z).IsSolid && !world.GetBlock(x, y + 1, z).IsSolid;
        }
    }

    internal interface IDistanceHeuristic
    {
        int Compute(Node src, Node tgt);
    }

    internal class ManhattanDistance : IDistanceHeuristic
    {
        public int Compute(Node src, Node tgt)
        {
            return (Math.Abs(src.X - tgt.X) + Math.Abs(src.Y - tgt.Y) + Math.Abs(src.Z - tgt.Z)) * AStar.LATERAL_WEIGHT;
        }
    }

    internal class AStar
    {
        internal const int LATERAL_WEIGHT = 10;
        internal const int DIAGONAL_WEIGHT = 14;

        private readonly HashSet<Node> _closedSet;
        private readonly List<Node> _openSet;

        private readonly IDistanceHeuristic _distanceHeuristic;
        private readonly World _world;

        internal static readonly ManhattanDistance MANHATTAN_DISTANCE = new ManhattanDistance();
        internal static readonly PositionAccessibilityEstimator BASIC_POSITION_ESTIMATOR = new PositionAccessibilityEstimator();

        internal AStar(IDistanceHeuristic distanceHeuristic, World world)
        {
            _distanceHeuristic = distanceHeuristic;
            _world = world;

            _closedSet = new HashSet<Node>();
            _openSet = new List<Node>();
        }

        private Node GetNode(int x, int y, int z)
        {
            bool isObstacle = _world.GetBlock(x, y, z).IsSolid;

            return new Node(x, y, z, isObstacle);
        }

        internal bool GetRoute(Vector3i src, Vector3i target, List<Vector3> route, PositionAccessibilityEstimator positionEstimator/*, int depth = 0*/)
        {
            if (!positionEstimator.IsPositionAccessible(target.X, target.Y, target.Z, _world))
            {
                return false;
            }

            return GetRoute(GetNode(target.X, target.Y, target.Z), GetNode(target.X, target.Y, target.Z), route, positionEstimator);
        }

        private bool GetRoute(Node src, Node target, List<Vector3> route, PositionAccessibilityEstimator positionEstimator, int depth = 0)
        {
            HashSet<Node> closedSet = _closedSet;
            closedSet.Clear();

            List<Node> openSet = _openSet;
            openSet.Clear();

            src.CumulativePreviousCost = 0;
            src.HeuristicEstimateCost = _distanceHeuristic.Compute(src, target);
            src.FullCost = src.CumulativePreviousCost + src.HeuristicEstimateCost;

            openSet.Add(src);

            while (openSet.Count > 0)
            {
                Node node = openSet[0];

                if (node == target)
                {
                    BackTrace(src, target, route);
                    return true;
                }

                int minX = Math.Max(src.X - depth, node.X - 1);
                int minY = Math.Max(src.Y - depth, node.Y - 1);
                int minZ = Math.Max(src.Z - depth, node.Z - 1);

                int maxX = Math.Min(src.X + depth, node.X + 1);
                //                int maxY = Math.Min(src.Y + depth, node.Y + 1);
                int maxY = Math.Min(src.Y + depth, node.Y + 2);
                int maxZ = Math.Min(src.Z + depth, node.Z + 1);

                for (int x = minX; x < maxX; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        for (int z = minZ; z < maxZ; z++)
                        {
                            if (x != node.X || y != node.Y || z != node.Z)
                            {
                                Node neighbour = GetNode(x, y, z);

                                if (neighbour.IsObstacle || !positionEstimator.IsPositionAccessible(x, y, z, _world))
                                {
                                    continue;
                                }

                                int weight = (node.X == x || node.Y == y || node.Z == z)
                                                   ? LATERAL_WEIGHT
                                                   : DIAGONAL_WEIGHT;

                                int g = node.CumulativePreviousCost + weight;
                                int h = _distanceHeuristic.Compute(neighbour, target);
                                int f = g + h;

                                if (closedSet.Contains(neighbour))
                                {
                                    if (neighbour.FullCost > f)
                                    {
                                        neighbour.CumulativePreviousCost = g;
                                        neighbour.HeuristicEstimateCost = h;
                                        neighbour.FullCost = f;
                                        neighbour.Parent = node;

                                        closedSet.Remove(neighbour);
                                        openSet.Add(neighbour);
                                    }
                                }
                                else if (openSet.Contains(neighbour))
                                {
                                    if (neighbour.FullCost > f)
                                    {
                                        neighbour.CumulativePreviousCost = g;
                                        neighbour.HeuristicEstimateCost = h;
                                        neighbour.FullCost = f;
                                        neighbour.Parent = node;
                                    }
                                }
                                else
                                {
                                    neighbour.CumulativePreviousCost = g;
                                    neighbour.HeuristicEstimateCost = h;
                                    neighbour.FullCost = f;
                                    neighbour.Parent = node;

                                    openSet.Add(neighbour);
                                }
                            }
                        }
                    }
                }

                closedSet.Add(node);
            };

            return false;
        }

        private void BackTrace(Node n1, Node n2, List<Vector3> route)
        {
            route.Add(n2.ToVector3());

            while (n2 != n1)
            {
                n2 = n2.Parent;
                route.Add(n2.ToVector3());
            }
        }
    }
}