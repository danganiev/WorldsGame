using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledCharacter : ICustomModelHolder
    {
        [NonSerialized]
        private CompiledGameBundle _gameBundle;

        public CompiledGameBundle GameBundle
        {
            get { return _gameBundle; }
            set { _gameBundle = value; }
        }

        public string Name { get; set; }

        public List<SpawnedItemRule> InventorySpawnRules { get; private set; }

        public int CuboidCount { get; private set; }

        // Parts per cuboid
        // First dimension - cuboid, second - list of parts
        public Dictionary<int, List<CustomEntityPart>> Cuboids { get; private set; }

        public Vector3 MinVertice { get; set; }

        public Vector3 MaxVertice { get; set; }

        public float FaceHeight { get; set; }

        public bool IsPlayerCharacter { get; set; }

        public Dictionary<AnimationType, CompiledAnimation> BasicAnimations { get; private set; }

        // В момент загрузки вещи нам нужно точно определить место каждой вершины каждого кубоида модели вещи.
        // Для этого нужно знать четкое местоположение кубоида-хранителя вещи (вершины + повороты),
        // а также учитывать все матрицы анимации для данного кубоида-хранителя
        //
        // Вещь всегда грузится строго по размерам bounding box для ее вершин, даже если вещь повернута в редакторе моделей
        //
        // А вообще, на данный момент мне не хватает сохраненной ориентации для вещи внутри редактора персонажа, и отображенных осей при изменении
        //
        // Also, character face in character editor should always be facing the backward vector, and not set it by button.
        // Button should only set the cuboid from center of which the camera will be facing
        //
        // Also, there will be a rotationary problem, case we're facing forward (-1, 0, 0) instead of backward (1, 0, 0) in model editor
        // The easiest solution is probably to rotate model by 180 before initializing it
        public CompiledCuboid DefaultItemHolderCuboid { get; set; }

        public Dictionary<string, CompiledCuboid> ItemHolderCuboids { get; set; }

        //For serialization only!
        public CompiledCharacter()
        {
        }

        public CompiledCharacter(CompiledGameBundle gameBundle, Character character)
        {
            GameBundle = gameBundle;
            Name = character.Name;
            InventorySpawnRules = character.InventorySpawnRules;

            var naturalNumberSpaceDiff = new Vector3(
                character.LengthInBlocks * 16, character.HeightInBlocks * 16, character.WidthInBlocks * 16);

            MinVertice = character.MinVertice + naturalNumberSpaceDiff;
            MaxVertice = character.MaxVertice + naturalNumberSpaceDiff;
            IsPlayerCharacter = character.IsPlayerCharacter;

            if (IsPlayerCharacter)
            {
                FaceHeight = (character.FaceHeight + naturalNumberSpaceDiff.Y) / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER;
            }

            CuboidCount = character.Cuboids.Count;

            InitializeParts(character);
            InitializeAnimationCuboids(character, new Vector3(0, character.HeightInBlocks * 16, 0));
            InitializeAnimations(character);
        }

        private void InitializeParts(Character character)
        {
            //            Vector3 minMaxDiff = MaxVertice - MinVertice;

            Cuboids = CustomEntityPart.GetPartsFromCuboids(character.Cuboids, /*minMaxDiff,*/ GameBundle, character.HeightInBlocks);

            MinVertice = MinVertice / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER;
            MaxVertice = MaxVertice / Cuboid.MODEL_EDITOR_DIFF_MULTIPLIER;
        }

        private void InitializeAnimations(Character character)
        {
            BasicAnimations = new Dictionary<AnimationType, CompiledAnimation>();

            foreach (KeyValuePair<AnimationType, CompiledAnimation> animation in character.Animations)
            {
                BasicAnimations[animation.Key] = animation.Value;
            }
        }

        private void InitializeAnimationCuboids(Character character, Vector3 yDiff)
        {
            if (character.DefaultItemData.Cuboid != null)
            {
                character.DefaultItemData.Cuboid.MinPoint = character.DefaultItemData.Cuboid.MinPoint +
                                                            yDiff;
                character.DefaultItemData.Cuboid.MaxPoint = character.DefaultItemData.Cuboid.MaxPoint +
                                                            yDiff;

                DefaultItemHolderCuboid = new CompiledCuboid(character.DefaultItemData.Cuboid, isYTranslated: false);
            }

            ItemHolderCuboids = new Dictionary<string, CompiledCuboid>();

            foreach (KeyValuePair<string, ItemCuboidData> itemCuboidData in character.OverriddenItemsData)
            {
                itemCuboidData.Value.Cuboid.MinPoint = itemCuboidData.Value.Cuboid.MinPoint +
                                                            yDiff;
                itemCuboidData.Value.Cuboid.MaxPoint = itemCuboidData.Value.Cuboid.MaxPoint +
                                                            yDiff;

                ItemHolderCuboids[itemCuboidData.Key] = new CompiledCuboid(itemCuboidData.Value.Cuboid/*, isYTranslated: false*/);
            }
        }

        public Dictionary<AnimationType, ComputedAnimation> ComputeAnimations()
        {
            var result = new Dictionary<AnimationType, ComputedAnimation>();

            foreach (KeyValuePair<AnimationType, CompiledAnimation> animation in BasicAnimations)
            {
                result[animation.Key] = ComputedAnimation.CreateFromCompiledAnimation(animation.Value, isScaled: true);
            }

            return result;
        }
    }
}