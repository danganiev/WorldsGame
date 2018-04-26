using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nuclex.UserInterface.Controls.Desktop;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers
{
    /// <summary>Renders text input controls in a traditional flat style</summary>
    public class FlatTextPopupControlRenderer :
        IFlatControlRenderer<Controls.Desktop.TextPopupControl>//,
    //        Controls.Desktop.IOpeningLocator
    {
        /// <summary>Style from the skin this renderer uses</summary>
        private const string Style = "textpopup";

        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(TextPopupControl control, IFlatGuiGraphics graphics)
        {
            if (control.Enabled)
            {
                string text = control.Text ?? string.Empty;

                if (text == string.Empty)
                {
                    return;
                }

                var controlBounds = control.IsInventory ?
                    new RectangleF
                    {
                        X = Mouse.GetState().X + 16,
                        Y = Mouse.GetState().Y
                    } : new RectangleF
                            {
                                X = Mouse.GetState().X + 32,
                                Y = Mouse.GetState().Y - 16
                            };

                RectangleF stringSize = graphics.MeasureString(Style, controlBounds, text);

                controlBounds.Width = 20 + stringSize.Width;
                controlBounds.Height = 10 + stringSize.Height;

                graphics.DrawElement(Style, controlBounds);

                controlBounds.Offset(stringSize.X + 6, 4);

                graphics.DrawString(Style, controlBounds, text);
            }
        }
    }
}