#region License

//  TechCraft - http://techcraft.codeplex.com
//  This source code is offered under the Microsoft Public License (Ms-PL) which is outlined as follows:

//  Microsoft Public License (Ms-PL)
//  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//  1. Definitions
//  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
//  A "contribution" is the original software, or any additions or changes to the software.
//  A "contributor" is any person that distributes its contribution under this license.
//  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

//  2. Grant of Rights
//  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//  3. Conditions and Limitations
//  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
//  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
//  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
//  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

#endregion License

using WorldsGame.Models;
using WorldsGame.Models.Terrain;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Terrain.Blocks.Types;

namespace WorldsGame.Terrain.Types
{
    internal class ObjectGeneratorTerrain : ChunkGenerator
    {
        protected const int SAMPLE_RATE_3_D_HOR = 4;
        protected const int SAMPLE_RATE_3_D_VERT = 8;

        public ObjectGeneratorTerrain(CompiledGameBundle bundle)
            : base(bundle)
        {
        }

        internal World World { get; private set; }

        internal override void Generate(Chunk chunk)
        {
            Chunkie = chunk;
            World = Chunkie.World;
            GenerateTerrain();
        }

        internal override void ClearAfterGenerating()
        {
            Chunkie = null;
            EverythingGenerated = true;
        }

        protected virtual void GenerateTerrain()
        {
            for (byte x = 0; x < Chunk.SIZE.X; x++)
            {
                for (byte z = 0; z < Chunk.SIZE.Z; z++)
                {
                    BlockType blockType = BlockTypeHelper.AIR_BLOCK_TYPE;

                    for (int y = Chunk.MAX_VECTOR.Y; y >= 0; y--)
                    {
                        if (Chunkie.Index.Y == -1 && y <= 64)
                        {
                            blockType = BlockTypeHelper.SystemBlockTypes[CompiledBlock.WHITE_CUBE];
                        }
                        Chunkie.SetBlock(x, y, z, blockType);
                    }
                }
            }
        }

        public override void Dispose()
        {
        }
    }
}