using Microsoft.Xna.Framework;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers
{
    /// <summary>Renders button controls in a traditional flat style</summary>
    public class FlatIconCellControlRenderer :
        IFlatControlRenderer<IconCellControl>
    {
        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
            IconCellControl control, IFlatGuiGraphics graphics
            )
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();

            // Determine the style to use for the button
            int stateIndex = 0;
            if (control.Enabled)
            {
                if (control.MouseHovering)
                {
                    stateIndex = 1;
                }
            }

            // Draw the button's frame
            graphics.DrawElement(states[stateIndex], controlBounds);

            if (control.Icon != null)
            {
                Vector2 coordinates = control.GetIconCoordinates();
                graphics.DrawSprite(control.Icon, new Rectangle((int)coordinates.X, (int)coordinates.Y, 32, 32));
            }
        }

        /// <summary>Names of the states the button control can be in</summary>
        /// <remarks>
        ///   Storing this as full strings instead of building them dynamically prevents
        ///   any garbage from forming during rendering.
        /// </remarks>
        private static readonly string[] states = new[]
        {
            "iconcell.normal",
            "iconcell.highlighted",
        };
    }
}