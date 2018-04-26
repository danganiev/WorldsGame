using System;
using System.Collections.Generic;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class CompiledTexture
    {
        [NonSerialized]
        private readonly CompiledGameBundle _gameBundle;

        public const string WAIT_TEXTURE_NAME = "System__WaitTexture";
        public const string WHITE_TEXTURE_NAME = "System__WhiteTexture";
        internal const int ANIMATED_FRAME_COUNT = 4;
        internal const int MAX_TEXTURE_SIZE = 32;

        //        public int Index { get; set; }
        // X coordinate of the texture
        public int Width { get; set; }

        public int Height { get; set; }

        // X coordinate in atlas
        public int XUV { get; set; }

        // Y coordinate in atlas
        public int YUV { get; set; }

        //Texture name. Should be unique per bundle.
        public string Name { get; set; }

        //Index of the atlas in the bundle
        public int AtlasIndex { get; set; }

        public bool IsAnimated { get; set; }
        public bool IsTransparent { get; set; }

        public List<int> FrameAtlasIndices { get; set; }

        public List<int> FrameXUVs { get; set; }

        public List<int> FrameYUVs { get; set; }

        // For serialization purposes only, don't use it
        public CompiledTexture()
        {
        }

        public CompiledTexture(CompiledGameBundle gameBundle, string name, int atlasIndex,
            int xuv, int yuv, int width = 32, int height = 32, bool isTransparent = false, bool isAnimated = false)
        {
            //            Index = index;
            XUV = xuv;
            YUV = yuv;
            Name = name;
            AtlasIndex = atlasIndex;
            _gameBundle = gameBundle;
            Width = width;
            Height = height;
            IsTransparent = isTransparent;
            IsAnimated = isAnimated;

            if (IsAnimated)
            {
                FrameAtlasIndices = new List<int>();
                FrameXUVs = new List<int>();
                FrameYUVs = new List<int>();
            }
        }

        //        public void GetTextureUVCoordinates(out float xOfs, out float yOfs)
        //        {
        //            BlockPart.GetXOfsYOfs(this, _gameBundle.GetTextureAtlas(AtlasIndex).SizeInPixels, out xOfs, out yOfs);
        //        }

        public float OneOverAtlasSize()
        {
            return 1f / _gameBundle.GetTextureAtlas(AtlasIndex).SizeInPixels;
        }
    }
}