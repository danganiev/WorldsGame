using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Utils.GeometricPrimitives;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Plane
    {
        public List<Vector3> Vertices { get; private set; }

        //        public Vector3 Normal { get; private set; }

        public string TextureName { get; set; }

        public PlaneNormalOrientationEnum Orientation { get; set; }

        public Plane(List<Vector3> vertices,/* Vector3 normal,*/ string textureName)
        {
            Vertices = vertices;
            //            Normal = normal;
            TextureName = textureName;
        }

        /// <summary>
        /// Constructs a new plane, with the specified size.
        /// </summary>
        public Plane(float xWidth, float yWidth, Vector3 initialDistance, string textureName = "")
        {
            TextureName = textureName;
            Vertices = new List<Vector3>();

            RecalculateVertices(xWidth, yWidth, initialDistance);
        }

        private void RecalculateVertices(float xWidth, float yWidth, Vector3 initialDistance)
        {
            Vector3 side1 = Vector3.Normalize(new Vector3(initialDistance.Y, initialDistance.Z, initialDistance.X));
            Vector3 side2 = Vector3.Normalize(Vector3.Cross(initialDistance, side1));

            Vertices.Add(initialDistance - side1 * yWidth / 2 - side2 * xWidth / 2);
            Vertices.Add(initialDistance - side1 * yWidth / 2 + side2 * xWidth / 2);
            Vertices.Add(initialDistance + side1 * yWidth / 2 + side2 * xWidth / 2);
            Vertices.Add(initialDistance + side1 * yWidth / 2 - side2 * xWidth / 2);
        }

        public PlanePrimitive GetPrimitive(GraphicsDevice graphicsDevice, string worldName)
        {
            var result = new PlanePrimitive(graphicsDevice, Vertices, textureName: TextureName, orientation: Orientation);
            result.LoadTexture(worldName, TextureName);
            return result;
        }

        public bool ContainsTexture(string name)
        {
            return TextureName == name;
        }

        public static Plane Lerp(Plane plane1, Plane plane2, float amount)
        {
            var vertices = new List<Vector3>();

            for (int index = 0; index < plane1.Vertices.Count; index++)
            {
                Vector3 newVertex = Vector3.Lerp(plane1.Vertices[index], plane2.Vertices[index], amount);
                vertices.Add(newVertex);
            }

            return new Plane(vertices, plane1.TextureName);
        }
    }
}