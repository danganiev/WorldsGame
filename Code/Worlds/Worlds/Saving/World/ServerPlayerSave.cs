using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Items.Inventory;

namespace WorldsGame.Saving.World
{
    public class ServerPlayerSave : ISaveDataSerializable<ServerPlayerSave>
    {
        public string FileName { get { return FullName + ".sav"; } }

        public string ContainerName { get { return StaticContainerName; } }

        public static string StaticContainerName { get { return "ServerPlayers"; } }

        public string Name { get; set; }

        public string IP { get; set; }

        public string FullName
        {
            get
            {
                return Name + IP;
            }
        }

        public Vector3 Position { get; set; }

        public float CameraLeftRightRotation { get; set; }

        public float CameraUpDownRotation { get; set; }

        public List<InventoryItem> Inventory { get; set; }

        public static SaverHelper<ServerPlayerSave> StaticSaverHelper()
        {
            return new SaverHelper<ServerPlayerSave>(StaticContainerName);
        }

        public SaverHelper<ServerPlayerSave> SaverHelper()
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
    }
}