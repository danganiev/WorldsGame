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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.DataClasses;
using WorldsGame.Playing.Renderers;
using WorldsGame.Playing.Renderers.Character;
using WorldsGame.Playing.Renderers.ContentLoaders;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Renderers
{
    internal class PlayerGUIRenderer : IRenderer
    {
        private readonly World _world;

        private readonly GraphicsDevice _graphicsDevice;

        private Texture2D _crosshairTexture;

        //        private Texture2D _selectedBlockTexture;
        private SpriteBatch _spriteBatch;

        private PlayerAttributesRenderer _playerAttributesRenderer;

        // This is on-screen 10 items
        private PlayerHUDInventoryRenderer _playerHUDInventoryRenderer;

        // This is full inventory
        private PlayerInventoryRenderer _inventoryRenderer;

        internal PlayerGUIRenderer(GraphicsDevice device, World world)
        {
            _world = world;
            _graphicsDevice = device;

            _playerAttributesRenderer = new PlayerAttributesRenderer(_graphicsDevice, _world);
            _playerHUDInventoryRenderer = new PlayerHUDInventoryRenderer(_graphicsDevice, _world);
        }

        public void Initialize()
        {
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _playerAttributesRenderer.Initialize();
            _playerHUDInventoryRenderer.Initialize();

            Subscribe();
        }

        private void Subscribe()
        {
            Messenger.On("PlayerInventoryToggle", OnInventoryToggle);
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
            _crosshairTexture = content.Load<Texture2D>("Textures\\crosshair");

            //            _selectedBlockTexture = null;
            _playerAttributesRenderer.LoadContent();
            _playerHUDInventoryRenderer.LoadContent(content);
        }

        public void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.Default, RasterizerState.CullNone);

            DrawCrosshair();
            _playerAttributesRenderer.Draw(gameTime, _spriteBatch);
            _playerHUDInventoryRenderer.Draw(gameTime, _spriteBatch);

            _spriteBatch.End();
        }

        private void DrawCrosshair()
        {
            _spriteBatch.Draw(_crosshairTexture, new Vector2(
                                                     (_graphicsDevice.Viewport.Width / 2) - 10,
                                                     (_graphicsDevice.Viewport.Height / 2) - 10), Color.White);

            //            if (_selectedBlockTexture != null)
            //            {
            //                _spriteBatch.Draw(_selectedBlockTexture,
            //                                  new Vector2(_graphicsDevice.Viewport.Width - 100,
            //                                              _graphicsDevice.Viewport.Height - 100),
            //                                  Color.White);
            //            }
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }

        //        private void ChangeSelectedBlockTexture(string blockTypeKey)
        //        {
        //            if (_selectedBlockTexture != null)
        //            {
        //                _selectedBlockTexture.Dispose();
        //            }
        //
        //            BlockType blockType = BlockTypeHelper.Get(blockTypeKey);
        //            CompiledTexture texture = blockType.GetTexture();
        //            TextureAtlas atlas = World.CompiledGameBundle.GetTextureAtlas(texture.AtlasIndex);
        //            Color[] colors = atlas.GetPartialTexture(texture.Index);
        //
        //            _selectedBlockTexture = new Texture2D(_graphicsDevice, 32, 32);
        //            _selectedBlockTexture.SetData(colors);
        //        }

        private void OnInventoryToggle()
        {
        }

        public void Dispose()
        {
            Messenger.Off("PlayerInventoryToggle", OnInventoryToggle);

            _spriteBatch.Dispose();
        }
    }
}