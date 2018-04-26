using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Playing.Entities;
using WorldsGame.Playing.Entities.Components;
using WorldsGame.Playing.NPCs;
using WorldsGame.Playing.Players;
using WorldsGame.Saving;
using WorldsGame.Terrain;

namespace WorldsGame.Playing.Renderers.Character
{
    internal class PlayerAttributesRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Player _player;

        private Texture2D _attributeIconsAtlas;

        private World _world;

        private Dictionary<int, byte> _attributeVisualValues;

        private Dictionary<int, Vector2> _screenPositions;
        private Dictionary<int, Tuple<int, int>> _computedUVs;

        internal PlayerAttributesRenderer(GraphicsDevice graphicsDevice, World world)
        {
            _graphicsDevice = graphicsDevice;
            _world = world;
            _player = world.ClientPlayer;
            _attributeVisualValues = new Dictionary<int, byte>();
        }

        public void Initialize()
        {
            var componentContainer = _player.PlayerEntity.GetComponent<CharacterActorComponent>();
            componentContainer.OnAttributeChange += OnAttributeChange;

            foreach (KeyValuePair<string, float> keyValuePair in componentContainer.Attributes)
            {
                OnAttributeChange(_player.PlayerEntity, keyValuePair.Key, keyValuePair.Value);
            }

            int x = _graphicsDevice.Viewport.Width / 2 - PlayerHUDInventoryRenderer.CELL_SIZE_IN_PIXELS * PlayerHUDInventoryRenderer.INVENTORY_CELL_AMOUNT / 2;
            int y = _graphicsDevice.Viewport.Height - 86;

            _screenPositions = new Dictionary<int, Vector2>();

            for (int i = 0; i < CharacterAttribute.MAX_ATTRIBUTES * 2; i += 2)
            {
                // This one is closer to the left side
                _screenPositions.Add(i, new Vector2(x, y));
                i += 2;
                _screenPositions.Add(i, new Vector2(x + PlayerHUDInventoryRenderer.CELL_SIZE_IN_PIXELS * 10 - ((CharacterAttribute.ICON_PIXEL_SIZE + 5) * 10 - 5), y));
                y -= CharacterAttribute.ICON_PIXEL_SIZE + 5;
            }

            _computedUVs = new Dictionary<int, Tuple<int, int>>();

            int atlasPower = (int)Math.Sqrt(CharacterAttribute.MAX_ATTRIBUTES * 2);

            //            int atlasSize = atlasPower*atlasPower*CharacterAttribute.ICON_PIXEL_SIZE*CharacterAttribute.ICON_PIXEL_SIZE;
            //            float oneOverAtlasSize = 1/atlasSize;

            for (int i = 0; i < CharacterAttribute.MAX_ATTRIBUTES * 2; i++)
            {
                var uv = new Tuple<int, int>(i % atlasPower, i / atlasPower);

                _computedUVs.Add(i, uv);
            }
        }

        public void LoadContent()
        {
            _attributeIconsAtlas = _world.CompiledGameBundle.CharacterAttributeIconsAtlas;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (KeyValuePair<int, byte> attributeVisualValue in _attributeVisualValues)
            {
                byte amount = attributeVisualValue.Value;
                bool isHalved = amount % 2 == 1;
                amount = (byte)(amount / 2);
                Tuple<int, int> uv = _computedUVs[attributeVisualValue.Key];
                Vector2 screenPosition = _screenPositions[attributeVisualValue.Key];
                for (int i = 0; i <= amount - 1; i++)
                {
                    spriteBatch.Draw(
                        _attributeIconsAtlas,
                        screenPosition + new Vector2(i * (CharacterAttribute.ICON_PIXEL_SIZE + 5), 0),
                        new Rectangle(
                            uv.Item1 * CharacterAttribute.ICON_PIXEL_SIZE, uv.Item2 * CharacterAttribute.ICON_PIXEL_SIZE,
                            CharacterAttribute.ICON_PIXEL_SIZE, CharacterAttribute.ICON_PIXEL_SIZE),
                        Color.White
                    );
                }
            }
        }

        private void OnAttributeChange(Entity owner, string name, float value)
        {
            float diff = CharacterAttributeHelper.GetMinMaxDiff(name);

            float relativeValue = (value / diff) * 20;

            _attributeVisualValues[CharacterAttributeHelper.GetIndex(name)] = (byte)Math.Ceiling(relativeValue);
        }
    }
}