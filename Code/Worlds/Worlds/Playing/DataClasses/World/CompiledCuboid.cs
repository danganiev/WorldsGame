using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Saving;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledCuboid
    {
        public CompiledCuboid(Cuboid cuboid, bool isResized = true, bool isYTranslated = true)
        {
            Vector3 translation = (cuboid.MaxPoint + cuboid.MinPoint) / 2;

            if (!isYTranslated)
            {
                //                translation.Y = 0;
            }

            if (isResized)
            {
                DefaultBoundingBox = new BoundingBox(
                    (cuboid.MinPoint - translation) / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER,
                    (cuboid.MaxPoint - translation) / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER);
            }
            else
            {
                DefaultBoundingBox = new BoundingBox(
                    (cuboid.MinPoint - translation), (cuboid.MaxPoint - translation));

                //                Translation = translation;
            }

            Translation = translation / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER;

            Yaw = cuboid.Yaw;
            Pitch = cuboid.Pitch;
            Roll = cuboid.Roll;
        }

        // Box with zero point in center
        public BoundingBox DefaultBoundingBox { get; set; }

        public float Yaw { get; set; }

        public float Pitch { get; set; }

        public float Roll { get; set; }

        public Vector3 Translation { get; set; }

        public CustomModelHolder Adjust(/*CompiledCuboid adjuster, */ICustomModelHolder adjustee)
        {
            var result = new CustomModelHolder
            {
                CuboidCount = adjustee.CuboidCount
            };

            Vector3 maxVertice = adjustee.Cuboids[0][0].VertexList[0];
            Vector3 minVertice = adjustee.Cuboids[0][0].VertexList[0];

            foreach (KeyValuePair<int, List<CustomEntityPart>> keyValuePair in adjustee.Cuboids)
            {
                result.Cuboids.Add(keyValuePair.Key, new List<CustomEntityPart>());
                for (int i = 0; i < keyValuePair.Value.Count; i++)
                {
                    CustomEntityPart customEntityPart = keyValuePair.Value[i];
                    result.Cuboids[keyValuePair.Key].Add(
                        new CustomEntityPart((Vector3[])customEntityPart.VertexList.Clone(), (Vector2[])customEntityPart.UVMappings.Clone()));

                    for (int j = 0; j < customEntityPart.VertexList.Length; j++)
                    {
                        Vector3 vector3 = customEntityPart.VertexList[j];

                        maxVertice = Vector3.Max(maxVertice, vector3);
                        minVertice = Vector3.Min(minVertice, vector3);
                    }
                }
            }

            var translation = (maxVertice + minVertice) / 2;

            Vector3 adjusteeDiameter = maxVertice - minVertice;

            Vector3 diameter = DefaultBoundingBox.Max - DefaultBoundingBox.Min;

            Vector3 scale;
            Matrix scaleMatrix;

            scale = diameter / adjusteeDiameter;
            scaleMatrix = Matrix.CreateScale(scale);

            //            if (Vector3.Min(adjusteeDiameter, diameter) == diameter)
            //            {
            //                scale = diameter / adjusteeDiameter;
            //                scaleMatrix = Matrix.CreateScale(scale);
            //
            //                //                foreach (var cuboidPrimitive in cuboids)
            //                //                {
            //                //                    cuboidPrimitive.Scale(scaleMatrix);
            //                //                    ChildCuboids.Add(cuboidPrimitive);
            //                //                }
            //            }
            //            else
            //            {
            //                scale = diameter / adjusteeDiameter;
            //                scaleMatrix = Matrix.CreateScale(scale);
            //
            //                //                Scale(scaleMatrix);
            //                //
            //                //                foreach (var cuboidPrimitive in cuboids)
            //                //                {
            //                //                    ChildCuboids.Add(cuboidPrimitive);
            //                //                }
            //            }

            // I don't apply adjustee translation again, cause we only want the translation of holder cuboid to apply
            Matrix transformMatrix = Matrix.CreateTranslation(-translation) * Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) * scaleMatrix * Matrix.CreateTranslation(/*translation +*/ Translation);

            maxVertice = new Vector3(float.MinValue);
            minVertice = new Vector3(float.MaxValue);

            foreach (KeyValuePair<int, List<CustomEntityPart>> keyValuePair in adjustee.Cuboids)
            {
                for (int i = 0; i < keyValuePair.Value.Count; i++)
                {
                    CustomEntityPart customEntityPart = keyValuePair.Value[i];

                    for (int j = 0; j < customEntityPart.VertexList.Length; j++)
                    {
                        Vector3 vector3 = customEntityPart.VertexList[j];

                        vector3 = Vector3.Transform(vector3, transformMatrix);
                        maxVertice = Vector3.Max(maxVertice, vector3);
                        minVertice = Vector3.Min(minVertice, vector3);

                        result.Cuboids[keyValuePair.Key][i].VertexList[j] = vector3;
                    }
                }
            }

            result.MaxVertice = maxVertice;
            result.MinVertice = minVertice;

            return result;
        }
    }
}