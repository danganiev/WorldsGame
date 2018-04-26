using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving.World;
using WorldsGame.Utils;
using WorldsGame.Utils.Textures;

namespace WorldsGame.Saving.DataClasses
{
    [Serializable]
    public class CompiledGameBundleSave : ISaveDataSerializable<CompiledGameBundleSave>
    {
        public string FileName { get { return FullName + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "CompiledGameBundles"; } }

        public static string NetworkContainerName { get { return "NetworkBundles"; } }

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

        public string Name { get; set; }

        public string Guid { get; set; }

        public List<string> AtlasNames { get; set; }

        public List<CompiledTexture> CompiledTextures { get; set; }

        public List<CompiledBlock> CompiledBlocks { get; set; }

        public List<CompiledGameObject> CompiledGameObjects { get; set; }

        public List<CompiledNoise> CompiledNoises { get; set; }

        public Dictionary<int, CompiledRule> CompiledRules { get; set; }

        public List<CompiledCharacterAttribute> CompiledCharacterAttributes { get; set; }

        public Dictionary<string, int> BlockNameMap { get; set; }

        public List<CompiledCharacter> CompiledCharacters { get; set; }

        public List<CompiledItem> CompiledItems { get; set; }

        public List<CompiledRecipe> CompiledRecipes { get; set; }

        public string WorldSettingsName { get; set; }

        public int AtlasCount { get; set; }

        public int SunlitHeight { get; set; }

        public static string GetFullFilePath(string fullName, BundleType bundleType, bool isAtlas = false, string additionalContainerName = "")
        {
            string containerName = ContainerNameFromType(bundleType);
            containerName = Path.Combine(containerName, additionalContainerName);

            string fullnameExtension = fullName.Split('.').Last();

            string extension = isAtlas ? "png" : "sav";

            if (fullnameExtension != extension)
            {
                fullName += string.Format(".{0}", extension);
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SavedGames",
                Constants.SaveGamesFolder,
                containerName,
                "AllPlayers",
                fullName);
        }

        public static string GetAtlasesPath(string fullName, BundleType bundleType)
        {
            string containerName = ContainerNameFromType(bundleType);
            //            containerName = Path.Combine(containerName, additionalContainerName);

            string fullnameExtension = fullName.Split('.').Last();

            const string extension = "png";

            if (fullnameExtension != extension)
            {
                fullName += string.Format(".{0}", extension);
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SavedGames",
                Constants.SaveGamesFolder,
                containerName,
                //                "AllPlayers",
                fullName);
        }

        private static string ContainerNameFromType(BundleType bundleType)
        {
            return bundleType == BundleType.Normal ? StaticContainerName : NetworkContainerName;
        }

        internal static SaverHelper<CompiledGameBundleSave> SaverHelper(string name, BundleType bundleType = BundleType.Normal)
        {
            return new SaverHelper<CompiledGameBundleSave>(ContainerNameFromType(bundleType)) { DirectoryRelativePath = name };
        }

        public SaverHelper<CompiledGameBundleSave> SaverHelper()
        {
            return SaverHelper("");
        }

        public CompiledGameBundle ToCompiledGameBundle(ContentManager content, GraphicsDevice graphicsDevice, BundleType bundleType = BundleType.Normal)
        {
            var bundle = new CompiledGameBundle(
                Name, textures: CompiledTextures, blocks: CompiledBlocks, gameObjects: CompiledGameObjects,
                noises: CompiledNoises, rules: CompiledRules, characterAttributes: CompiledCharacterAttributes,
                characters: CompiledCharacters, items: CompiledItems, recipes: CompiledRecipes,
                blockNameMap: BlockNameMap, worldSettingsName: WorldSettingsName)
            {
                Guid = Guid
            };
            bundle.SunlitHeight = SunlitHeight;

            LoadAtlases(content, graphicsDevice, bundle, bundleType);

            foreach (CompiledBlock block in bundle.Blocks)
            {
                block.GameBundle = bundle;
                block.InitializeCube();
            }

            foreach (CompiledGameObject compiledGameObject in bundle.GameObjects)
            {
                compiledGameObject.GameBundle = bundle;
            }

            return bundle;
        }

        private void LoadAtlases(ContentManager content, GraphicsDevice graphicsDevice, CompiledGameBundle bundle, BundleType bundleType = BundleType.Normal)
        {
            Texture2D systemAtlas = content.Load<Texture2D>("Textures\\systemAtlas");

            bundle.TextureAtlases.Add(GameBundleCompiler.SYSTEM_ATLAS_ID,
                                      new TextureAtlas(Constants.TEXTURE_SIZE * 4, systemAtlas, GameBundleCompiler.SYSTEM_ATLAS_ID));

            //            int textureCount = bundle.Textures.Count / GameBundleCompiler.TEXTURE_COUNT_PER_ATLAS + 1;

            for (int i = 0; i < AtlasCount; i++)
            {
                var textureAtlas = new Texture2D(graphicsDevice, GameBundleCompiler.ATLAS_SIZE, GameBundleCompiler.ATLAS_SIZE,
                    mipMap: true, format: SurfaceFormat.Color);

                var textureLoader = new TextureLoader(textureAtlas, FullName, i, graphicsDevice);
                textureLoader.Load(textureAtlas.LevelCount, bundleType);

                bundle.TextureAtlases.Add(i, new TextureAtlas(GameBundleCompiler.ATLAS_SIZE, textureAtlas, i));
            }

            LoadCharacterAttributeIconsAtlas(graphicsDevice, bundle);
        }

        private void LoadCharacterAttributeIconsAtlas(GraphicsDevice graphicsDevice, CompiledGameBundle bundle)
        {
            Texture2D atlas = SaverHelper(FullName).LoadAtlas(CompiledCharacterAttribute.AtlasName, graphicsDevice);
            bundle.CharacterAttributeIconsAtlas = atlas;
        }

        public void Delete()
        {
            SaverHelper().Delete(FullName);
        }

        public static void Delete(string name)
        {
            SaverHelper("").Delete(name);
        }

        public void Save()
        {
            SaverHelper().Save(this);
        }
    }
}