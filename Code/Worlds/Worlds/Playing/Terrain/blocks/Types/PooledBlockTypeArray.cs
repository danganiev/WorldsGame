using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using WorldsGame.Models;
using WorldsGame.Utils;

namespace WorldsGame.Terrain.Blocks.Types
{
    public class PooledBlockTypeArray : IDisposable, IPoolable, IEnumerable<BlockType>
    {
        private readonly Pool<PooledBlockTypeArray> _pool;
        internal readonly BlockType[] Blocks;

        public PooledBlockTypeArray(Pool<PooledBlockTypeArray> pool)
        {
            if (pool == null)
                throw new ArgumentNullException("pool");

            _pool = pool;
            Blocks = new BlockType[Chunk.SIZE.X * Chunk.SIZE.Z * Chunk.SIZE.Y];
        }

        internal BlockType this[int key]
        {            
            get
            {
                return Blocks[key];
            }
            set
            {
                Blocks[key] = value;
            }
        }

        internal void SetBlocksFromKeys(int[] keys)
        {
            if (Blocks.Length == keys.Length)
            {
                for (int index = 0; index < keys.Length; index++)
                {
                    Blocks[index] = BlockTypeHelper.Get(keys[index]);
                }
            }
        }

        public void Clear()
        {
            
        }

        public IEnumerator<BlockType> GetEnumerator()
        {
            return ((IEnumerable<BlockType>) Blocks).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_pool.IsDisposed)
            {
                Array.Clear(Blocks, 0, Blocks.Length);
            }
            else
            {
                _pool.Release(this);
            }

        }
    }
}