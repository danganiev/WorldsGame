using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.Utils;

namespace WorldsGame.Network.Chat
{
    // Draws "Say: <text>" message on screen
    internal class SayLine
    {
        internal static readonly int Y = SettingsManager.Settings.ResolutionHeight - 100;

        private SpriteFont SpriteFont { get; set; }

        private TextSection LabelSection { get; set; }

        private TextSection TextSection { get; set; }

        private string _text;

        internal string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                TextSection.Text = value;
            }
        }

        internal SayLine(SpriteFont spriteFont)
        {
            SpriteFont = spriteFont;
            LabelSection = new TextSection("Say: ", Color.DarkBlue, spriteFont);
            TextSection = new TextSection("", Color.Black, spriteFont);
            Text = "";
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            int x = 50;

            LabelSection.Draw(spriteBatch, new Vector2(x, Y));
            Vector2 textSize = LabelSection.GetMeasurements();
            x += (int)textSize.X;
            TextSection.Draw(spriteBatch, new Vector2(x, Y));
        }
    }
}