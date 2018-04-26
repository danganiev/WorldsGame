using System;

using Microsoft.Xna.Framework.Graphics;

namespace WorldsGame.Models
{
    internal enum ChunkBufferType
    {
        Opaque,
        Custom,
        Transparent,
        Animated
    }

    internal class ChunkBlocksGraphicsRepresentation : IDisposable
    {
        internal VertexBuffer VertexBuffer { get; set; }
        internal IndexBuffer IndexBuffer { get; set; }

        internal VertexBuffer TransparentVertexBuffer { get; set; }
        internal IndexBuffer TransparentIndexBuffer { get; set; }

        // NOTE: I might want to cut out transparent/custom and animated buffers to 1 per world (depends on if it's ok to rebuild a whole buffer on a change of 1 block)
        // If multiple buffers will perform well though then it doesn't matter

        internal VertexBuffer CustomVertexBuffer { get; set; }
        internal IndexBuffer CustomIndexBuffer { get; set; }

        internal VertexBuffer AnimatedVertexBuffer { get; set; }
        internal IndexBuffer AnimatedIndexBuffer { get; set; }

        public void Dispose()
        {
            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
            }            

            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
            }

            if (TransparentVertexBuffer != null)
            {
                TransparentVertexBuffer.Dispose();
            }

            if (TransparentIndexBuffer != null)
            {
                TransparentIndexBuffer.Dispose();
            }

            if (CustomVertexBuffer != null)
            {
                CustomVertexBuffer.Dispose();
            }

            if (CustomIndexBuffer != null)
            {
                CustomIndexBuffer.Dispose();
            }

            if (AnimatedVertexBuffer != null)
            {
                AnimatedVertexBuffer.Dispose();
            }

            if (AnimatedIndexBuffer != null)
            {
                AnimatedIndexBuffer.Dispose();
            }
        }
    }
}