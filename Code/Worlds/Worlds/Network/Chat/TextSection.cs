using System;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace WorldsGame.Network.Chat
{
    internal class TextSection
    {
        internal String Text { get; set; }

        private Color Color { get; set; }

        private SpriteFont Font { get; set; }

        internal TextSection(string text, Color color, SpriteFont font)
        {
            Text = text;
            Color = color;
            Font = font;
        }

        internal void Draw(SpriteBatch spriteBatch, Vector2 location)
        {
            spriteBatch.DrawString(Font, Text, location, Color);
        }

        internal Vector2 GetMeasurements()
        {
            return Font.MeasureString(Text);
        }
    }
}