using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Terrain;

namespace WorldsGame.Playing.Players
{
    internal class SpawnPlayerParams
    {
        internal Vector3 Position { get; set; }

        internal float CameraUpDownRotation { get; set; }

        internal float CameraLeftRightRotation { get; set; }

        internal List<InventoryItem> Inventory { get; set; }

        internal byte InventorySelectedSlot { get; set; }

        internal SpawnPlayerParams()
        {
            CameraUpDownRotation = 0;
            CameraLeftRightRotation = -MathHelper.PiOver2;
            Position = Vector3.Zero;
            Inventory = new List<InventoryItem>();
            InventorySelectedSlot = 0;
        }
    }

    internal interface IPlayerManager : IDisposable
    {
        Player ClientPlayer { get; }

        World World { get; }

        void Initialize(World world);

        void Update(GameTime gameTime);

        // Used on server only
        void UpdateStep(GameTime gameTime);

        void SpawnMainPlayer(SpawnPlayerParams paramz);
    }
}