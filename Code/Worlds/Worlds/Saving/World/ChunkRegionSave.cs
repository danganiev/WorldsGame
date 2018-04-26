using System;
using System.Collections.Generic;
using System.IO;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Terrain.Chunks;

using WorldsLib;

namespace WorldsGame.Saving.World
{
    [Serializable]
    public class ChunkRegionSave : ISaveDataSerializable<ChunkRegionSave>
    {
        public string FileName { get { return Name + ".rgn"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "ChunkRegions"; } }

        public string CompiledGameBundleName { get; set; }

        // Saving 16*4*16 chunks, 9 regions total at start
        // R R R
        // R R R
        // R R R
        public Vector3i RegionIndex { get; set; }

        public string Name { get { return NameByIndex(RegionIndex); } }

        //NOTE: Let's say we map every block name to int in CompiledGameBundleSave
        public Dictionary<Vector3i, int[]> Chunks { get; set; }

        // Entities are just list of json strings. Type is the type of the template class.
        public Dictionary<Vector3i, List<Tuple<Type, string>>> Entities {get; set;}

        public ChunkRegionSave()
        {
            Chunks = new Dictionary<Vector3i, int[]>();
            Entities = new Dictionary<Vector3i, List<Tuple<Type, string>>>();
        }

        public static string NameByIndex(Vector3i regionIndex)
        {
            return string.Format("{0}.{1}.{2}", regionIndex.X, regionIndex.Y, regionIndex.Z);
        }

        public static string FileNameByIndex(Vector3i regionIndex)
        {
            return string.Format("{0}.rgn", NameByIndex(regionIndex));
        }

        public static SaverHelper<ChunkRegionSave> SaverHelper(string name)
        {
            return new SaverHelper<ChunkRegionSave>(StaticContainerName) { DirectoryRelativePath = name };
        }

        public SaverHelper<ChunkRegionSave> SaverHelper()
        {
            return SaverHelper(CompiledGameBundleName);
        }

        public void Save()
        {
            try
            {
                SaverHelper().Save(this);
            }
            catch (IOException)
            {
                // Probably region is already being saved
            }
            catch (UnauthorizedAccessException)
            {
                // TODO: THIS PROBABLY BREAKS SAVING, SO I NEED TO FIX IT
                // Haven't figured out source of the problem yet.
            }
        }

        internal ChunkRegion ToChunkRegion(Terrain.World world, CompiledGameBundle bundle)
        {
            return new ChunkRegion(world, RegionIndex, bundle, this);
        }

        public void Delete()
        {
            SaverHelper().Delete(Name);
        }
    }
}