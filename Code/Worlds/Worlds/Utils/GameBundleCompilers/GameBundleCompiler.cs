using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Editors.Textures;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Files;
using WorldsGame.Utils.Textures;
using Texture = WorldsGame.Saving.Texture;

namespace WorldsGame.Utils
{
    internal class GameBundleCompiler
    {
        internal const int SYSTEM_ATLAS_ID = -1;
        internal const int ATLAS_SIZE = 2048;
        //        internal const int TEXTURE_COUNT_PER_ATLAS = (ATLAS_SIZE * ATLAS_SIZE) / ((Constants.TEXTURE_SIZE + 2) * (Constants.TEXTURE_SIZE + 2));

        protected readonly GraphicsDevice graphics;
        protected readonly ContentManager content;

        internal CompiledGameBundle CompiledGameBundle { get; private set; }

        private readonly string _worldName;

        private WorldSettings WorldSettings { get; set; }

        private SaverHelper<CompiledGameBundleSave> BundleSaverHelper
        {
            get
            {
                return CompiledGameBundleSave.SaverHelper(CompiledGameBundle.FullName);
            }
        }

        internal bool CompileForObjectCreation { get; set; }

        private bool _isCompileStarted;

        internal bool IsCompileStarted
        {
            get { return _isCompileStarted; }
            private set
            {
                _isCompileStarted = value;
                if (value)
                {
                    IsCompileFinished = false;
                }
            }
        }

        private bool _isCompileFinished;

        internal bool IsCompileFinished
        {
            get { return _isCompileFinished; }
            private set
            {
                _isCompileFinished = value;
                if (value)
                {
                    IsCompileStarted = false;
                }
            }
        }

        private List<string> _textureNames;

        internal GameBundleCompiler(WorldSettings worldSettings, GraphicsDevice graphics, Game game, string worldName)
        {
            WorldSettings = worldSettings;
            this.graphics = graphics;
            content = new ContentManager(game.Services, "Content");
            _worldName = worldName;

            _textureNames = new List<string>();
        }

        internal void Compile()
        {
            Messenger.Invoke("LoadingMessageChange", "Compiling started...");
            IsCompileStarted = true;
            CompiledGameBundle = null;

            if (WorldSettings != null)
            {
                CompiledGameBundle = new CompiledGameBundle(_worldName, worldSettingsName: WorldSettings.Name);
                CompiledGameBundle.SunlitHeight = WorldSettings.SunlitHeight;

                Messenger.Invoke("LoadingMessageChange", "Filling required default data...");
                CheckPlayerCharacter();
                Messenger.Invoke("LoadingMessageChange", "Compiling atlases...");
                AddSystemAtlasTextures();
                CompileAtlases();
                Messenger.Invoke("LoadingMessageChange", "Compiling blocks...");
                CompileBlocks();
                Messenger.Invoke("LoadingMessageChange", "Compiling objects...");
                CompileObjects();
                Messenger.Invoke("LoadingMessageChange", "Compiling character attributes...");
                CompileCharacterAttributes();
                Messenger.Invoke("LoadingMessageChange", "Compiling characters...");
                CompileCharacters();
                Messenger.Invoke("LoadingMessageChange", "Compiling items and recipes...");
                CompileItems();
                CompileRecipes();

                if (!CompileForObjectCreation)
                {
                    Messenger.Invoke("LoadingMessageChange", "Compiling noises...");
                    CompileNoises();
                    Messenger.Invoke("LoadingMessageChange", "Compiling rules...");
                    CompileRules();
                }

                Messenger.Invoke("LoadingMessageChange", "Copying sounds...");
                CopyDefaultSounds();

                CompiledGameBundleSave bundleSave = CompiledGameBundle.ToSaveFile();
                bundleSave.Save();
            }

            Messenger.Invoke("LoadingMessageChange", "Compiling finished!");
            IsCompileFinished = true;
        }

