using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.VertexTypes;

namespace WorldsGame.Playing.Entities
{
    internal class CustomModelBuffersComponent : IEntityComponent
    {
        // Dict key is for atlases, list is for cuboids
        internal Dictionary<int, List<VertexBuffer>> VertexBuffers { get; set; }
        
        internal Dictionary<int, List<IndexBuffer>> IndexBuffers { get; set; }

        internal CustomModelBuffersComponent()
        {
            VertexBuffers = new Dictionary<int, List<VertexBuffer>>();
            IndexBuffers = new Dictionary<int, List<IndexBuffer>>();
        }

        internal static void CreateBuffers(EntityWorld entityWorld, CustomModelBuffersComponent buffersComponent, ICustomModelHolder customModelHolder)
        {
            Dictionary<int, List<CustomEntityPart>> cuboids = customModelHolder.Cuboids;

            // 1st dimension - atlas index, 2nd - parts per cuboid, 3rd - vertice list
            var vertexListDictionary = new Dictionary<int, List<List<VertexFloatPositionTextureLight>>>();
            var indexListDictionary = new Dictionary<int, List<List<short>>>();
            var vertexCountDict = new Dictionary<int, List<int>>();

            foreach (int atlasIndex in entityWorld.World.CompiledGameBundle.TextureAtlases.Keys)
            {
                if (atlasIndex == -1)
                {
                    continue;
                }

                vertexListDictionary.Add(atlasIndex, new List<List<VertexFloatPositionTextureLight>>());
                indexListDictionary.Add(atlasIndex, new List<List<short>>());
                vertexCountDict.Add(atlasIndex, new List<int>());

                buffersComponent.VertexBuffers[atlasIndex] = new List<VertexBuffer>();
                buffersComponent.IndexBuffers[atlasIndex] = new List<IndexBuffer>();

                foreach (KeyValuePair<int, List<CustomEntityPart>> cuboid in cuboids)
                {
                    vertexListDictionary[atlasIndex].Add(new List<VertexFloatPositionTextureLight>());
                    indexListDictionary[atlasIndex].Add(new List<short>());
                    vertexCountDict[atlasIndex].Add(0);

                    // Planes aka entity parts
                    for (int i = 0; i < cuboid.Value.Count; i++)
                    {
                        CustomEntityPart part = cuboid.Value[i];
                        //                        int atlasIndex = part.AtlasIndex;

                        for (int j = 0; j < part.VertexList.Length; j++)
                        {
                            Vector3 vertex = part.VertexList[j];

                            vertexListDictionary[atlasIndex][cuboid.Key].Add(
                                new VertexFloatPositionTextureLight(
                                    new Vector4(vertex.X, vertex.Y, vertex.Z, 1),
                                    new NormalizedShort2(part.UVMappings[j]), Color.Black));

                            AddIndices(CustomEntityPart.INDICES_LIST, indexListDictionary[atlasIndex][cuboid.Key],
                                   vertexCountDict[atlasIndex][cuboid.Key]);
                        }

                        vertexCountDict[atlasIndex][cuboid.Key] += part.VertexList.Length;
                    }

                    if (vertexCountDict[atlasIndex][cuboid.Key] == 0)
                    {
                        continue;
                    }

                    var vertexBuffer = new VertexBuffer(
                        entityWorld.World.Graphics, VertexFloatPositionTextureLight.VertexDeclaration,
                        vertexCountDict[atlasIndex][cuboid.Key],
                        BufferUsage.WriteOnly);

                    vertexBuffer.SetData(vertexListDictionary[atlasIndex][cuboid.Key].ToArray());
                    vertexListDictionary[atlasIndex][cuboid.Key].Clear();

                    buffersComponent.VertexBuffers[atlasIndex].Add(vertexBuffer);

                    var indexArray = indexListDictionary[atlasIndex][cuboid.Key].ToArray();

                    var indexBuffer = new IndexBuffer(
                        entityWorld.World.Graphics, IndexElementSize.SixteenBits, indexArray.Length,
                        BufferUsage.WriteOnly);
                    indexBuffer.SetData(indexArray);

                    buffersComponent.IndexBuffers[atlasIndex].Add(indexBuffer);

                    indexListDictionary[atlasIndex][cuboid.Key].Clear();
                }
            }

            vertexListDictionary.Clear();
            indexListDictionary.Clear();
            vertexCountDict.Clear();
        }

        private static void AddIndices(IList<short> indices, List<short> indexListDictionary, int vertexCount)
        {
            var newIndices = new short[indices.Count];

            for (int i = 0; i < indices.Count; i++)
            {
                var index = indices[i];

                newIndices[i] = (short)(vertexCount + index);
            }

            // Sync problem here
            indexListDictionary.AddRange(newIndices);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<int, List<VertexBuffer>> vertexBuffers in VertexBuffers)
            {
                foreach (VertexBuffer buffer in vertexBuffers.Value)
                {
                    if (!buffer.IsDisposed)
                    {
                        buffer.Dispose();
                    }
                }
            }
            foreach (KeyValuePair<int, List<IndexBuffer>> indexBuffers in IndexBuffers)
            {
                foreach (IndexBuffer buffer in indexBuffers.Value)
                {
                    if (!buffer.IsDisposed)
                    {
                        buffer.Dispose();
                    }
                }
            }
        }
    }
}