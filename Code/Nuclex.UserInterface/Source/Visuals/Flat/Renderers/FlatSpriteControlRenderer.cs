using Microsoft.Xna.Framework;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers
{
    /// <summary>Renders button controls in a traditional flat style</summary>
    public class FlatSpriteControlRenderer :
        IFlatControlRenderer<SpriteControl>
    {
        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(SpriteControl control, IFlatGuiGraphics graphics)
        {
            if (control.Sprite != null)
            {
                Vector2 coordinates = control.GetSpriteCoordinates();
                graphics.DrawSprite(control.Sprite, new Rectangle((int)coordinates.X, (int)coordinates.Y, control.Size, control.Size));
            }
        }
    }
}