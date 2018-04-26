using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Playing.Players;
using WorldsGame.Saving;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.Renderers
{
    internal class PlayerHUDInventoryRenderer
    {
        internal const int CELL_SIZE_IN_PIXELS = 50;
        private const int TEXTURE_SIZE_IN_PIXELS = 32;
        private const int CELL_TEXTURE_DIFF = (CELL_SIZE_IN_PIXELS - TEXTURE_SIZE_IN_PIXELS) / 2;
        internal const int INVENTORY_CELL_AMOUNT = 10;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Player _player;
        
        private Texture2D _inventoryAtlas;

        private const int _unselectedCellCoordinatesTopLeftX = 45;
        private const int _unselectedCellCoordinatesTopLeftY = 1;

        private const int _selectedCellCoordinatesTopLeftX = 78;
        private const int _selectedCellCoordinatesTopLeftY = 1;

        private int _xDrawStartPoint;
        private int _yDrawStartPoint;

        private readonly List<Texture2D> _icons;
        private readonly List<string> _quantities;
        private SpriteFont _font;

        internal PlayerHUDInventoryRenderer(GraphicsDevice graphicsDevice, World world)
        {
            _graphicsDevice = graphicsDevice;            
            _player = world.ClientPlayer;
            _icons = new List<Texture2D>();
            _quantities = new List<string>();
        }

        public void Initialize()
        {
            // Unneccessary multiplication and than division by 2 for clarity!
            _xDrawStartPoint = _graphicsDevice.Viewport.Width / 2 - CELL_SIZE_IN_PIXELS * INVENTORY_CELL_AMOUNT / 2;
            _yDrawStartPoint = _graphicsDevice.Viewport.Height - CELL_SIZE_IN_PIXELS;            

            for (int i = 0; i < INVENTORY_CELL_AMOUNT; i++)
            {
                _icons.Add(new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE));
                _quantities.Add("");
            }

            Messenger.On<int>("PlayerInventoryUpdate", OnPlayerInventoryUpdate);

            for (int slot = 0; slot < 10; slot++)
            {
                OnPlayerInventoryUpdate(slot);
            }
        }

        private void OnPlayerInventoryUpdate(int itemSlot)
        {
            if (itemSlot > INVENTORY_CELL_AMOUNT)
            {
                return;
            }

            InventoryItem inventoryItem = _player.Inventory.Items[itemSlot];

            if (inventoryItem != null)
            {
                CompiledItem item = ItemHelper.Get(inventoryItem);

                _icons[itemSlot].SetData(item.IconColors);
                if (inventoryItem.Quantity > 1)
                {
                    _quantities[itemSlot] = inventoryItem.Quantity.ToString();
                }
                else
                {
                    _quantities[itemSlot] = "";
                }
            }
            else
            {
                _icons[itemSlot].SetData(Icon.EMPTY_COLORS);
                _quantities[itemSlot] = "";
            }
        }

        public void LoadContent(ContentManager content)
        {
            _inventoryAtlas = content.Load<Texture2D>("Skin\\Suave\\Inventory\\Inventory");
            _font = content.Load<SpriteFont>("Skin//Suave//SuaveDefaultFont");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < INVENTORY_CELL_AMOUNT; i++)
            {
                bool isSelected = i == _player.Inventory.SelectedSlot;

                spriteBatch.Draw(
                    _inventoryAtlas,
                    new Rectangle(_xDrawStartPoint + CELL_SIZE_IN_PIXELS * i, _yDrawStartPoint, CELL_SIZE_IN_PIXELS, CELL_SIZE_IN_PIXELS),
                    new Rectangle(
                        isSelected ? _selectedCellCoordinatesTopLeftX : _unselectedCellCoordinatesTopLeftX,
                        isSelected ? _selectedCellCoordinatesTopLeftY : _unselectedCellCoordinatesTopLeftY,
                        TEXTURE_SIZE_IN_PIXELS, TEXTURE_SIZE_IN_PIXELS),
                    Color.White
                );

                spriteBatch.Draw(_icons[i],
                    new Rectangle(_xDrawStartPoint + CELL_SIZE_IN_PIXELS * i + CELL_TEXTURE_DIFF, _yDrawStartPoint + CELL_TEXTURE_DIFF, TEXTURE_SIZE_IN_PIXELS, TEXTURE_SIZE_IN_PIXELS),
                    null, Color.White);

                spriteBatch.DrawString(_font, _quantities[i], new Vector2(_xDrawStartPoint + CELL_SIZE_IN_PIXELS * i + 6, _yDrawStartPoint + 30), Color.Black);
            }
        }
    }
}