        private void CompileAtlases()
        {
            // We are creating atlases first, then save them to disk with mipmaps, then load them with mipmaps from disk again.
            //            CalculateAndSaveAtlases(CompiledGameBundle.Textures, WorldSettings.Textures, AddCompiledTexture);
            int atlasesCount = CalculateAndSaveAtlases(CompiledGameBundle.Textures, WorldSettings.Textures);

            Texture2D systemAtlas = content.Load<Texture2D>("Textures\\systemAtlas");
            CompiledGameBundle.TextureAtlases.Add(SYSTEM_ATLAS_ID, new TextureAtlas(128, systemAtlas, SYSTEM_ATLAS_ID));

            for (int i = 0; i < atlasesCount; i++)
            {
                var textureAtlas = new Texture2D(graphics, ATLAS_SIZE, ATLAS_SIZE, mipMap: true, format: SurfaceFormat.Color);

                var textureLoader = new TextureLoader(textureAtlas, CompiledGameBundle.FullName, i, graphics);
                textureLoader.Load(textureAtlas.LevelCount);

                CompiledGameBundle.TextureAtlases.Add(i, new TextureAtlas(ATLAS_SIZE, textureAtlas, i));
            }
        }

        private void SaveAtlas(Texture2D textureAtlas, int atlasIndex)
        {
            BundleSaverHelper.SaveAtlas(CompiledGameBundle.TextureAtlasName(atlasIndex), textureAtlas);
        }

        private void SaveIconAtlas(Texture2D atlas)
        {
            BundleSaverHelper.SaveAtlas(CompiledCharacterAttribute.AtlasName, atlas);
        }

        private void AddSystemAtlasTextures()
        {
            // System textures don't need Colors field filled, cause system atlas is same for everyone
            CompiledGameBundle.Textures.Add(new CompiledTexture(CompiledGameBundle, CompiledTexture.WHITE_TEXTURE_NAME, SYSTEM_ATLAS_ID, 0, 0));

            CompiledGameBundle.Textures.Add(new CompiledTexture(CompiledGameBundle, CompiledTexture.WAIT_TEXTURE_NAME, SYSTEM_ATLAS_ID, 32, 0));
        }

        private int CalculateAndSaveAtlases(List<CompiledTexture> compiledTextures, IEnumerable<Texture> textures)
        {
            int atlasIndex = 0;

            int xIndex = 0, yIndex = 0;
            int maxYIndex = 0;
            var colorData = new Color[ATLAS_SIZE * ATLAS_SIZE];
            int fullTextureSize = Constants.TEXTURE_SIZE + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2;

            var texturesList = textures.OrderByDescending(texture => texture.Weight).ToList();

            for (int i = 0; i < texturesList.Count(); i++)
            {
                Texture texture = texturesList[i];
                int textureWidth = texture.Width;
                int textureHeight = texture.Height;

                int frameCount = texture.IsAnimated ? CompiledTexture.ANIMATED_FRAME_COUNT : 1;

                CompiledTexture compiledTexture = null;

                for (int j = 0; j < frameCount; j++)
                {
                    GetXYIndicesAndFillColors(
                        colorData, texture, textureHeight, textureWidth, fullTextureSize,
                        texture.IsAnimated, j, ref maxYIndex, ref xIndex, ref yIndex);

                    // functional hack to make method static and that won't be used anywhere for now
                    AddCompiledTexture(compiledTextures, texture, atlasIndex, xIndex, yIndex, j, texture.IsAnimated, ref compiledTexture);

                    xIndex += textureWidth + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2;

                    // This leaves a bit of space on the right, but meh
                    if (xIndex >= ATLAS_SIZE - (ATLAS_SIZE % fullTextureSize))
                    {
                        xIndex = 0;
                        yIndex += maxYIndex + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2;
                        maxYIndex = 0;
                    }

                    if (xIndex >= ATLAS_SIZE - (ATLAS_SIZE % fullTextureSize) &&
                        yIndex >= ATLAS_SIZE - (ATLAS_SIZE % fullTextureSize) ||
                        i == texturesList.Count() - 1)
                    {
                        var textureAtlas = new Texture2D(graphics, ATLAS_SIZE, ATLAS_SIZE, mipMap: true, format: SurfaceFormat.Color);
                        textureAtlas.SetData(0, null, colorData, 0, colorData.Length);

                        SaveAtlas(textureAtlas, atlasIndex);

                        var mipmapCompiler = new MipmapCompilerSaver(colorData, textureAtlas.LevelCount, atlasIndex);
                        mipmapCompiler.CompileAndSave(CompiledGameBundle.FullName, graphics);

                        xIndex = 0;
                        yIndex = 0;
                        colorData = new Color[ATLAS_SIZE * ATLAS_SIZE];
                        atlasIndex++;
                    }
                }
            }
            return atlasIndex;
        }

