using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.Saving.World
{
    [Serializable]
    public class WorldSave : ISaveDataSerializable<WorldSave>
    {
        public const string OBJECT_CREATION_WORLD_NAME = "System__ObjectCreation";

        public string FileName { get { return FullName + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "Worlds"; } }

        public string Name { get; set; }

        public string Guid { get; set; }

        public int Seed { get; set; }

        public string FullName
        {
            get
            {
                if (Name != OBJECT_CREATION_WORLD_NAME)
                {
                    return Name + Guid;
                }

                return OBJECT_CREATION_WORLD_NAME;
            }
        }

        #region SinglePlayer

        // Player stuff, we don't fill it on server
        public Vector3 PlayerPosition { get; set; }

        public float CameraLeftRightRotation { get; set; }

        public float CameraUpDownRotation { get; set; }

        public List<InventoryItem> PlayerInventory { get; set; }

        public byte PlayerInventorySelectedSlot { get; set; }

        #endregion SinglePlayer

        public int NextLiquidPoolId { get; set; }

        public static SaverHelper<WorldSave> StaticSaverHelper()
        {
            return new SaverHelper<WorldSave>(StaticContainerName);
        }

        public SaverHelper<WorldSave> SaverHelper()
        {
            return StaticSaverHelper();
        }

        public void Save()
        {
            SaverHelper().Save(this);
        }

        public void Delete()
        {
            SaverHelper().Delete(Name);
        }

        public static void Delete(string name)
        {
            StaticSaverHelper().Delete(name);
        }

        public override string ToString()
        {
            return Name;
        }

        internal static void DeleteWorldSave(string name)
        {
            CompiledGameBundleSave.Delete(name);
            CompiledGameBundleSave.SaverHelper(name).Clear();
            ChunkRegionSave.SaverHelper(name).Clear();
            Delete(name);
        }
    }
}