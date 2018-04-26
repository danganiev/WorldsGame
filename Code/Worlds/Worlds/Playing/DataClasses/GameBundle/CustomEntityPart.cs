using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldsGame.Saving;
using WorldsGame.Utils;
using WorldsGame.View.Blocks;
using Plane = WorldsGame.Saving.Plane;

namespace WorldsGame.Playing.DataClasses
{
    /// <summary>
    /// The main class to draw everything custom: blocks, items, characters, whatever
    /// </summary>
    [Serializable]
    public class CustomEntityPart
    {
        // Always clockwise
        public static readonly short[] INDICES_LIST = new short[] { 0, 1, 2, 0, 2, 3 };

        public Vector3[] VertexList { get; private set; }

        public Vector2[] UVMappings { get; private set; }

        public byte AtlasIndex { get; private set; }

        public CustomEntityPart(Vector3[] vertexList, Vector2[] uvMappings)
        {
            VertexList = vertexList;
            UVMappings = uvMappings;
        }

        public static Dictionary<int, List<CustomEntityPart>> GetPartsFromCuboids(
            List<Cuboid> cuboids, /*Vector3 minMaxDiff,*/ CompiledGameBundle gameBundle, int heightInBlocks)
        {
            var result = new Dictionary<int, List<CustomEntityPart>>();
            // I used to center cuboids around max min vertices of overall cuboids, but now I don't
            //            float xDiff = minMaxDiff.X / 32;
            //            float zDiff = minMaxDiff.Z / 32;
            float yDiff = (float)heightInBlocks / 2;

            for (int i = 0; i < cuboids.Count; i++)
            {
                Cuboid cuboid = cuboids[i];
                result.Add(i, new List<CustomEntityPart>());
                foreach (Plane plane in cuboid.Planes)
                {
                    if (plane.TextureName == "")
                    {
                        continue;
                    }

                    CompiledTexture texture = gameBundle.GetTexture(plane.TextureName);
                    int textureAtlasSizeInPixels = gameBundle.GetTextureAtlas(texture.AtlasIndex).SizeInPixels;

                    Vector3[] vertices = cuboid.GetTransformedVerticeList(plane).ToArray();

                    for (int j = 0; j < vertices.Length; j++)
                    {
                        var vertice = vertices[j];
                        //                        vertice.X = vertice.X - xDiff;
                        //                        vertice.Z = vertice.Z - zDiff;
                        vertice.Y = vertice.Y + yDiff;
                        vertices[j] = vertice;
                    }

                    Vector2[] uvMappings = BlockPart.GetCustomUVMappingList(texture, textureAtlasSizeInPixels);
                    var customPart = new CustomEntityPart(vertices, uvMappings);

                    result[i].Add(customPart);
                }
            }

            return result;
        }
    }
}