        private static void GetXYIndicesAndFillColors(
            Color[] colorData, Texture texture, int textureHeight, int textureWidth,
            int fullTextureSize, bool isAnimated, int currentFrame, ref int maxYIndex, ref int xIndex, ref int yIndex)
        {
            if (xIndex + textureWidth + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2 >=
                ATLAS_SIZE - (ATLAS_SIZE % fullTextureSize))
            {
                xIndex = 0;
                yIndex += maxYIndex + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2;
                maxYIndex = 0;
            }

            var tempXIndex = xIndex;
            var tempYIndex = yIndex;

            int colorIndex = 0;

            maxYIndex = Math.Max(maxYIndex, textureHeight);

            Color[] colors = isAnimated ? texture.FrameColors[currentFrame] : texture.Colors;

            while (colorIndex < colors.Length)
            {
                Color color = colors[colorIndex];
                FillCornerColors(colorIndex, tempXIndex, tempYIndex, color, colorData, textureWidth,
                                 textureHeight);
                FillOneColor(tempXIndex, tempYIndex, color, colorData);

                tempXIndex++;
                colorIndex++;

                if (colorIndex % textureWidth == 0)
                {
                    tempYIndex++;
                    tempXIndex = xIndex;
                }
            }
            //            return xIndex;
        }

        private static void FillOneColor(int tempXIndex, int tempYIndex, Color color, Color[] colorData)
        {
            if (color.A == 0 && color.B == 0 && color.R == 0 && color.G == 0)
            {
                colorData[GetColorIndex(tempXIndex, tempYIndex)] =
                    new Color(0f, 0f, 0f, 0f);
            }
            else
            {
                colorData[GetColorIndex(tempXIndex, tempYIndex)] = color;
            }
        }

        private static int GetColorIndex(int tempXIndex, int tempYIndex)
        {
            return tempXIndex + Constants.TEXTURE_MIPMAP_BORDER_SIZE + (tempYIndex + Constants.TEXTURE_MIPMAP_BORDER_SIZE) * ATLAS_SIZE;
        }

        private static void FillCornerColors(int colorIndex, int tempXIndex, int tempYIndex, Color color, Color[] colorData,
            int textureWidth, int textureHeight)
        {
            color = (color.A == 0 && color.B == 0 && color.R == 0 && color.G == 0) ? new Color(0f, 0f, 0f, 0f) : color;

            if (colorIndex == 0)
            {
                colorData[tempXIndex + tempYIndex * ATLAS_SIZE] = color;
                colorData[tempXIndex + 1 + tempYIndex * ATLAS_SIZE] = color;
                colorData[tempXIndex + (tempYIndex + 1) * ATLAS_SIZE] = color;
            }
            if (colorIndex == textureWidth - 1)
            {
                colorData[tempXIndex + 1 + tempYIndex * ATLAS_SIZE] = color;
                colorData[tempXIndex + 1 + 1 + (tempYIndex + 1) * ATLAS_SIZE] = color;
                colorData[tempXIndex + 1 + 1 + tempYIndex * ATLAS_SIZE] = color;
            }
            if (colorIndex == (textureHeight * textureWidth) - textureWidth)
            {
                colorData[tempXIndex + (tempYIndex + 1) * ATLAS_SIZE] = color;
                colorData[tempXIndex + (tempYIndex + 1 + 1) * ATLAS_SIZE] = color;
                colorData[tempXIndex + 1 + (tempYIndex + 1 + 1) * ATLAS_SIZE] = color;
            }
            if (colorIndex == (textureHeight * textureWidth) - 1)
            {
                colorData[tempXIndex + 1 + (tempYIndex + 1 + 1) * ATLAS_SIZE] = color;
                colorData[tempXIndex + 1 + 1 + (tempYIndex + 1 + 1) * ATLAS_SIZE] = color;
                colorData[tempXIndex + 1 + 1 + (tempYIndex + 1) * ATLAS_SIZE] = color;
            }
            if (colorIndex < textureWidth)
            {
                colorData[tempXIndex + 1 + tempYIndex * ATLAS_SIZE] = color;
            }
            if (colorIndex > (textureHeight * textureWidth) - textureWidth)
            {
                colorData[tempXIndex + 1 + (tempYIndex + 1 + 1) * ATLAS_SIZE] = color;
            }
            if (colorIndex % textureWidth == 0 || textureWidth == 1)
            {
                colorData[tempXIndex + (tempYIndex + 1) * ATLAS_SIZE] = color;
            }
            if (colorIndex % textureWidth == textureWidth - 1 || textureWidth == 1)
            {
                colorData[tempXIndex + 1 + 1 + (tempYIndex + 1) * ATLAS_SIZE] = color;
            }
        }

