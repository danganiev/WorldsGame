using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Saving.World;
using SystemGuid = System.Guid;

namespace WorldsGame.Playing.DataClasses
{
    public enum BundleType
    {
        Normal,
        Network
    }

    public class CompiledGameBundle : IDisposable
    {
        public string Name { get; private set; }

        public string WorldSettingsName { get; private set; }

        //NON-SORTED list of textures involved
        public List<CompiledTexture> Textures { get; private set; }

        public List<CompiledBlock> Blocks { get; private set; }

        // BlockNameMap is needed for effective chunk saving
        public Dictionary<string, int> BlockNameMap { get; private set; }

        // BlockIndexMap is needed for effective loading
        public Dictionary<int, string> BlockIndexMap { get; private set; }

        public List<CompiledGameObject> GameObjects { get; private set; }

        public List<CompiledNoise> Noises { get; private set; }

        public Dictionary<int, CompiledRule> Rules { get; private set; }

        public List<CompiledCharacterAttribute> CharacterAttributes { get; private set; }

        public Dictionary<int, TextureAtlas> TextureAtlases { get; private set; }

        public Texture2D CharacterAttributeIconsAtlas { get; set; }

        public List<CompiledCharacter> Characters { get; private set; }

        public List<CompiledItem> Items { get; private set; }

        public List<CompiledRecipe> Recipes { get; private set; }

        public Dictionary<int, Color> ColorsPerTimeOfDay { get; private set; }

        /// <summary>
        /// The name of character that player is playing
        /// </summary>
        public string PlayerCharacterName { get; private set; }

        public string Guid { get; set; }

        public int SunlitHeight { get; set; }

        public string FullName
        {
            get
            {
                if (Name != WorldSave.OBJECT_CREATION_WORLD_NAME)
                {
                    return Name + Guid;
                }

                return WorldSave.OBJECT_CREATION_WORLD_NAME;
            }
        }

        public string GetFullFilePath(BundleType bundleType = BundleType.Normal)
        {
            return CompiledGameBundleSave.GetFullFilePath(FullName, bundleType);
        }

        public List<string> NoiseNames
        {
            get { return Noises.Select(n => n.Name.ToLowerInvariant()).Distinct().Union(WorldSettings.SPECIAL_KEYS).ToList(); }
        }

        public CompiledGameBundle(string name, Dictionary<int, TextureAtlas> textureAtlases = null,
            List<CompiledTexture> textures = null, List<CompiledBlock> blocks = null, List<CompiledGameObject> gameObjects = null,
            List<CompiledNoise> noises = null, Dictionary<int, CompiledRule> rules = null,
            List<CompiledCharacterAttribute> characterAttributes = null, List<CompiledCharacter> characters = null,
            List<CompiledItem> items = null, List<CompiledRecipe> recipes = null,
            Dictionary<string, int> blockNameMap = null, string worldSettingsName = "")
        {
            Name = name;
            Guid = SystemGuid.NewGuid().ToString();
            TextureAtlases = textureAtlases ?? new Dictionary<int, TextureAtlas>();
            Textures = textures ?? new List<CompiledTexture>();
            Blocks = blocks ?? new List<CompiledBlock>();
            RemapBlocks(blockNameMap: blockNameMap);
            GameObjects = gameObjects ?? new List<CompiledGameObject>();
            Noises = noises ?? new List<CompiledNoise>();
            Rules = rules ?? new Dictionary<int, CompiledRule>();
            CharacterAttributes = characterAttributes ?? new List<CompiledCharacterAttribute>();
            Characters = characters ?? new List<CompiledCharacter>();
            Items = items ?? new List<CompiledItem>();
            Recipes = recipes ?? new List<CompiledRecipe>();

            WorldSettingsName = worldSettingsName;

            SunlitHeight = 0;

            ColorsPerTimeOfDay = new Dictionary<int, Color>
            {
//                {0, Color.DarkBlue},
                {0, Color.FloralWhite},
                {12, Color.FloralWhite}
            };
        }

        internal void RemapBlocks(Dictionary<string, int> blockNameMap = null)
        {
            BlockNameMap = new Dictionary<string, int>();
            BlockIndexMap = new Dictionary<int, string>();

            if (blockNameMap == null)
            {
                for (int index = 0; index < Blocks.Count; index++)
                {
                    CompiledBlock compiledBlock = Blocks[index];
                    BlockNameMap[compiledBlock.Name] = index;
                    BlockIndexMap[index] = compiledBlock.Name;
                }

                BlockNameMap[CompiledBlock.AIR_CUBE] = -1;
                BlockIndexMap[-1] = CompiledBlock.AIR_CUBE;
            }
            else
            {
                foreach (KeyValuePair<string, int> keyValuePair in blockNameMap)
                {
                    BlockNameMap[keyValuePair.Key] = keyValuePair.Value;
                    BlockIndexMap[keyValuePair.Value] = keyValuePair.Key;
                }
            }
        }

        public static string TextureAtlasName(int number, int mipmapLevel = 0)
        {
            return mipmapLevel == 0 ? String.Format("atlas{0}.png", number) : String.Format("atlas{0}__mipmap{1}.png", number, mipmapLevel);
        }

        public CompiledGameBundleSave ToSaveFile()
        {
            var save = new CompiledGameBundleSave
            {
                Name = Name,
                Guid = Guid,
                CompiledTextures = Textures,
                CompiledBlocks = Blocks,
                BlockNameMap = BlockNameMap,
                CompiledGameObjects = GameObjects,
                CompiledNoises = Noises,
                CompiledRules = Rules,
                CompiledCharacterAttributes = CharacterAttributes,
                CompiledCharacters = Characters,
                CompiledItems = Items,
                CompiledRecipes = Recipes,
                AtlasCount = TextureAtlases.Count - 1,
                SunlitHeight = SunlitHeight
            };

            return save;
        }

        public TextureAtlas GetTextureAtlas(int index)
        {
            return TextureAtlases[index];
        }

        // If get methods below will start to hampering everything, cache dictionaries should be initialized and filled in the constructor
        public CompiledTexture GetTexture(string textureName)
        {
            return (from texture in Textures
                    where texture.Name == textureName
                    select texture).FirstOrDefault();
        }

        public CompiledGameObject GetObject(string objectName)
        {
            return (from obj in GameObjects
                    where obj.Name == objectName
                    select obj).FirstOrDefault();
        }

        public CompiledNoise GetNoise(string noiseName)
        {
            return (from noise in Noises
                    where noise.Name.ToLowerInvariant() == noiseName.ToLowerInvariant()
                    select noise).FirstOrDefault();
        }

        public CompiledItem GetItem(string name)
        {
            return (from item in Items
                    where item.Name.ToLowerInvariant() == name.ToLowerInvariant()
                    select item).FirstOrDefault();
        }

        public void SeedNoises(int seed)
        {
            int newSeed = seed;
            foreach (CompiledNoise compiledNoise in Noises)
            {
                newSeed = compiledNoise.NoiseFunction.SetSeed(newSeed);
            }
        }

        public void Dispose()
        {
            foreach (KeyValuePair<int, TextureAtlas> atlas in TextureAtlases)
            {
                atlas.Value.Texture.Dispose();
            }
        }
    }
}