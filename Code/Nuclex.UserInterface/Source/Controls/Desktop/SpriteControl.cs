using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Input;

namespace Nuclex.UserInterface.Controls.Desktop
{
    /// <summary>A cell that contains an icon</summary>
    public class SpriteControl : Control, IDisposable
    {
        public Texture2D Sprite { get; set; }

        public int Size { get; set; }

        /// <summary>
        /// Gets icon coordinates
        /// </summary>
        /// <returns></returns>
        public Vector2 GetSpriteCoordinates()
        {
            const int diff = 6;
            RectangleF bounds = GetAbsoluteBounds();
            return new Vector2(bounds.X + diff, bounds.Y + diff);
        }

        public void Dispose()
        {
            Sprite.Dispose();
        }
    }
}