using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using WorldsGame.Playing.DataClasses;
using WorldsGame.Utils;

namespace WorldsGame.Playing.Renderers.ContentLoaders
{
    internal class WorldsContentLoader
    {
        private readonly ContentManager _content;

        internal Dictionary<int, Texture2D> TextureAtlases { get; private set; }

        internal Texture2D AttributeIconsAtlas { get; private set; }

        internal Effect SolidBlockEffect { get; set; }

        internal Effect LiquidBlockEffect { get; set; }

        internal Effect CustomModelEffect { get; set; }

        internal Effect AnimatedBlockEffect { get; set; }

        private readonly CompiledGameBundle _compiledGameBundle;

        internal WorldsContentLoader(CompiledGameBundle compiledGameBundle, ContentManager content)
        {
            _compiledGameBundle = compiledGameBundle;
            _content = content;

            TextureAtlases = new Dictionary<int, Texture2D>();
        }

        internal void LoadContent()
        {
            SolidBlockEffect = InternalSystemSettings.IsServer ? null : _content.Load<Effect>("Effects\\SolidBlockEffect");
            CustomModelEffect = InternalSystemSettings.IsServer ? null : _content.Load<Effect>("Effects\\CustomModelEffect");
            LiquidBlockEffect = InternalSystemSettings.IsServer ? null : _content.Load<Effect>("Effects\\LiquidBlockEffect");
            AnimatedBlockEffect = InternalSystemSettings.IsServer ? null : _content.Load<Effect>("Effects\\AnimatedBlockEffect");

            foreach (var textureAtlas in _compiledGameBundle.TextureAtlases)
            {
                TextureAtlases.Add(textureAtlas.Key, textureAtlas.Value.Texture);
            }
        }

        internal void Unload()
        {
            foreach (var atlas in TextureAtlases)
            {
                atlas.Value.Dispose();
            }
        }
    }
}