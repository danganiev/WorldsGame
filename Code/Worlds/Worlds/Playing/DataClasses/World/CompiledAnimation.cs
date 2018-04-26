using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldsGame.Editors.Blocks;
using WorldsGame.Saving;

namespace WorldsGame.Playing.DataClasses
{
    public enum AnimationType
    {
        Walk,
        Swing,
        Consume
    }

    public static class AnimationTypeHelper
    {
        public static readonly Dictionary<AnimationType, string> ANIMATION_TYPE_NAMES = new Dictionary<AnimationType, string>
        {
            {AnimationType.Walk, "Walk"},
            {AnimationType.Swing, "Swing item"},
            {AnimationType.Consume, "Consume item"}
        };

        public static readonly Dictionary<AnimationType, string> ITEM_RELATED_ANIMATION_TYPES = new Dictionary<AnimationType, string>
        {
            {AnimationType.Swing, "Swing item"},
            {AnimationType.Consume, "Consume item"}
        };
    }

    [Serializable]
    public class CompiledAnimation
    {
        public const int FRAMES_PER_SECOND = 30;
        public const float DEFAULT_LENGTH = 1f;

        //        public string Name { get; set; }
        //
        //        public AnimationType AnimationType { get; set; }

        // Keyframes for every cuboid in the model
        // First dimension says how much keyframes, second how many cuboids

        // If we have 2 cuboids both transformed 3 times, we should have the list like this:
        // [1st cuboid:[Transform, Transform, Transform], 2nd cuboid: [Transform, Transform, Transform]] (if its not like this, I have to do it right)
        public List<List<CompiledKeyframe>> Keyframes { get; set; }

        public float Length { get { return Keyframes.Count == 0 ? 0 : Keyframes[0].Last().Time; } }

        public int KeyframeCount
        {
            get { return Keyframes[0].Count; }
        }

        public int CuboidCount
        {
            get { return Keyframes.Count; }
        }

        public CompiledAnimation(int cuboidCount)
        {
            Keyframes = new List<List<CompiledKeyframe>>();

            for (int i = 0; i < cuboidCount; i++)
            {
                Keyframes.Add(new List<CompiledKeyframe> { new CompiledKeyframe { IsFirst = true } });
            }
        }

        public CompiledKeyframe GetFirstKeyframe(int cuboidIndex)
        {
            return Keyframes[cuboidIndex].First();
        }

        public CompiledKeyframe GetLastKeyframe(int cuboidIndex)
        {
            return Keyframes[cuboidIndex].Last();
        }

        public void SortKeyframes(int cuboidIndex)
        {
            Keyframes[cuboidIndex] = Keyframes[cuboidIndex].OrderBy(keyframe1 => keyframe1.Time).ToList();
        }

        public void SortKeyframes()
        {
            for (int i = 0; i < Keyframes.Count; i++)
            {
                SortKeyframes(i);
            }
        }

