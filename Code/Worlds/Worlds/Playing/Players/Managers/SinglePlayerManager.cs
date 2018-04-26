using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Players
{
    internal class SinglePlayerManager : IPlayerManager
    {
        public Player ClientPlayer { get; internal set; }

        public World World { get; private set; }

        public void Initialize(World world)
        {
            World = world;

            Messenger.On<Player>("PlayerDied", OnPlayerDied);
        }

        private void OnPlayerDied(Player player)
        {
            player.IsDead = true;
        }

        public void Update(GameTime gameTime)
        {
            if (ClientPlayer.IsDead)
            {
                SpawnMainPlayer(new SpawnPlayerParams());                
            }

            ClientPlayer.Update(gameTime);            
        }

        public void UpdateStep(GameTime gameTime)
        {
        }

        public void SpawnMainPlayer(SpawnPlayerParams paramz)
        {
            Vector3 position = paramz.Position;
            float cameraUpDownRotation = paramz.CameraUpDownRotation;
            float cameraLeftRightRotation = paramz.CameraLeftRightRotation;

            if (ClientPlayer == null)
            {
                bool isPlayerPositionFound = position != Vector3.Zero;

                ClientPlayer = new Player(World, position)
                {
                    UpDownRotation = cameraUpDownRotation,
                    LeftRightRotation = cameraLeftRightRotation,
                    IsPlayerPositionFound = isPlayerPositionFound
                };

                if (World.WorldType == WorldType.ObjectCreationWorld)
                {
                    ClientPlayer.UseSelectionBlockWithAir = true;
                }

                ClientPlayer.Initialize();
                ClientPlayer.InitializeInventory(paramz.Inventory, paramz.InventorySelectedSlot);
            }
            else
            {
                ClientPlayer.Respawn();
            }
            ClientPlayer.IsDead = false;
        }

        public void Dispose()
        {
            Messenger.Off<Player>("PlayerDied", OnPlayerDied);

            ClientPlayer.Dispose();
        }
    }
}