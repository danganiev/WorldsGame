using System;
using System.Collections.Generic;
using System.Linq;

using LibNoise.Xna;
using Microsoft.Xna.Framework;

using NCalc;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;
using WorldsLib;

using Random = System.Random;

namespace WorldsGame.Models.Terrain
{
    internal class SettingsChunkGenerator : ChunkGenerator
    {
        private readonly Dictionary<string, double[][][]> _noiseValues;
        private bool _isFirstIteration;
        private static readonly Random OBJECT_CREATION_DIRECTION_RANDOM = new Random();
        private Queue<string> _cachedNoiseNames = new Queue<string>();

        private Dictionary<Vector3i, KeyValuePair<int, BlockType>> CurrentPrecomputedBlocks { get; set; }

        private Dictionary<Guid, int> RulePriorities { get; set; }

        private int _currentPriority;

        internal SettingsChunkGenerator(CompiledGameBundle bundle)
            : base(bundle)
        {
            _noiseValues = new Dictionary<string, double[][][]>();

            BuildRulePriorityDict();
        }

        // Here might lay bugs with caching problems
        private List<string> _noiseNames;

        private List<string> NoiseNames
        {
            get
            {
                if (_noiseNames == null)
                {
                    var noiseNames = new List<string>();
                    foreach (KeyValuePair<int, CompiledRule> rule in Bundle.Rules)
                    {
                        noiseNames.AddRange(rule.Value.Parameters);
                    }
                    _noiseNames = noiseNames.Distinct().ToList();
                }
                return _noiseNames;
            }
        }

        private void BuildRulePriorityDict()
        {
            RulePriorities = new Dictionary<Guid, int>();
            _currentPriority = 0;

            foreach (int key in Bundle.Rules.Keys.OrderBy(i => i))
            {
                BuildRulePriorityDict(Bundle.Rules[key]);
            }
        }

        private void BuildRulePriorityDict(CompiledRule rule)
        {
            _currentPriority++;
            RulePriorities.Add(rule.Guid, _currentPriority);

            foreach (int key in rule.Subrules.Keys.OrderBy(i => i))
            {
                BuildRulePriorityDict(rule.Subrules[key]);
            }
        }

        private void SetDefaultNoiseValues(string noiseKey)
        {
            var noise = new double[Chunk.SIZE.X + 1][][];
            _noiseValues[noiseKey.ToLowerInvariant()] = noise;

            // One more element here for interpolation purposes
            for (int x = 0; x <= Chunk.SIZE.X; x++)
            {
                noise[x] = new double[Chunk.SIZE.Z + 1][];
                for (int z = 0; z <= Chunk.SIZE.Z; z++)
                {
                    noise[x][z] = new double[Chunk.SIZE.Y + 1];
                    for (int y = 0; y <= Chunk.SIZE.Y; y++)
                    {
                        noise[x][z][y] = 0;
                    }
                }
            }
        }

        internal override void Generate(Chunk chunk)
        {
            Chunkie = chunk;
            _cachedNoiseNames.Clear();
            _isFirstIteration = true;

            if (!PrecomputedBlocks.ContainsKey(chunk.Index))
            {
                PrecomputedBlocks[chunk.Index] = new Dictionary<Vector3i, KeyValuePair<int, BlockType>>();
            }

            CurrentPrecomputedBlocks = PrecomputedBlocks[chunk.Index];

            _noiseValues.Clear();

            _cachedNoiseNames = new Queue<string>(NoiseNames.Except(WorldSettings.SPECIAL_KEYS));

            while (_cachedNoiseNames.Count > 0)
            {
                string cachedNoiseName = _cachedNoiseNames.Dequeue();
                PopulateNoise(cachedNoiseName);
            }

            foreach (int key in Bundle.Rules.Keys.OrderBy(i => i))
            {
                CompiledRule rule = Bundle.Rules[key];
                CheckRuleConditions(rule);
                _isFirstIteration = false;
            }

            foreach (KeyValuePair<Vector3i, KeyValuePair<int, BlockType>> keyValuePair in PrecomputedBlocks[chunk.Index])
            {
                Chunkie.SetBlock(keyValuePair.Key, keyValuePair.Value.Value);
            }

            PrecomputedBlocks[chunk.Index].Clear();
        }

