#region CPL License

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

namespace Nuclex.UserInterface.Controls.Desktop
{
    /// <summary>Vertical slider that can be moved using the mouse</summary>
    public class VerticalSliderControl : SliderControl
    {
        /// <summary>Obtains the region covered by the slider's thumb</summary>
        /// <returns>The region covered by the slider's thumb</returns>
        protected override RectangleF GetThumbRegion()
        {
            RectangleF bounds = GetAbsoluteBounds();

            if (ThumbLocator != null)
            {
                return ThumbLocator.GetThumbPosition(
                      bounds, ThumbPosition, ThumbSize
                );
            }
            else
            {
                float thumbHeight = bounds.Height * ThumbSize;
                float thumbY = (bounds.Height - thumbHeight) * ThumbPosition;

                return new RectangleF(0, thumbY, bounds.Width, thumbHeight);
            }
        }

        /// <summary>Moves the thumb to the specified location</summary>
        /// <param name="x">X coordinate for the new left border of the thumb</param>
        /// <param name="y">Y coordinate for the new upper border of the thumb</param>
        protected override void MoveThumb(float x, float y)
        {
            RectangleF bounds = GetAbsoluteBounds();

            float thumbHeight = bounds.Height * ThumbSize;
            float maxY = bounds.Height - thumbHeight;

            // Prevent divide-by-zero if the thumb fills out the whole rail
            ThumbPosition = maxY > 0.0f ? MathHelper.Clamp(y / maxY, 0.0f, 1.0f) : 0.0f;

            OnMoved();
        }
    }
} // namespace Nuclex.UserInterface.Controls.Desktop