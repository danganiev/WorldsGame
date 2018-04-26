﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldsGame.Models;
using WorldsLib;

namespace WorldsGame.Playing.Terrain.Light
{
    internal class LightAdditionNode {
        internal const int MAX_SUNLIGHT = 15;

        internal Vector3i Index { get; private set; } //this is the x y z in offset format
        internal Chunk Chunk { get; private set; }

        internal LightAdditionNode(Vector3i index, Chunk chunk)
        {
            Index = index;
            Chunk = chunk;
        }
    }
}