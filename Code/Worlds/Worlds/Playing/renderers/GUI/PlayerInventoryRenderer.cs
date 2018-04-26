using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Graphics.SpecialEffects.Masks;
using Nuclex.UserInterface;
using WorldsGame.GUI.Inventory;
using WorldsGame.Playing.Players;
using WorldsGame.Terrain;

namespace WorldsGame.Playing.Renderers
{
    // Full inventory
    internal class PlayerInventoryRenderer : IDisposable
    {
        private readonly WorldsGame _game;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly Player _player;

        private World _world;

        private readonly Screen _screen;

        private ColorScreenMask _colorScreenMask;

        private InventoryGUI _inventoryGUI;

        internal PlayerInventoryRenderer(WorldsGame game, World world, Screen screen)
        {
            _game = game;
            _graphicsDevice = game.GraphicsDevice;
            _world = world;
            _screen = screen;
            _player = world.ClientPlayer;

            _colorScreenMask = ColorScreenMask.Create(_graphicsDevice);

            Color color = Color.Black;
            color.A = 150;
            _colorScreenMask.Color = color;

            _inventoryGUI = new InventoryGUI(game, _screen, _player, _world);
        }

        public void Start()
        {
            _inventoryGUI.Start();
        }

        public void Stop()
        {
            _inventoryGUI.Stop();
        }

        public void LoadContent(ContentManager content)
        {
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _colorScreenMask.Draw();
        }

        public void DrawAfterGUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _inventoryGUI.DrawAfterGUI(gameTime, spriteBatch);
        }

        public void Dispose()
        {
            _inventoryGUI.Dispose();
            _colorScreenMask.Dispose();
        }
    }
}