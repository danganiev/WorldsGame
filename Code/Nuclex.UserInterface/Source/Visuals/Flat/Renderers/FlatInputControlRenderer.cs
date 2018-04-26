﻿#region CPL License

/*
Nuclex Framework
Copyright (C) 2002-2010 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/

#endregion CPL License

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers
{
    /// <summary>Renders text input controls in a traditional flat style</summary>
    public class FlatInputControlRenderer :
      IFlatControlRenderer<Controls.Desktop.InputControl>,
      Controls.Desktop.IOpeningLocator
    {
        /// <summary>Style from the skin this renderer uses</summary>
        private const string Style = "input.normal";

        /// <summary>
        ///   Renders the specified control using the provided graphics interface
        /// </summary>
        /// <param name="control">Control that will be rendered</param>
        /// <param name="graphics">
        ///   Graphics interface that will be used to draw the control
        /// </param>
        public void Render(
          Controls.Desktop.InputControl control, IFlatGuiGraphics graphics
        )
        {
            RectangleF controlBounds = control.GetAbsoluteBounds();

            // Draw the control's frame and background
            graphics.DrawElement(Style, controlBounds);

            using (graphics.SetClipRegion(controlBounds))
            {
                string text = control.Text ?? string.Empty;

                // Amount by which the text will be moved within the input box in
                // order to keep the caret in view even when the text is wider than
                // the input box.
                float left = 0;

                // Only scroll the text within the input box when it has the input
                // focus and the caret is being shown.
                if (control.HasFocus)
                {
                    // Find out where the cursor is from the left end of the text
                    RectangleF stringSize = graphics.MeasureString(
                      Style, controlBounds, text.Substring(0, control.CaretPosition)
                    );

                    // TODO: Renderer should query the size of the control's frame
                    //   Otherwise text will be visible over the frame, which might look bad
                    //   if a skin uses a frame wider than 2 pixels or in a different color
                    //   than the text.
                    while (stringSize.Width + left > controlBounds.Width)
                    {
                        left -= controlBounds.Width / 10.0f;
                    }
                }

                // Draw the text into the input box
                controlBounds.X += left;
                graphics.DrawString(Style, controlBounds, control.Text);

                // If the input box is in focus, also draw the caret so the user knows
                // where characters will be inserted into the text.
                if (control.HasFocus)
                {
                    if (control.MillisecondsSinceLastCaretMovement % 500 < 250)
                    {
                        graphics.DrawCaret(
                          "input.normal", controlBounds, control.Text, control.CaretPosition
                        );
                    }
                }
            }

            // Let the control know that we can provide it with additional informations
            // about how its text is being rendered
            control.OpeningLocator = this;
            this.graphics = graphics;
        }

        /// <summary>
        ///   Calculates which opening between two letters is closest to a position
        /// </summary>
        /// <param name="bounds">
        ///   Boundaries of the control, should be in absolute coordinates
        /// </param>
        /// <param name="text">Text in which the opening will be looked for</param>
        /// <param name="position">
        ///   Position to which the closest opening will be found,
        ///   should be in absolute coordinates
        /// </param>
        /// <returns>The index of the opening closest to the provided position</returns>
        public int GetClosestOpening(
          RectangleF bounds, string text, Vector2 position
        )
        {
            return this.graphics.GetClosestOpening("input.normal", bounds, text, position);
        }

        // TODO: Find a better solution than remembering the graphics interface here
        //   Otherwise the renderer could try to renderer when no frame is being drawn.
        //   Also, the renderer makes the assumption that all drawing happens through
        //   one graphics interface only.

        /// <summary>Graphics interface we used for the last draw call</summary>
        private IFlatGuiGraphics graphics;
    }
} // namespace Nuclex.UserInterface.Visuals.Flat.Renderers