using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldsGame.Playing.DataClasses
{
    public class TextureAtlas
    {
        public const string CONTAINER_NAME = "Atlases";

        public int SizeInPixels { get; private set; }

        public Texture2D Texture { get; private set; }

        public int AtlasIndex { get; private set; }

        public TextureAtlas(int size, Texture2D texture, int atlasIndex)
        {
            AtlasIndex = atlasIndex;
            SizeInPixels = size;
            Texture = texture;
        }

        //        public Color[] GetPartialTexture(int index)
        //        {
        //            int width = index % SizeInPixels;
        //            int height = index / SizeInPixels;
        //
        //            var textureData = new Color[CompiledTexture.Width * CompiledTexture.Height];
        //
        //            Texture.GetData(0,
        //                new Rectangle(width * CompiledTexture.Width,
        //                    height * CompiledTexture.Height,
        //                    CompiledTexture.Width, CompiledTexture.Height),
        //                textureData,
        //                0,
        //                CompiledTexture.Width * CompiledTexture.Height);
        //
        //            return textureData;
        //        }
    }
}