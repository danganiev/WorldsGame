using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Saving;
using WorldsGame.Saving.DataClasses;

namespace WorldsGame.GUI.RecipeEditor
{
    internal class ItemMousePicker : IDisposable
    {
        private static readonly Color[] _emptyColors = Icon.EMPTY_COLORS;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _size;
        private IItemLike _pickedItem;
        private Texture2D _icon;
        private Texture2D _oldIcon;

        internal IItemLike PickedItem
        {
            set
            {
                if (value != null)
                {
                    _pickedItem = value;
                    _isPicked = true;

                    try
                    {
                        _icon.SetData(value.IconColors);
                    }
                    catch (InvalidOperationException)
                    {
                        UpdateIcon(value.IconColors);
                    }
                }
                else
                {
                    Quantity = 0;
                    _isPicked = false;
                    _pickedItem = null;

                    UpdateIcon(_emptyColors);
                }
            }
            get { return _pickedItem; }
        }

        internal InventoryItem GetInventoryItem()
        {
            if (PickedItem == null)
            {
                return null;
            }

            return new InventoryItem { Name = PickedItem.Name, Quantity = Quantity };
        }

        private void UpdateIcon(Color[] colors)
        {
            if (_oldIcon != null)
            {
                _oldIcon.Dispose();
            }
            _oldIcon = _icon;
            _icon = new Texture2D(_graphicsDevice, Icon.SIZE, Icon.SIZE);
            _icon.SetData(colors);
        }

        private string _quantityText;
        private int _quantity;

        internal int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                _quantityText = _quantity.ToString();
            }
        }

        private bool _isPicked;
        private SpriteFont _font;

        internal ItemMousePicker(GraphicsDevice graphicsDevice, ContentManager content, int size = 32)
        {
            _graphicsDevice = graphicsDevice;
            _size = size;
            _icon = new Texture2D(graphicsDevice, Icon.SIZE, Icon.SIZE);

            _font = content.Load<SpriteFont>("Skin//Suave//SuaveDefaultFont");
        }

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_isPicked)
            {
                var currentMouseState = Mouse.GetState();
                spriteBatch.Draw(
                    _icon,
                    new Rectangle(currentMouseState.X - Icon.SIZE / 2, currentMouseState.Y - Icon.SIZE / 2, _size, _size),
                    Color.White);

                if (Quantity > 1)
                {
                    spriteBatch.DrawString(
                        _font, _quantityText,
                        new Vector2(currentMouseState.X - Icon.SIZE / 2, currentMouseState.Y - 2), Color.Black);
                }
            }
        }

        public void Clear()
        {
            PickedItem = null;
        }

        public void Dispose()
        {
            _icon.Dispose();
        }
    }
}