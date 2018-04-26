using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface;
using WorldsGame.Playing.Players;
using WorldsGame.Playing.Renderers;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Items.Inventory
{
    // Master class over every player inventory aspect (singleplayer)
    internal class SinglePlayerInventoryManager
    {
        private readonly WorldsGame _game;
        private readonly World _world;
        private readonly Screen _screen;
        private readonly GraphicsDevice _graphicsDevice;
        private PlayerInventoryRenderer _inventoryRenderer;
        private Player _player;

        private ClientPlayerActionManager _playerActionManager;

        internal ClientPlayerActionManager PlayerActionManager
        {
            get { return _playerActionManager; }
            set
            {
                _playerActionManager = value;

                if (value != null)
                {
                    _playerActionManager.OnPlayerToggledInventory += OnPlayerToggledInventory;
                    _playerActionManager.OnPlayerSelectedItem += OnPlayerSelectedItem;
                }
            }
        }

        private void OnPlayerSelectedItem(byte itemSlot)
        {
            if (!IsInventoryOpened)
            {
                _player.Inventory.SelectedSlot = itemSlot;                
            }
        }

        private void OnSelectedItemChange()
        {
            _player.ChangeItemInHand(_player.Inventory.SelectedItem);
        }

        internal bool IsInventoryOpened { get; private set; }

        internal event Action OnInventoryOpened;

        internal event Action OnInventoryClosed;

        internal SinglePlayerInventoryManager(WorldsGame game, World world, Screen screen)
        {
            _game = game;
            _world = world;
            _screen = screen;
            _player = world.ClientPlayer;
            _graphicsDevice = game.GraphicsDevice;

            _inventoryRenderer = new PlayerInventoryRenderer(_game, _world, _screen);

            _player.Inventory.OnSelectionChanged += OnSelectedItemChange;
        }

        internal void Start()
        {
            _game.IsMouseVisible = true;

            Messenger.Invoke("ToggleMouseCentering");

            IsInventoryOpened = true;
            _inventoryRenderer.Start();
            OnInventoryOpened();
        }

        internal void Stop()
        {
            Messenger.Invoke("ToggleMouseCentering");

            _game.IsMouseVisible = false;

            IsInventoryOpened = false;
            _inventoryRenderer.Stop();
            OnInventoryClosed();
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsInventoryOpened)
            {
                _inventoryRenderer.Draw(gameTime, spriteBatch);
            }
        }

        internal void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsInventoryOpened)
            {
                _inventoryRenderer.DrawAfterGUI(gameTime, spriteBatch);
            }
        }

        private void OnPlayerToggledInventory()
        {
            if (IsInventoryOpened)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }

        internal void LoadContent(ContentManager content)
        {
            _inventoryRenderer.LoadContent(content);
        }

        internal void CloseInventory()
        {
            if (IsInventoryOpened)
            {
                OnPlayerToggledInventory();
            }
        }
    }
}