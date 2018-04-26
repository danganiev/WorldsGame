using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Models;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Light
{
    internal class LightRemovalNode
    {
        internal Vector3i Index { get; private set; } //this is the x y z in offset format

        internal Chunk Chunk { get; private set; }

        internal int Value { get; private set; }

        internal LightRemovalNode(Vector3i index, Chunk chunk, int value)
        {
            Index = index;
            Chunk = chunk;
            Value = value;
        }
    }
}