using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldsGame.Network.Chat
{
    internal class ChatLine
    {
        private readonly List<TextSection> _sections;

        internal ChatLine()
        {
            _sections = new List<TextSection>();
        }

        internal ChatLine(string playerName, string text, SpriteFont font)
            : this()
        {
            _sections.Add(new TextSection(string.Format("[{0}]: ", playerName), Color.DarkViolet, font));
            _sections.Add(new TextSection(text, Color.Black, font));
        }

        internal void Draw(SpriteBatch spriteBatch, int height)
        {
            int x = 50;

            foreach (var textSection in _sections)
            {
                textSection.Draw(spriteBatch, new Vector2(x, height));
                Vector2 textSize = textSection.GetMeasurements();
                x += (int)textSize.X;
            }
        }
    }
}