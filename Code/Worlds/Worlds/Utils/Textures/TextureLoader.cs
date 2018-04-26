using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Utils.Textures
{
    internal class TextureLoader
    {
        private readonly Texture2D _textureAtlas;
        private readonly string _containerName;
        private readonly int _atlasIndex;
        private readonly GraphicsDevice _graphicsDevice;

        internal TextureLoader(Texture2D textureAtlas, string containerName, int atlasIndex, GraphicsDevice graphicsDevice)
        {
            _textureAtlas = textureAtlas;
            _containerName = containerName;
            _atlasIndex = atlasIndex;
            _graphicsDevice = graphicsDevice;
        }

        internal void Load(int levelCount, BundleType bundleType = BundleType.Normal)
        {
            var saverHelper = CompiledGameBundleSave.SaverHelper(_containerName, bundleType);

            // Try to load mipmap, if not exists, recreate them
            try
            {
                Texture2D firstMipmap = saverHelper.LoadAtlas(CompiledGameBundle.TextureAtlasName(_atlasIndex, 1),
                                                        _graphicsDevice);
                firstMipmap.Dispose();
            }
            catch (FileNotFoundException)
            {
                RecreateMipmaps(saverHelper, bundleType);
            }

            // Old mipmaps
            for (int i = 0; i < levelCount; i++)
            {
                FillLevel(saverHelper, i);
            }

            // New mipmaps
            // Some very VERY strange problems with those
            //            Fill(saverHelper, levelCount);

            //            FillNoMipmaps(saverHelper);
        }

        private void RecreateMipmaps(SaverHelper<CompiledGameBundleSave> saverHelper, BundleType bundleType)
        {
            using (Texture2D atlas = saverHelper.LoadAtlas(CompiledGameBundle.TextureAtlasName(_atlasIndex), _graphicsDevice))
            {
                var colors = new Color[GameBundleCompiler.ATLAS_SIZE * GameBundleCompiler.ATLAS_SIZE];
                atlas.GetData(colors);
                int levelCount = (int)Math.Round(Math.Log(GameBundleCompiler.ATLAS_SIZE, 2)) + 1;
                var mipmapCompiler = new MipmapCompilerSaver(colors, levelCount, _atlasIndex);
                mipmapCompiler.CompileAndSave(_containerName, _graphicsDevice, bundleType: bundleType);
            }
        }

        private void FillLevel(SaverHelper<CompiledGameBundleSave> saverHelper, int level)
        {
            Texture2D atlas = saverHelper.LoadAtlas(CompiledGameBundle.TextureAtlasName(_atlasIndex, level), _graphicsDevice);
            var atlasColors = new Color[(GameBundleCompiler.ATLAS_SIZE >> level) * (GameBundleCompiler.ATLAS_SIZE >> level)];
            atlas.GetData(atlasColors);

            _textureAtlas.SetData(level, null, atlasColors, 0, atlasColors.Length);

            atlas.Dispose();
        }

        private void FillNoMipmaps(SaverHelper<CompiledGameBundleSave> saverHelper)
        {
            Texture2D atlas = saverHelper.LoadAtlas(CompiledGameBundle.TextureAtlasName(_atlasIndex, 0), _graphicsDevice);
            var atlasColors = new Color[GameBundleCompiler.ATLAS_SIZE * GameBundleCompiler.ATLAS_SIZE];
            atlas.GetData(atlasColors);

            _textureAtlas.SetData(atlasColors);

            atlas.Dispose();
        }

        private void Fill(SaverHelper<CompiledGameBundleSave> saverHelper, int levelCount)
        {
            Texture2D atlas = saverHelper.LoadAtlas(CompiledGameBundle.TextureAtlasName(_atlasIndex, 0), _graphicsDevice);

            var rt = new RenderTarget2D(
                _graphicsDevice, GameBundleCompiler.ATLAS_SIZE, GameBundleCompiler.ATLAS_SIZE, true, SurfaceFormat.Color, DepthFormat.Depth16);

            _graphicsDevice.SetRenderTarget(rt);
            var spriteBatch = new SpriteBatch(_graphicsDevice);
            spriteBatch.Begin();
            spriteBatch.Draw(atlas, new Vector2(0, 0), Color.White);
            spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);

            for (int level = 0; level < levelCount; level++)
            {
                var atlasColors = new Color[(GameBundleCompiler.ATLAS_SIZE >> level) * (GameBundleCompiler.ATLAS_SIZE >> level)];

                rt.GetData(level, null, atlasColors, 0, atlasColors.Length);
                _textureAtlas.SetData(level, null, atlasColors, 0, atlasColors.Length);
            }

            rt.Dispose();
            spriteBatch.Dispose();
            atlas.Dispose();
        }
    }
}