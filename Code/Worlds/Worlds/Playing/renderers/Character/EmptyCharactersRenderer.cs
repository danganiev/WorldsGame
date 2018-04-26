using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using WorldsGame.Playing.Renderers.ContentLoaders;

namespace WorldsGame.Playing.Renderers.Character
{
    internal class EmptyCharactersRenderer : ICharactersRenderer
    {
        public void Initialize()
        {
        }

        public void Draw(GameTime gameTime)
        {
        }

        public void DrawTransparent(GameTime gameTime)
        {
        }

        public void LoadContent(ContentManager content, WorldsContentLoader worldsContentLoader)
        {
        }

        public void Dispose()
        {
        }
    }
}