        private void AddCompiledTexture(
            List<CompiledTexture> compiledTextures, Texture texture, int atlasIndex, int xuv, int yuv,
            int currentFrame, bool isAnimated, ref CompiledTexture compiledTexture)
        {
            if (!isAnimated)
            {
                compiledTextures.Add(
                    new CompiledTexture(CompiledGameBundle, texture.Name, atlasIndex, xuv, yuv, texture.Width, texture.Height, 
                        isTransparent: texture.IsTransparent, isAnimated: texture.IsAnimated));
                _textureNames.Add(texture.Name);
            }
            else
            {
                if (currentFrame == 0)
                {
                    compiledTexture = new CompiledTexture(CompiledGameBundle, texture.Name, atlasIndex, xuv, yuv,
                                                          texture.Width, texture.Height, isAnimated: texture.IsAnimated);
                    compiledTextures.Add(compiledTexture);
                    _textureNames.Add(texture.Name);
                }

                compiledTexture.FrameAtlasIndices.Add(atlasIndex);
                compiledTexture.FrameXUVs.Add(xuv);
                compiledTexture.FrameYUVs.Add(yuv);
            }
        }

        private void CompileBlocks()
        {
            foreach (Block block in WorldSettings.Blocks)
            {
                CompileBlock(block);
            }

            if (WorldSettings.Blocks.Count == 0 && !CompileForObjectCreation)
            {
                CompiledGameBundle.Blocks.Add(new CompiledBlock(
                    CompiledGameBundle, CompiledBlock.WHITE_CUBE, CompiledTexture.WHITE_TEXTURE_NAME, isDestroyable: false, isSystem: true));
            }

            AddSystemBlocks();
            CompiledGameBundle.RemapBlocks();
        }

        private void CompileBlock(Block block)
        {
            CompiledGameBundle.Blocks.Add(new CompiledBlock(CompiledGameBundle, block));
        }

        private void AddSystemBlocks()
        {
            if (CompileForObjectCreation)
            {
                CompiledGameBundle.Blocks.Add(new CompiledBlock(
                    CompiledGameBundle, CompiledBlock.WHITE_CUBE, CompiledTexture.WHITE_TEXTURE_NAME, isDestroyable: false, isSystem: true));
            }

            CompiledGameBundle.Blocks.Add(new CompiledBlock(CompiledGameBundle, CompiledBlock.WAIT_CUBE, CompiledTexture.WAIT_TEXTURE_NAME,
                isDestroyable: false, isSystem: true));
        }

        private void CompileObjects()
        {
            foreach (GameObject gameObject in WorldSettings.GameObjects)
            {
                CompiledGameBundle.GameObjects.Add(new CompiledGameObject(CompiledGameBundle, gameObject));
            }
        }

        private void CompileCharacters()
        {
            foreach (Character character in WorldSettings.Characters)
            {
                CompiledGameBundle.Characters.Add(new CompiledCharacter(CompiledGameBundle, character));
            }
        }

        private void CheckPlayerCharacter()
        {
            bool playerCharacterExists = WorldSettings.Characters.Any(character => character.IsPlayerCharacter);

            if (!playerCharacterExists)
            {
                Character.CreateDefaultPlayer(WorldSettings.Name, content, graphics);
            }
        }

        private void CompileItems()
        {
            Item.CreateDefaultItem(WorldSettings.Name);
            foreach (Item item in WorldSettings.Items)
            {
                CompiledGameBundle.Items.Add(new CompiledItem(CompiledGameBundle, item));
            }
        }

