using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace WorldsGame.Playing.VertexTypes
{
    [Serializable]
    // Vertex data for custom blocks
    public struct VertexFloatPositionTextureLight : IVertexType
    {
        public Vector4 Position { get; set; }

        public NormalizedShort2 TextureCoordinate { get; set; }

        public Color Light { get; set; }

        //http://xboxforums.create.msdn.com/forums/p/108260/638180.aspx
        public static readonly VertexElement[] VertexElements = new[]
        {
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*4, VertexElementFormat.NormalizedShort2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*4 + sizeof(uint), VertexElementFormat.Color, VertexElementUsage.Color, 1),
        };

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public VertexFloatPositionTextureLight(Vector4 position, NormalizedShort2 textureCoordinate, Color light)
            : this()
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
            Light = light;
        }

        public override String ToString()
        {
            return "(" + Position + "),(" + TextureCoordinate + ")";
        }

        public static int SizeInBytes { get { return sizeof(float) * 4 + sizeof(uint) + sizeof(uint); } }
    }
}