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
#endregion

using System;
using System.Collections.Generic;

namespace Nuclex.UserInterface.Visuals.Flat.Renderers {

  /// <summary>Renders choice controls in a traditional flat style</summary>
  public class FlatChoiceControlRenderer :
    IFlatControlRenderer<Controls.Desktop.ChoiceControl> {

    /// <summary>
    ///   Renders the specified control using the provided graphics interface
    /// </summary>
    /// <param name="control">Control that will be rendered</param>
    /// <param name="graphics">
    ///   Graphics interface that will be used to draw the control
    /// </param>
    public void Render(
      Controls.Desktop.ChoiceControl control, IFlatGuiGraphics graphics
    ) {

      // Determine the index of the state we're going to display
      int stateIndex = (control.Selected ? 4 : 0);
      if(control.Enabled) {
        if(control.Depressed) {
          stateIndex += 3;
        } else if(control.MouseHovering) {
          stateIndex += 2;
        } else {
          stateIndex += 1;
        }
      }

      // Get the pixel coordinates of the region covered by the control on
      // the screen
      RectangleF controlBounds = control.GetAbsoluteBounds();
      float width = controlBounds.Width;

      // Now adjust the bounds to a square of height x height pixels so we can
      // render the graphical portion of the choice control
      controlBounds.Width = controlBounds.Height;
      graphics.DrawElement(states[stateIndex], controlBounds);

      // If the choice has text assigned to it, render it too
      if(!string.IsNullOrEmpty(control.Text)) {

        // Restore the original width, then subtract the region that was covered by
        // the graphical portion of the control.
        controlBounds.Width = width - controlBounds.Height;
        controlBounds.X += controlBounds.Height;

        // Draw the text that was assigned to the choice control        
        graphics.DrawString(states[stateIndex], controlBounds, control.Text);

      }

    }

    /// <summary>Names of the states the choice control can be in</summary>
    /// <remarks>
    ///   Storing this as full strings instead of building them dynamically prevents
    ///   any garbage from forming during rendering.
    /// </remarks>
    private static readonly string[] states = new string[] {
      "radio.off.disabled",
      "radio.off.normal",
      "radio.off.highlighted",
      "radio.off.depressed",
      "radio.on.disabled",
      "radio.on.normal",
      "radio.on.highlighted",
      "radio.on.depressed"
    };      

  }

} // namespace Nuclex.UserInterface.Visuals.Flat.Renderers
