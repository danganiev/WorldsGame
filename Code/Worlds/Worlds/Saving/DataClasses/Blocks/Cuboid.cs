using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors.Blocks;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.GeometricPrimitives;

namespace WorldsGame.Saving
{
    [Serializable]
    public class Cuboid
    {
        //        public static readonly Vector3 COORDINATE_TRANSLATION = new Vector3(16, 16, 16);
        public const int MODEL_EDITOR_DIFF_MULTIPLIER = 32;

        public List<Plane> Planes { get; set; }

        public Vector3 Translation { get; set; }

        public float Yaw { get; set; }

        public float Pitch { get; set; }

        public float Roll { get; set; }

        public Vector3 MinPoint { get; set; }

        public Vector3 MaxPoint { get; set; }

        public float XWidth { get { return MaxPoint.X - MinPoint.X; } }

        public float YHeight { get { return MaxPoint.Y - MinPoint.Y; } }

        public float ZWidth { get { return MaxPoint.Z - MinPoint.Z; } }

        public bool IsItem { get; set; }

        public Cuboid(List<Plane> planes, Vector3 translation, float yaw, float pitch, float roll)
        {
            Planes = planes;
            Translation = translation;
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
        }

        public Cuboid(Vector3 size, Vector3 position, bool isItem = false)
        {
            Translation = position;
            IsItem = isItem;

            Planes = new List<Plane>();

            Initialize(size);
        }

        private void Initialize(Vector3 size)
        {
            Planes.Clear();

            InitializePlanes(Vector3.Right * size.X / 2, size.Y, size.Z);
            InitializePlanes(Vector3.Up * size.Y / 2, size.Z, size.X);
            InitializePlanes(Vector3.Backward * size.Z / 2, size.X, size.Y);
        }

        private void InitializePlanes(Vector3 normal, float size1, float size2)
        {
            var plane = new Plane(size1, size2, normal);

            if (IsItem)
            {
                if (Vector3.Normalize(normal) == Vector3.Backward)
                {
                    plane.Orientation = PlaneNormalOrientationEnum.Forward;
                }
                else if (Vector3.Normalize(normal) == Vector3.Up)
                {
                    plane.Orientation = PlaneNormalOrientationEnum.Up;
                }
                else if (Vector3.Normalize(normal) == Vector3.Right)
                {
                    plane.Orientation = PlaneNormalOrientationEnum.Right;
                }
            }

            Vector3 negatedNormal = Vector3.Negate(normal);

            plane = new Plane(size1, size2, negatedNormal);

            Planes.Add(plane);
        }

        public CuboidPrimitive GetPrimitive(GraphicsDevice graphicsDevice, string worldName, EditedModel editedModel)
        {
            var planePrimitives = Planes.Select(plane => plane.GetPrimitive(graphicsDevice, worldName)).ToList();

            return new CuboidPrimitive(
                graphicsDevice, Yaw, Pitch, Roll, Translation, planePrimitives, editedModel, isItem: IsItem);
        }

        public bool ContainsTexture(string name)
        {
            foreach (Plane plane in Planes)
            {
                bool result = plane.ContainsTexture(name);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<string> GetTextureNames()
        {
            return from plane in Planes
                   select plane.TextureName;
        }

        public List<Vector3> GetTransformedVerticeList(Plane plane)
        {
            var result = new List<Vector3>();

            foreach (Vector3 vertice in plane.Vertices)
            {
                Vector3 vector =
                    Vector3.Transform(vertice, Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) *
                                      Matrix.CreateTranslation(Translation));

                // This is because of coordinate adjustion in model editor
                vector = vector / MODEL_EDITOR_DIFF_MULTIPLIER;
                result.Add(vector);
            }

            return result;
        }

        public static Cuboid Lerp(Cuboid cuboid1, Cuboid cuboid2, float amount)
        {
            var planes = new List<Plane>();

            for (int index = 0; index < cuboid1.Planes.Count; index++)
            {
                planes.Add(Plane.Lerp(cuboid1.Planes[index], cuboid2.Planes[index], amount));
            }

            Vector3 translation = Vector3.Lerp(cuboid1.Translation, cuboid2.Translation, amount);

            float yaw = MathHelper.Lerp(cuboid1.Yaw, cuboid2.Yaw, amount);
            float pitch = MathHelper.Lerp(cuboid1.Pitch, cuboid2.Pitch, amount);
            float roll = MathHelper.Lerp(cuboid1.Roll, cuboid2.Roll, amount);

            return new Cuboid(planes, translation, yaw, pitch, roll);
        }

        public Matrix GetTranslation(Cuboid cuboid, bool useModelEditorMultiplier = true)
        {
            if (useModelEditorMultiplier)
            {
                return Matrix.CreateTranslation((Translation - cuboid.Translation) / MODEL_EDITOR_DIFF_MULTIPLIER);
            }

            return Matrix.CreateTranslation((Translation - cuboid.Translation));
        }

        public Matrix GetRotation(Cuboid cuboid)
        {
            return Matrix.CreateFromYawPitchRoll(Yaw - cuboid.Yaw, Pitch - cuboid.Pitch, Roll - cuboid.Roll);
        }

        public Matrix GetScale(Cuboid previousCuboid)
        {
            //            float xDiff = (XWidth - previousCuboid.XWidth) / XWidth;
            //            float yDiff = (YHeight - previousCuboid.YHeight) / YHeight;
            //            float zDiff = (ZWidth - previousCuboid.ZWidth) / ZWidth;

            float xDiff = XWidth / previousCuboid.XWidth;
            float yDiff = YHeight / previousCuboid.YHeight;
            float zDiff = ZWidth / previousCuboid.ZWidth;

            return Matrix.CreateScale(xDiff, yDiff, zDiff);
        }
    }
}