        private void CheckRuleConditions(CompiledRule compiledRule)
        {
            _currentPriority++;
            for (int x = 0; x < Chunk.SIZE.X; x++)
            {
                for (int z = 0; z < Chunk.SIZE.Z; z++)
                {
                    for (int y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        if (_isFirstIteration)
                        {
                            Chunkie.SetDefaultBlock(x, y, z);
                        }

                        try
                        {
                            if (!PrecomputedBlocks[Chunkie.Index].ContainsKey(new Vector3i(x, y, z)))
                            {
                                CheckRuleConditions(compiledRule, x, y, z, RulePriorities[compiledRule.Guid]);
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                        }
                    }
                }
            }
        }

        private void CheckRuleConditions(CompiledRule compiledRule, int x, int y, int z, int priority)
        {
            int worldY = Chunkie.Position.Y + y;
            compiledRule.Condition.Expression.Parameters.Clear();
            //NOTE: Little possibility of optimization, if we start with Y loop

            //System parameter, should be moved outta here.
            compiledRule.Condition.Expression.Parameters["height"] = worldY;

            foreach (string parameter in compiledRule.Condition.Parameters.Except(WorldSettings.SPECIAL_KEYS))
            {
                compiledRule.Condition.Expression.Parameters[parameter.ToLowerInvariant()] = _noiseValues[parameter.ToLowerInvariant()][x][z][y];
            }

            bool expressionResult;

            try
            {
                if (compiledRule.Condition.HasErrors())
                {
                    expressionResult = false;
                }
                else
                {
                    expressionResult = (bool)compiledRule.Condition.Expression.Evaluate();
                }
            }
            catch (EvaluationException)
            {
                expressionResult = false;
            }
            catch (InvalidCastException)
            {
                expressionResult = false;
            }

            if (expressionResult)
            {
                ApplyRule(compiledRule, x, y, z, priority);
            }
        }

        private void ApplyRule(CompiledRule compiledRule, int x, int y, int z, int priority)
        {
            switch (compiledRule.ActionType)
            {
                case RuleActionsEnum.UseSubrules:
                    ApplySubrules(compiledRule, x, y, z, priority);
                    break;

                case RuleActionsEnum.PlaceBlock:
                    PlaceBlock(compiledRule, x, y, z, priority);
                    break;

                case RuleActionsEnum.PlaceObject:
                    PlaceObject(compiledRule, x, y, z, priority);
                    break;

                case RuleActionsEnum.AddSpawnData:
                    FillSpawnData(compiledRule);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplySubrules(CompiledRule compiledRule, int x, int y, int z, int priority)
        {
            foreach (int key in compiledRule.Subrules.Keys.OrderBy(i => i))
            {
                CheckRuleConditions(compiledRule.Subrules[key], x, y, z, priority + key);
            }
        }

        private void PlaceBlock(CompiledRule compiledRule, int x, int y, int z, int priority)
        {
            var key = new Vector3i(x, y, z);
            if (CurrentPrecomputedBlocks.ContainsKey(key) && CurrentPrecomputedBlocks[key].Key > priority)
            {
                return;
            }

            BlockType blockType = BlockTypeHelper.Get(compiledRule.BlockName);

            Chunkie.SetBlock(x, y, z, blockType);
        }

        private void PlaceObject(CompiledRule compiledRule, int x, int y, int z, int priority)
        {
            Array enumValues = Enum.GetValues(typeof(GameObjectDirection));

            var randomBar = (GameObjectDirection)enumValues.GetValue(
                OBJECT_CREATION_DIRECTION_RANDOM.Next(enumValues.Length));

            Vector2 direction;

            switch (randomBar)
            {
                case GameObjectDirection.North:
                    direction = new Vector2(1, 1);
                    break;

                case GameObjectDirection.East:
                    direction = new Vector2(-1, 1);
                    break;

                case GameObjectDirection.South:
                    direction = new Vector2(-1, -1);
                    break;

                case GameObjectDirection.West:
                    direction = new Vector2(1, -1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            CompiledGameObject gameObject = Bundle.GetObject(compiledRule.ObjectName);

            Chunkie.World.SetObject(gameObject, Chunkie.GetWorldPosition(x, y, z), direction, priority);
        }

        private void FillSpawnData(CompiledRule compiledRule)
        {
            Chunkie.SetSpawnData(compiledRule);
        }

        private void PopulateNoise(string noiseName)
        {
            noiseName = noiseName.ToLowerInvariant();
            if (_noiseValues.ContainsKey(noiseName))
            {
                return;
            }

            ModuleBase noiseModule = Bundle.GetNoise(noiseName).NoiseFunction;

            if (noiseModule.Type == ModuleBaseType.ValueStorage)
            {
                // If there is no noise in the whole bundle, name of which user entered in "Noise" module, then we can't do
                // anything

                // We are avoiding LINQ Any at all costs
                for (int i = 0; i < noiseModule.Keys.Count; i++)
                {
                    string key = noiseModule.Keys[i];
                    if (!Bundle.NoiseNames.Contains(key))
                    {
                        SetDefaultNoiseValues(noiseName);

                        return;
                    }
                }
                // We do not welcome recursion here

                // We are avoiding LINQ Any at all costs
                for (int i = 0; i < noiseModule.Keys.Count; i++)
                {
                    string key = noiseModule.Keys[i];
                    if (key == noiseName)
                    {
                        SetDefaultNoiseValues(noiseName);

                        return;
                    }
                }
                // If there is no key in NoiseNames, then Rules for current chunk just don't use it, so we add it to calculation
                // queue
                // (Actually everything now is calculated, should work on optimizing starting with subrules)
                bool shouldReturn = false;
                for (int i = 0; i < noiseModule.Keys.Count; i++)
                {
                    string key = noiseModule.Keys[i];
                    if (!NoiseNames.Contains(key))
                    {
                        _cachedNoiseNames.Enqueue(key);
                        _cachedNoiseNames.Enqueue(noiseName);
                        NoiseNames.Add(key);
                        shouldReturn = true;
                    }
                }
                if (shouldReturn)
                {
                    return;
                }
                for (int i = 0; i < noiseModule.Keys.Count; i++)
                {
                    string key = noiseModule.Keys[i];
                    // Noise wasn't calculated yet, we keep waiting.
                    if (!_noiseValues.ContainsKey(key))
                    {
                        _cachedNoiseNames.Enqueue(noiseName);
                        return;
                    }
                }

                // Noise calculated! So we take it's values
                for (int i = 0; i < noiseModule.Keys.Count; i++)
                {
                    string key = noiseModule.Keys[i];
                    noiseModule.SetValues(key, _noiseValues[key]);
                }
            }

            noiseModule.Clear();
            SetDefaultNoiseValues(noiseName);

            double[][][] noiseTable = _noiseValues[noiseName];

            FillNoiseTable(noiseTable, noiseModule);
            InterpolateNoiseTable(noiseTable);
        }

        private void FillNoiseTable(double[][][] noiseTable, ModuleBase noiseModule)
        {
            //            bool isStorage = noiseModule.Type == ModuleBaseType.ValueStorage;

            for (int x = 0; x <= Chunk.SIZE.X; x += SAMPLE_RATE_3D_HOR)
            {
                int worldX = Chunkie.Position.X + x;

                for (int z = 0; z <= Chunk.SIZE.Z; z += SAMPLE_RATE_3D_HOR)
                {
                    int worldZ = Chunkie.Position.Z + z;

                    for (int y = 0; y <= Chunk.SIZE.Y; y += SAMPLE_RATE_3D_VERT)
                    {
                        noiseTable[x][z][y] = noiseModule.GetValue(worldX, Chunkie.Position.Y + y, worldZ);
                    }
                }
            }
        }

        private void InterpolateNoiseTable(double[][][] noiseTable)
        {
            //TODO: we shouldn't interpolate, say, for constants
            for (int x = 0; x < Chunk.SIZE.X; x++)
            {
                for (int z = 0; z < Chunk.SIZE.Z; z++)
                {
                    for (int y = 0; y < Chunk.SIZE.Y; y++)
                    {
                        if (!(x % SAMPLE_RATE_3D_HOR == 0 && y % SAMPLE_RATE_3D_VERT == 0 && z % SAMPLE_RATE_3D_HOR == 0))
                        {
                            int offsetX = (x / SAMPLE_RATE_3D_HOR) * SAMPLE_RATE_3D_HOR;
                            int offsetY = (y / SAMPLE_RATE_3D_VERT) * SAMPLE_RATE_3D_VERT;
                            int offsetZ = (z / SAMPLE_RATE_3D_HOR) * SAMPLE_RATE_3D_HOR;

                            noiseTable[x][z][y] = MathUtils.TriLerp(x, y, z, noiseTable[offsetX][offsetZ][offsetY],
                                noiseTable[offsetX][offsetZ][SAMPLE_RATE_3D_VERT + offsetY],
                                noiseTable[offsetX][offsetZ + SAMPLE_RATE_3D_HOR][offsetY],
                                noiseTable[offsetX][offsetZ + SAMPLE_RATE_3D_HOR][offsetY + SAMPLE_RATE_3D_VERT],
                                noiseTable[SAMPLE_RATE_3D_HOR + offsetX][offsetZ][offsetY],
                                noiseTable[SAMPLE_RATE_3D_HOR + offsetX][offsetZ][offsetY + SAMPLE_RATE_3D_VERT],
                                noiseTable[SAMPLE_RATE_3D_HOR + offsetX][offsetZ + SAMPLE_RATE_3D_HOR][offsetY],
                                noiseTable[SAMPLE_RATE_3D_HOR + offsetX][offsetZ + SAMPLE_RATE_3D_HOR][offsetY + SAMPLE_RATE_3D_VERT],
                                offsetX, SAMPLE_RATE_3D_HOR + offsetX, offsetY, SAMPLE_RATE_3D_VERT + offsetY, offsetZ,
                                offsetZ + SAMPLE_RATE_3D_HOR);
                        }
                    }
                }
            }
        }

        private void ChunkDisposing(Vector3i index)
        {
            //This condition is a shitty solution to a strange thread-related problem at the start of the game
            //            if (PrecomputedBlocks.ContainsKey(index))
            //            {
            //                PrecomputedBlocks[index].Clear();
            //            }
        }

        internal override void ClearAfterGenerating()
        {
            Chunkie = null;

            EverythingGenerated = true;

            _noiseValues.Clear();
        }

        public override void Dispose()
        {
            PrecomputedBlocks.Clear();
            CurrentPrecomputedBlocks = null;
            Messenger.Off("ChunkDisposing");
        }
    }
}