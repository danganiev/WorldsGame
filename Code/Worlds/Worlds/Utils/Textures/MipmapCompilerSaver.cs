using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;
using WorldsGame.Utils;

namespace WorldsGame.Playing.DataClasses
{
    public class MipmapCompilerSaver
    {
        private readonly int _mipmapLevel;
        private readonly int _atlasIndex;

        public Color[] Texture { get; set; }

        public Dictionary<int, Color[]> Mipmaps { get; set; }

        private class Mipmap
        {
            private int Width { get; set; }

            private int Height { get; set; }

            private Color[][] _colors;
            private readonly int _level;

            internal Color[] Colors
            {
                get
                {
                    var colors = new Color[Width * Height];

                    for (int j = 0; j < Height; j++)
                    {
                        for (int i = 0; i < Width; i++)
                        {
                            colors[j * Width + i] = _colors[j][i];
                        }
                    }

                    return colors;
                }
            }

            internal Mipmap(int width, int height, Color[] colors)
            {
                Width = width / 2;
                Height = height / 2;

                _level = 1;

                Color[][] newColors = PrepareFirstMipmapColors(width, height, colors);
                InitializeColors();

                BuildLameMipmap(this, colors: newColors);
            }

            internal Mipmap(Mipmap previousMipmap, int level)
            {
                _level = level;

                Width = Math.Max(previousMipmap.Width / 2, 1);
                Height = Math.Max(previousMipmap.Height / 2, 1);

                InitializeColors();

                BuildLameMipmap(this, source: previousMipmap);
            }

            private Color[][] PrepareFirstMipmapColors(int width, int height, Color[] atlasColors)
            {
                var colors = new Color[width][];

                for (int i = 0; i < height; i++)
                {
                    colors[i] = new Color[height];
                }

                for (int index = 0; index < atlasColors.Length; index++)
                {
                    int h = index / width;
                    int w = index - h * height;
                    colors[h][w] = atlasColors[index];
                }

                return colors;
            }

            private void InitializeColors()
            {
                _colors = new Color[Height][];

                for (int i = 0; i < Height; i++)
                {
                    _colors[i] = new Color[Width];
                }
            }

            // Builds a lamest and unprofessional mipmap, but it works
            private void BuildLameMipmap(Mipmap dest, Mipmap source = null, Color[][] colors = null)
            {
                Color[][] sourceColors = colors ?? source._colors;

                const int height = GameBundleCompiler.ATLAS_SIZE;
                const int width = GameBundleCompiler.ATLAS_SIZE;

                for (int h = 0; h < Math.Max(1, height >> _level); h++)
                {
                    int h0 = h << 1;
                    int h1 = Math.Min(h0 + 1, Math.Max(1, height >> (_level - 1)) - 1);
                    for (int w = 0; w < Math.Max(1, width >> _level); w++)
                    {
                        int w0 = w << 1;
                        int w1 = Math.Min(w0 + 1, Math.Max(1, width >> (_level - 1)) - 1);

                        int destRed = (sourceColors[h0][w0].R + sourceColors[h0][w1].R
                                       + sourceColors[h1][w0].R + sourceColors[h1][w1].R) / 4;
                        int destGreen = (sourceColors[h0][w0].G + sourceColors[h0][w1].G
                                         + sourceColors[h1][w0].G + sourceColors[h1][w1].G) / 4;
                        int destBlue = (sourceColors[h0][w0].B + sourceColors[h0][w1].B
                                        + sourceColors[h1][w0].B + sourceColors[h1][w1].B) / 4;
                        int destAlpha = (sourceColors[h0][w0].A + sourceColors[h0][w1].A
                                         + sourceColors[h1][w0].A + sourceColors[h1][w1].A) / 4;

                        dest._colors[h][w] = new Color(destRed, destGreen, destBlue, destAlpha);
                    }
                }
            }
        }

        public MipmapCompilerSaver(Color[] colors, int mipmapLevel, int atlasIndex)
        {
            Texture = colors;
            Mipmaps = new Dictionary<int, Color[]>();
            _mipmapLevel = mipmapLevel;
            _atlasIndex = atlasIndex;
        }

        public void CompileAndSave(string atlasName, GraphicsDevice graphics, BundleType bundleType = BundleType.Normal)
        {
            if (_mipmapLevel > 0)
            {
                BuildMipmaps(Texture);
                SaveMipmaps(atlasName, graphics, bundleType);
            }
        }

        private void BuildMipmaps(Color[] colors)
        {
            var previousMipmap = new Mipmap(GameBundleCompiler.ATLAS_SIZE, GameBundleCompiler.ATLAS_SIZE, colors);
            Mipmaps[1] = previousMipmap.Colors;

            if (_mipmapLevel > 1)
            {
                for (int i = 2; i < _mipmapLevel; i++)
                {
                    var newMipmap = new Mipmap(previousMipmap, i);
                    Mipmaps[i] = newMipmap.Colors;

                    previousMipmap = newMipmap;
                }
            }
        }

        private void SaveMipmaps(string atlasName, GraphicsDevice graphics, BundleType bundleType = BundleType.Normal)
        {
            SaverHelper<CompiledGameBundleSave> saverHelper = CompiledGameBundleSave.SaverHelper(atlasName, bundleType);

            for (int i = 1; i < _mipmapLevel; i++)
            {
                int width = Math.Max(GameBundleCompiler.ATLAS_SIZE >> i, 1);
                int height = Math.Max(GameBundleCompiler.ATLAS_SIZE >> i, 1);

                var mipmap = new Texture2D(graphics, width, height, mipMap: false, format: SurfaceFormat.Color);

                mipmap.SetData(Mipmaps[i]);

                saverHelper.SaveAtlas(CompiledGameBundle.TextureAtlasName(_atlasIndex, i), mipmap);

                mipmap.Dispose();
            }
        }
    }
}