using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.NPCs;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils;
using WorldsGame.Utils.ExtensionMethods;

namespace WorldsGame.Playing.Entities.Components
{
    internal class AnimationComponent : IEntityComponent
    {
        private readonly Dictionary<AnimationType, ComputedAnimation> _animations;

        private List<AnimationType> _animationsToRemove;
        private List<int> _affectedCuboids;

        private bool _isItem;
        private string _itemName;

        internal List<AnimationType> CurrentlyPlayedAnimations { get; private set; }

        internal Dictionary<AnimationType, float> CurrentFrameTime { get; private set; }

        //        internal Dictionary<AnimationType, List<ComputedKeyframe>> CurrentKeyframes { get; private set; }
        internal List<ComputedKeyframe> CurrentKeyframes { get; private set; }

        internal AnimationComponent(Dictionary<AnimationType, ComputedAnimation> animations, string itemName = "")
        {
            if (itemName != "")
            {
                _isItem = true;
                _itemName = itemName;
            }

            _animations = animations;
            _animationsToRemove = new List<AnimationType>();
            _affectedCuboids = new List<int>();

            CurrentKeyframes = new List<ComputedKeyframe>();
            CurrentFrameTime = new Dictionary<AnimationType, float>();
            CurrentlyPlayedAnimations = new List<AnimationType>();

            foreach (AnimationType animationType in EnumUtils.GetValues<AnimationType>())
            {
                CurrentFrameTime.Add(animationType, 0);

                for (int i = 0; i < _animations[animationType].CuboidCount; i++)
                {
                    if (_animations[animationType].KeyframesPerCuboids[i].Count > 0)
                    {
                        CurrentKeyframes.Add(_animations[animationType].KeyframesPerCuboids[i][0]);
                    }
                }
            }
        }

        // TODO: make combined animations work (like walking and hitting for example)
        internal void PlayAnimation(AnimationType animationType)
        {
            if (CurrentlyPlayedAnimations.Contains(animationType) || CurrentKeyframes.Count == 0)
            {
                return;
            }

            CurrentFrameTime[animationType] = 0;

            CurrentlyPlayedAnimations.Add(animationType);

            if (animationType == AnimationType.Swing || animationType == AnimationType.Consume)
            {
                _affectedCuboids.AddRange(_animations[animationType].AffectedCuboids);
            }

            if (!_isItem)
            {
                for (int i = 0; i < _animations[animationType].CuboidCount; i++)
                {
                    if (animationType == AnimationType.Walk && _affectedCuboids.Contains(i))
                    {
                        continue;
                    }

                    // i is for the cuboid, 0 is for its starting keyframe
                    CurrentKeyframes[i].Translation = _animations[animationType].KeyframesPerCuboids[i][0].Translation;
                    CurrentKeyframes[i].Rotation = _animations[animationType].KeyframesPerCuboids[i][0].Rotation;
                    CurrentKeyframes[i].Scale = _animations[animationType].KeyframesPerCuboids[i][0].Scale;
                }
            }
            else
            {
                PlayItemAnimation(animationType);
            }
        }

        private void PlayItemAnimation(AnimationType animationType)
        {
            int i = _animations[animationType].CuboidCount - 1;
            {
                if (animationType == AnimationType.Walk && _affectedCuboids.Contains(i))
                {
                    return;
                }

                // i is for the cuboid, 0 is for its starting keyframe
                // for item there could be only one cuboid, so 0 in CurrentKeyframes[]
                CurrentKeyframes[0].Translation = _animations[animationType].KeyframesPerCuboids[i][0].Translation;
                CurrentKeyframes[0].Rotation = _animations[animationType].KeyframesPerCuboids[i][0].Rotation;
                CurrentKeyframes[0].Scale = _animations[animationType].KeyframesPerCuboids[i][0].Scale;
            }
        }

        public void StopAnimation(AnimationType animationType)
        {
            CurrentFrameTime[animationType] = 0;
            _animationsToRemove.Add(animationType);

            if (CurrentKeyframes.Count > 0)
            {
                if (!_isItem)
                {
                    for (int i = 0; i < _animations[animationType].CuboidCount; i++)
                    {
                        CurrentKeyframes[i] = _animations[animationType].KeyframesPerCuboids[i][0];
                    }
                }
                else
                {
                    CurrentKeyframes[0] =
                        _animations[animationType].KeyframesPerCuboids[_animations[animationType].CuboidCount - 1][0];
                }
            }
        }

        internal void UpdateAnimations(GameTime gameTime)
        {
            for (int i = 0; i < _animationsToRemove.Count; i++)
            {
                AnimationType animationType = _animationsToRemove[i];
                CurrentlyPlayedAnimations.Remove(animationType);
            }
            _animationsToRemove.Clear();

            for (int i = 0; i < CurrentlyPlayedAnimations.Count; i++)
            {
                AnimationType currentlyPlayedAnimationType = CurrentlyPlayedAnimations[i];
                ComputedAnimation currentlyPlayedAnimation = _animations[currentlyPlayedAnimationType];

                CurrentFrameTime[currentlyPlayedAnimationType] += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                float currentTick = CurrentFrameTime[currentlyPlayedAnimationType];

                int frame = (int)currentTick / Constants.MILLISECONDS_PER_KEYFRAME;

                if (frame >= _animations[currentlyPlayedAnimationType].KeyframeCount)
                {
                    StopAnimation(currentlyPlayedAnimationType);
                    continue;
                }

                if (!_isItem)
                {
                    for (int j = 0; j < _animations[currentlyPlayedAnimationType].CuboidCount; j++)
                    {
                        // i is for the cuboid, 0 is for its starting keyframe
                        CurrentKeyframes[j].Translation =
                            currentlyPlayedAnimation.KeyframesPerCuboids[j][frame].Translation;
                        CurrentKeyframes[j].Rotation = currentlyPlayedAnimation.KeyframesPerCuboids[j][frame].Rotation;
                        CurrentKeyframes[j].Scale = currentlyPlayedAnimation.KeyframesPerCuboids[j][frame].Scale;
                    }
                }
                else
                {
                    int j = _animations[currentlyPlayedAnimationType].CuboidCount - 1;

                    CurrentKeyframes[0].Translation =
                            currentlyPlayedAnimation.KeyframesPerCuboids[j][frame].Translation;
                    CurrentKeyframes[0].Rotation = currentlyPlayedAnimation.KeyframesPerCuboids[j][frame].Rotation;
                    CurrentKeyframes[0].Scale = currentlyPlayedAnimation.KeyframesPerCuboids[j][frame].Scale;
                }
            }
        }

        public void Dispose()
        {
            CurrentKeyframes.Clear();
            CurrentFrameTime.Clear();
        }
    }
}