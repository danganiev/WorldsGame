using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Saving
{
    [Serializable]
    public class WorldSettings : ISaveDataSerializable<WorldSettings>, IRuleHolder
    {
        public static readonly List<string> SPECIAL_KEYS = new List<string> { "height" };

        public string FileName { get { return Name + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "WorldSettings"; } }

        public int HierarchyLevel { get { return 0; } }

        public Dictionary<int, Guid> SubruleGuids { get; set; }

        public Dictionary<Guid, int> SubrulePriorities { get; set; }

        public Dictionary<AnimationType, CompiledAnimation> DefaultFirstPersonAnimations { get; set; }

        public string Name { get; set; }

        // A height which we expect to be sunlit
        public int SunlitHeight { get; set; }

        public Dictionary<int, Color> ColorsPerTimeOfDay { get; set; }

        public WorldSettings()
        {
            SubruleGuids = new Dictionary<int, Guid>();
            SubrulePriorities = new Dictionary<Guid, int>();
            DefaultFirstPersonAnimations = new Dictionary<AnimationType, CompiledAnimation>();
        }

        internal void PrepareDefaultSettings(WorldsGame game)
        {
            CharacterAttribute.CreateHealthAttribute(game, Name);
        }

        internal static SaverHelper<WorldSettings> StaticSaverHelper()
        {
            return new SaverHelper<WorldSettings>(StaticContainerName);
        }

        public SaverHelper<WorldSettings> SaverHelper()
        {
            return StaticSaverHelper();
        }

        public static WorldSettings Load(string name)
        {
            return StaticSaverHelper().Load(name);
        }

        public void Save()
        {
            SaverHelper().Save(this);
        }

        public void Delete()
        {
            DeleteObjects();
            DeleteBlocks();
            DeleteTextures();
            DeleteRules();
            DeleteNoises();
            DeleteCharacterAttributes();
            DeleteCharacters();
            DeleteRecipes();
            DeleteItems();
            SaverHelper().Delete(Name);
        }

        // These functions could perform faster with DirectoryInfo usage.
        private void DeleteRules()
        {
            foreach (KeyValuePair<int, Guid> subrule in SubruleGuids)
            {
                var rule = new Rule(Name, guid: subrule.Value);
                rule.Delete();
            }
        }

        public void DeleteTextures()
        {
            foreach (var texture in Textures)
            {
                texture.Delete();
            }
        }

        public void DeleteBlocks()
        {
            foreach (var block in Blocks)
            {
                block.Delete();
            }
        }

        public void DeleteObjects()
        {
            foreach (var gameObject in GameObjects)
            {
                gameObject.Delete();
            }
        }

        public void DeleteNoises()
        {
            foreach (Noise noise in Noises)
            {
                noise.Delete();
            }
        }

        public void DeleteCharacterAttributes()
        {
            foreach (CharacterAttribute ca in CharacterAttributes)
            {
                ca.Delete();
            }
        }

        public void DeleteCharacters()
        {
            foreach (Character character in Characters)
            {
                character.Delete();
            }
        }

        public void DeleteRecipes()
        {
            foreach (Recipe recipe in Recipes)
            {
                recipe.Delete();
            }
        }

        public void DeleteItems()
        {
            foreach (Item item in Items)
            {
                item.Delete();
            }
        }

        public void DeleteRule(Rule rule)
        {
            int priority = SubrulePriorities[rule.Guid];

            for (int i = priority; i < SubruleGuids.Keys.Count; i++)
            {
                SubruleGuids[i] = SubruleGuids[i + 1];
                SubrulePriorities[SubruleGuids[i]] = i;
            }

            SubruleGuids.Remove(SubruleGuids.Count);
            SubrulePriorities.Remove(rule.Guid);

            Save();

            rule.Delete();
        }

        public List<Texture> Textures
        {
            get
            {
                List<Texture> textures = Texture.SaverHelper(Name).LoadList();

                return textures;
            }
        }

        public List<Block> Blocks
        {
            get
            {
                List<Block> blocks = Block.SaverHelper(Name).LoadList();

                return blocks;
            }
        }

        public List<Noise> Noises
        {
            get
            {
                List<Noise> noises = Noise.SaverHelper(Name).LoadList();

                return noises;
            }
        }

        public List<string> NoiseNames
        {
            get { return Noises.Select(n => n.Name.ToLowerInvariant()).Distinct().Union(SPECIAL_KEYS).ToList(); }
        }

        public List<Rule> Rules
        {
            get
            {
                List<string> ruleNames = SubruleGuids.Select(guid => guid.Value.ToString()).ToList();
                List<Rule> rules = Rule.SaverHelper(Name).LoadList(ruleNames);

                return rules;
            }
        }

        public List<GameObject> GameObjects
        {
            get
            {
                List<GameObject> gameObjects = GameObject.SaverHelper(Name).LoadList();

                return gameObjects;
            }
        }

        public List<CharacterAttribute> CharacterAttributes
        {
            get
            {
                List<CharacterAttribute> characterAttributes = CharacterAttribute.SaverHelper(Name).LoadList();

                return characterAttributes;
            }
        }

        public List<Item> Items
        {
            get
            {
                List<Item> items = Item.SaverHelper(Name).LoadList();

                return items;
            }
        }

        public List<Recipe> Recipes
        {
            get
            {
                List<Recipe> recipes = Recipe.SaverHelper(Name).LoadList();

                return recipes;
            }
        }

        public List<Character> Characters
        {
            get
            {
                List<Character> characters = Character.SaverHelper(Name).LoadList();

                return characters;
            }
        }
    }
}