        public void RemoveKeyframe(int index)
        {
            foreach (List<CompiledKeyframe> compiledKeyframes in Keyframes)
            {
                try
                {
                    compiledKeyframes.RemoveAt(index);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        public static List<CompiledKeyframe> CreateKeyframeList(float time, List<Cuboid> originalCuboids,
                                                            List<Cuboid> keyframeCuboids)
        {
            var result = new List<CompiledKeyframe>();

            Cuboid originalItemCuboid = null;
            Cuboid keyframeItemCuboid = null;

            for (int i = 0; i < originalCuboids.Count; i++)
            {
                Cuboid originalCuboid = originalCuboids[i];
                Cuboid keyframeCuboid = keyframeCuboids[i];

                if (originalCuboid.IsItem)
                {
                    originalItemCuboid = originalCuboid;
                    keyframeItemCuboid = keyframeCuboid;
                    continue;
                }

                var compiledKeyframe = new CompiledKeyframe
                {
                    Time = time,
                    Translation = keyframeCuboid.GetTranslation(originalCuboid, useModelEditorMultiplier: false),
                    Rotation = keyframeCuboid.GetRotation(originalCuboid),
                    Scale = keyframeCuboid.GetScale(originalCuboid)
                };

                result.Add(compiledKeyframe);
            }

            // Item is last so we know it's last
            if (originalItemCuboid != null && keyframeItemCuboid != null)
            {
                var compiledItemKeyframe = new CompiledKeyframe
                {
                    Time = time,
                    Translation = keyframeItemCuboid.GetTranslation(originalItemCuboid, useModelEditorMultiplier: false),
                    Rotation = keyframeItemCuboid.GetRotation(originalItemCuboid),
                    Scale = keyframeItemCuboid.GetScale(originalItemCuboid)
                };

                result.Add(compiledItemKeyframe);
            }

            return result;
        }

        public void RefreshAnimationPerCuboids(EditedModel model = null)
        {
            if (model != null)
            {
                if (model.RemovedCuboids.Count > 0)
                {
                    foreach (int removedCuboid in model.RemovedCuboids)
                    {
                        if (Keyframes.Count > removedCuboid)
                        {
                            Keyframes.RemoveAt(removedCuboid);
                        }
                    }
                }
                if (model.AddedCuboids.Count > 0)
                {
                    foreach (int addedCuboid in model.AddedCuboids)
                    {
                        Keyframes.Add(new List<CompiledKeyframe> { new CompiledKeyframe { IsFirst = true } });
                    }
                }

                // next two ifs should fix problems if something has gone wrong at some previous attempt
                if (model.CuboidCount > Keyframes.Count && model.AddedCuboids.Count == 0)
                {
                    int diff = model.CuboidCount - Keyframes.Count;
                    for (int i = 0; i < diff; i++)
                    {
                        Keyframes.Add(new List<CompiledKeyframe> { new CompiledKeyframe { IsFirst = true } });
                    }
                }
                if (model.CuboidCount < Keyframes.Count && model.RemovedCuboids.Count == 0)
                {
                    Keyframes.Clear();
                    for (int i = 0; i < model.CuboidCount; i++)
                    {
                        Keyframes.Add(new List<CompiledKeyframe> { new CompiledKeyframe { IsFirst = true } });
                    }
                }
            }
        }
    }

    [Serializable]
    public class CompiledKeyframe
    {
        public float Time { get; set; }

        public Matrix Translation { get; set; }

        public Matrix Rotation { get; set; }

        public Matrix Scale { get; set; }

        public bool IsFirst { get; set; }

        public string Name
        {
            get
            {
                if (IsFirst)
                {
                    return "First";
                }
                return Time.ToString();
            }
        }

        public CompiledKeyframe()
        {
            Time = 0;
            Translation = Matrix.Identity;
            Rotation = Matrix.Identity;
            Scale = Matrix.Identity;
        }

        public CompiledKeyframe Clone()
        {
            return new CompiledKeyframe
            {
                Time = Time,
                Translation = Translation,
                Rotation = Rotation,
                Scale = Scale,
                IsFirst = IsFirst
            };
        }
    }

    // Implied 30 fps
    // The difference from CompiledAnimation is that this class is created in runtime, and is not serialized and saved
    public class ComputedAnimation
    {
        // First dimension - keyframes, second - cuboids
        public List<List<ComputedKeyframe>> KeyframesPerCuboids { get; private set; }

        public int CuboidCount { get; private set; }

        public int KeyframeCount { get; private set; }

        public List<int> AffectedCuboids { get; private set; }

        private ComputedAnimation()
        {
            KeyframesPerCuboids = new List<List<ComputedKeyframe>>();
            AffectedCuboids = new List<int>();
        }

        public static ComputedAnimation CreateFromCompiledAnimation(CompiledAnimation animation, bool isScaled = false, bool isFPSItem = false)
        {
            var result = new ComputedAnimation
            {
                KeyframeCount = 0
            };

            result.CuboidCount = animation.CuboidCount;

            int maxKeyframesPerCuboid = 0;

            for (int i = 0; i < animation.CuboidCount; i++)
            {
                List<CompiledKeyframe> keyframesPerCuboid = animation.Keyframes[i];

                if (i == 0)
                {
                    maxKeyframesPerCuboid = keyframesPerCuboid.Count;
                }

                bool isCuboidAffected = false;

                CompiledKeyframe previousKeyframe = keyframesPerCuboid[0];

                // Balance in all things
                if (keyframesPerCuboid.Count < maxKeyframesPerCuboid)
                {
                    while (keyframesPerCuboid.Count < maxKeyframesPerCuboid)
                    {
                        CompiledKeyframe newKeyframe = keyframesPerCuboid[0].Clone();
                        newKeyframe.Time = animation.Keyframes[0][keyframesPerCuboid.Count].Time;

                        keyframesPerCuboid.Add(newKeyframe);
                    }
                }

                for (int k = 0; k < keyframesPerCuboid.Count; k++)
                {
                    if (k == 0)
                    {
                        result.KeyframesPerCuboids.Add(new List<ComputedKeyframe>());

                        continue;
                    }

                    int keyframeCount = (int)Math.Floor((keyframesPerCuboid[k].Time - previousKeyframe.Time) * 30);

                    if (i == 0)
                    {
                        result.KeyframeCount += keyframeCount;
                    }

                    CompiledKeyframe compiledKeyframe = keyframesPerCuboid[k];

                    Quaternion previousRotation = Quaternion.CreateFromRotationMatrix(previousKeyframe.Rotation);
                    Quaternion currentRotation = Quaternion.CreateFromRotationMatrix(compiledKeyframe.Rotation);

                    for (int j = 0; j < keyframeCount; j++)
                    {
                        float interpolationAmount = 1 - (float)(keyframeCount - j) / keyframeCount;

                        if (!isCuboidAffected && compiledKeyframe.Translation != Matrix.Identity || compiledKeyframe.Scale != Matrix.Identity || compiledKeyframe.Rotation != Matrix.Identity)
                        {
                            isCuboidAffected = true;
                        }

                        Vector3 previousTranslation = isScaled
                                                      ? previousKeyframe.Translation.Translation / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER
                                                      : previousKeyframe.Translation.Translation;

                        Vector3 currentTranslation = isScaled
                                                        ? compiledKeyframe.Translation.Translation / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER
                                                        : compiledKeyframe.Translation.Translation;

                        if (isFPSItem)
                        {
                            previousTranslation *= new Vector3(-1, 0, -1);
                            currentTranslation *= new Vector3(-1, 0, -1);
                        }

                        var computedKeyframe = new ComputedKeyframe(
                            Matrix.Lerp(Matrix.CreateTranslation(previousTranslation), Matrix.CreateTranslation(currentTranslation),
                                        interpolationAmount),
                            Matrix.CreateFromQuaternion(Quaternion.Slerp(previousRotation, currentRotation, interpolationAmount)),
                            Matrix.Lerp(previousKeyframe.Scale, compiledKeyframe.Scale, interpolationAmount));

                        result.KeyframesPerCuboids[i].Add(computedKeyframe);
                    }

                    previousKeyframe = compiledKeyframe;
                }

                result.AffectedCuboids.Add(i);
            }

            return result;
        }
    }

    public class ComputedKeyframe
    {
        public Matrix Translation { get; set; }

        public Matrix Rotation { get; set; }

        public Matrix Scale { get; set; }

        public ComputedKeyframe(Matrix transformation, Matrix rotation, Matrix scale)
        {
            Translation = transformation;
            Rotation = rotation;
            Scale = scale;
        }
    }
}