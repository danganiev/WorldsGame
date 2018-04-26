
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers
{
    /// <summary>Renders label controls in a traditional flat style</summary>
    public class FlatItemCountLabelControlRenderer :
      IFlatControlRenderer<Controls.ItemCountLabelControl>
    {
        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
          Controls.ItemCountLabelControl control, IFlatGuiGraphics graphics
        )
        {
            graphics.DrawString("item_count_label", control.GetAbsoluteBounds(), control.Text);
        }
    }
}