        private void CompileRecipes()
        {
            foreach (Recipe recipe in WorldSettings.Recipes)
            {
                CompiledGameBundle.Recipes.Add(new CompiledRecipe(CompiledGameBundle, recipe));
            }
        }

        private void CompileNoises()
        {
            foreach (Noise noise in WorldSettings.Noises)
            {
                CompiledGameBundle.Noises.Add(new CompiledNoise(/*CompiledGameBundle,*/ noise));
            }
        }

        private void CompileRules()
        {
            foreach (Rule rule in WorldSettings.Rules)
            {
                var errors = new List<string>();
                bool condition = rule.CheckCondition(WorldSettings, ref errors);

                if (!condition)
                {
                    throw new GameBundleCompilerException(Rule.ErrorMessage(errors));
                }

                CompiledGameBundle.Rules.Add(WorldSettings.SubrulePriorities[rule.Guid], new CompiledRule(CompiledGameBundle, rule));
            }

            if (WorldSettings.Rules.Count == 0 && !CompileForObjectCreation)
            {
                CompiledGameBundle.Rules.Add(0, new CompiledRule
                {
                    BlockName = CompiledGameBundle.Blocks[0].Name,
                    ConditionText = "Height < 0",
                    ActionType = RuleActionsEnum.PlaceBlock,
                    Subrules = new Dictionary<int, CompiledRule>(),
                    Guid = Guid.NewGuid()
                });
            }
        }

        private void CompileCharacterAttributes()
        {
            Texture2D atlas = CompileAttributesIconAtlas(WorldSettings.CharacterAttributes);
            CompiledGameBundle.CharacterAttributeIconsAtlas = atlas;

            for (int i = 0; i < WorldSettings.CharacterAttributes.Count; i++)
            {
                CharacterAttribute characterAttribute = WorldSettings.CharacterAttributes[i];
                CompiledGameBundle.CharacterAttributes.Add(
                    new CompiledCharacterAttribute(characterAttribute)
                    {
                        FullTextureIndex = i * 2 // *2 because of half value icon
                    }
                );
            }
        }

        private void CopyDefaultSounds()
        {
            SoundLoader.CopySounds(CompiledGameBundle);
        }

        private Texture2D CompileAttributesIconAtlas(List<CharacterAttribute> characterAttributes)
        {
            var colorData =
                new Color[
                    CharacterAttribute.MAX_ATTRIBUTES * CharacterAttribute.ICON_PIXEL_SIZE *
                    CharacterAttribute.ICON_PIXEL_SIZE * 2];

            int atlasPower = (int)Math.Sqrt(CharacterAttribute.MAX_ATTRIBUTES * 2);

            var atlas = new Texture2D(
                graphics, atlasPower * CharacterAttribute.ICON_PIXEL_SIZE, atlasPower * CharacterAttribute.ICON_PIXEL_SIZE);

            int x = 0;
            int y = 0;

            foreach (CharacterAttribute characterAttribute in characterAttributes)
            {
                ProcessIcon(characterAttribute.IconFull, colorData, x, y, atlasPower);
                x++;
                ProcessIcon(characterAttribute.IconHalf, colorData, x, y, atlasPower);
                x++;

                if (x % atlasPower == 0)
                {
                    y++;
                    x = 0;
                }
            }

            atlas.SetData(colorData);

            SaveIconAtlas(atlas);

            return atlas;
        }

        private static void ProcessIcon(Color[] colors, Color[] newColors, int x, int y, int atlasPower)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                int localX = i % CharacterAttribute.ICON_PIXEL_SIZE;
                int localY = i / CharacterAttribute.ICON_PIXEL_SIZE;
                Color color = colors[i];
                int currentIndex = (y * CharacterAttribute.ICON_PIXEL_SIZE + localY) * CharacterAttribute.ICON_PIXEL_SIZE * atlasPower +
                                   x * CharacterAttribute.ICON_PIXEL_SIZE + localX;
                newColors[currentIndex] = color;
            }
        }
    }

    public class GameBundleCompilerException : Exception
    {
        public GameBundleCompilerException()
        {
        }

        public GameBundleCompilerException(string message)
            : base(message)
        {
        }

        public GameBundleCompilerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected GameBundleCompilerException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}