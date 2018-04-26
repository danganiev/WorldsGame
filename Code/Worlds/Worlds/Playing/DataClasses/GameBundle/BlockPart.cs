using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using WorldsGame.Utils;
using WorldsGame.View.Blocks;

namespace WorldsGame.Playing.DataClasses
{
    [Serializable]
    public class BlockPart
    {
        //As everything seems to be remained unchanged, I preferred to use arrays.

        public Vector3[] VertexList { get; private set; }

        public byte[] TextureEntranceIndexList { get; private set; }

        public short[] IndicesList { get; private set; }

        public CompiledTexture Texture { get; private set; }

        public Vector2[] UVMappings { get; private set; }

        public bool IsTransparent { get { return Texture.IsTransparent; } }

        public bool IsAnimated { get { return Texture.IsAnimated; } }

        public BlockPart(Vector3[] vertexAddList, byte[] textureEntranceIndexList, short[] indicesList, CompiledTexture texture, Vector2[] uvMappings)
        {
            VertexList = vertexAddList;
            //            TextureEntranceIndexList = textureEntranceIndexList;
            //            IndicesList = indicesList;
            Texture = texture;
            UVMappings = uvMappings;

            TextureEntranceIndexList = new byte[] { 0, 1, 2, 3 };
            IndicesList = new short[] { 0, 1, 2, 2, 1, 3 };
        }

        private static readonly HashSet<CubeFaceDirection> RDF_FACES = new HashSet<CubeFaceDirection>
        {
            CubeFaceDirection.Right,
            CubeFaceDirection.Down,
            CubeFaceDirection.Forward,
            CubeFaceDirection.Left,
            CubeFaceDirection.Back
        };

        private static readonly HashSet<CubeFaceDirection> LB_FACES = new HashSet<CubeFaceDirection>
        {
            CubeFaceDirection.Left,
            CubeFaceDirection.Back
        };

        internal static Vector2[] GetCubeUVMappingList(CompiledTexture texture, int textureAtlasSizeInPixels, CubeFaceDirection faceDir)
        {
            // Returns the relative (0,1) texture coordinates on the atlas
            float yOfs;
            float xOfs;
            //BUG: I think the bug is here
            /*float oneOverTextureAtlasSize = */
            GetXOfsYOfs(texture, textureAtlasSizeInPixels, out xOfs, out yOfs);

            //            float texel = (oneOverTextureAtlasSize / (Constants.TEXTURE_SIZE + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2));
            float texel = (1f / textureAtlasSizeInPixels);
            float oneOverTextureHeight = (float)(texture.Height + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2) / textureAtlasSizeInPixels;
            float oneOverTextureWidth = (float)(texture.Width + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2) / textureAtlasSizeInPixels;

            var UVList = new Vector2[4];

            //            if (RDF_FACES.Contains(faceDir))
            //            {
            //                UVList[0] = new Vector2(xOfs + texel, yOfs + texel); // 0,0
            //                UVList[1] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            //                UVList[2] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            //                UVList[3] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            //                UVList[4] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            //                UVList[5] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + oneOverTextureHeight - texel); // 1,1
            //            }
            //            else if (LB_FACES.Contains(faceDir))
            //            {
            //                //{ 0, 1, 5, 2 }
            //                UVList[0] = new Vector2(xOfs + texel, yOfs + texel); // 0,0
            //                UVList[1] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            //                UVList[2] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + oneOverTextureHeight - texel); // 1,1
            //                UVList[5] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            //            }
            //            else if (faceDir == CubeFaceDirection.Up)
            //            {
            //                UVList[0] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            //                UVList[1] = new Vector2(xOfs + texel, yOfs + texel); // 0,0
            //                UVList[2] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            //                UVList[3] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            //                UVList[4] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            //                UVList[5] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + oneOverTextureHeight - texel); // 1,1
            //            }

            UVList[0] = new Vector2(xOfs + texel, yOfs + texel); // 0,0
            UVList[1] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            UVList[2] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            UVList[3] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + oneOverTextureHeight - texel); // 1,1

            return UVList;
        }

        internal static Vector2[] GetCustomUVMappingList(CompiledTexture texture, int textureAtlasSizeInPixels)
        {
            float yOfs;
            float xOfs;

            //            float texel = (oneOverTextureAtlasSize / (Constants.TEXTURE_SIZE + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2));
            float texel = (1f / textureAtlasSizeInPixels);

            float oneOverTextureHeight = (float)(texture.Height + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2) / textureAtlasSizeInPixels;
            float oneOverTextureWidth = (float)(texture.Width + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2) / textureAtlasSizeInPixels;

            GetXOfsYOfs(texture, texel, out xOfs, out yOfs);

            var UVList = new Vector2[4];

            //            UVList[0] = new Vector2(xOfs + texel, yOfs + texel); // 0,0
            //            UVList[1] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            //            UVList[2] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            //            UVList[3] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + oneOverTextureHeight - texel); // 1,1

            UVList[0] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + texel); // 1,0
            UVList[1] = new Vector2(xOfs + oneOverTextureWidth - texel, yOfs + oneOverTextureHeight - texel); // 1,1
            UVList[2] = new Vector2(xOfs + texel, yOfs + oneOverTextureHeight - texel); // 0,1
            UVList[3] = new Vector2(xOfs + texel, yOfs + texel); // 0,0

            return UVList;
        }

        // Returns relative x and y on (0,1) coordinate system of the texture atlas, and 1/atlassize
        //        internal static float GetXOfsYOfs(int textureIndex, int textureAtlasSizeInPixels, out float xOfs, out float yOfs)
        //        {
        //            int textureAtlasSizeInTextureCount = textureAtlasSizeInPixels / (Constants.TEXTURE_SIZE + Constants.TEXTURE_MIPMAP_BORDER_SIZE * 2);
        //
        //            int x;
        //            int y;
        //            GetRelativeTextureCoordinates(textureIndex, textureAtlasSizeInTextureCount, out x, out y);
        //
        //            var oneOverTextureAtlasSize = GetOneOverTextureAtlasSize(textureAtlasSizeInPixels, x, y, out xOfs, out yOfs);
        //            return oneOverTextureAtlasSize;
        //        }

        // Returns relative x and y on (0,1) coordinate system of the texture atlas, and 1/atlassize
        internal static float GetXOfsYOfs(CompiledTexture texture, int textureAtlasSizeInPixels, out float xOfs, out float yOfs)
        {
            int x = texture.XUV;
            int y = texture.YUV;

            float oneOverTextureAtlasSize = 1f / textureAtlasSizeInPixels;

            yOfs = y * oneOverTextureAtlasSize;
            xOfs = x * oneOverTextureAtlasSize;

            return oneOverTextureAtlasSize;
        }

        internal static void GetXOfsYOfs(CompiledTexture texture, float texel, out float xOfs, out float yOfs)
        {
            int x = texture.XUV;
            int y = texture.YUV;

            yOfs = y * texel;
            xOfs = x * texel;
        }

        //        internal static float GetOneOverTextureAtlasSize(int textureAtlasSizeInPixels, int x, int y, out float xOfs, out float yOfs)
        //        {
        //            float oneOverTextureAtlasSize = 1f / textureAtlasSizeInPixels;
        //
        //            yOfs = y * oneOverTextureAtlasSize;
        //            xOfs = x * oneOverTextureAtlasSize;
        //            return oneOverTextureAtlasSize;
        //        }

        internal static void GetRelativeTextureCoordinates(int textureIndex, int textureAtlasSize, out int x, out int y)
        {
            y = textureIndex / textureAtlasSize;
            x = textureIndex % textureAtlasSize;
        }
    }
}