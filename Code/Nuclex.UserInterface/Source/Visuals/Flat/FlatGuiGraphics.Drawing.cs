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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Support;

namespace Nuclex.UserInterface.Visuals.Flat
{
    partial class FlatGuiGraphics
    {
        /// <summary>Needs to be called before the GUI drawing process begins</summary>
        public void BeginDrawing()
        {
            GraphicsDevice graphics = this.spriteBatch.GraphicsDevice;

            Viewport viewport = graphics.Viewport;
            graphics.ScissorRectangle = new Rectangle(
              0, 0, viewport.Width, viewport.Height
            );

            // On Windows Phone 7, if only the GUI is rendered (no other SpriteBatches)
            // and the initial spriteBatch.Begin() includes the scissor rectangle,
            // nothing will be drawn at all, so we don't use beginSpriteBatch() here
            // and instead call SpriteBatch.Begin() ourselves. Care has to be taken
            // if something ever gets added to the beginSpriteBatch() method.
            this.spriteBatch.Begin();
        }

        /// <summary>Needs to be called when the GUI drawing process has ended</summary>
        public void EndDrawing()
        {
            endSpriteBatch();
        }

        /// <summary>Sets the clipping region for any future drawing commands</summary>
        /// <param name="clipRegion">Clipping region that will be set</param>
        /// <returns>
        ///   An object that will unset the clipping region upon its destruction.
        /// </returns>
        /// <remarks>
        ///   Clipping regions can be stacked, though this is not very typical for
        ///   a game GUI and also not recommended practice due to performance constraints.
        ///   Unless clipping is implemented in software, setting up a clip region
        ///   on current hardware requires the drawing queue to be flushed, negatively
        ///   impacting rendering performance (in technical terms, a clipping region
        ///   change likely causes 2 more DrawPrimitive() calls from the painter).
        /// </remarks>
        public IDisposable SetClipRegion(RectangleF clipRegion)
        {
            // Cache the integer values of the clipping region's boundaries
            int clipX = (int)clipRegion.X;
            int clipY = (int)clipRegion.Y;
            int clipRight = clipX + (int)clipRegion.Width;
            int clipBottom = clipY + (int)clipRegion.Height;

            // Calculate the viewport's right and bottom coordinates
            Viewport viewport = this.spriteBatch.GraphicsDevice.Viewport;
            int viewportRight = viewport.X + viewport.Width;
            int viewportBottom = viewport.Y + viewport.Height;

            // Extract the part of the clipping region that lies within the viewport
            Rectangle scissorRegion = new Rectangle(
              Math.Max(clipX, viewport.X),
              Math.Max(clipY, viewport.Y),
              Math.Min(clipRight, viewportRight) - clipX,
              Math.Min(clipBottom, viewportBottom) - clipY
            );
            scissorRegion.Width += clipX - scissorRegion.X;
            scissorRegion.Height += clipY - scissorRegion.Y;

            // If the clipping region was entirely outside of the viewport (meaning
            // the calculated width and/or height are negative), use an empty scissor
            // rectangle instead because XNA doesn't like scissor rectangles with
            // negative coordinates.
            if ((scissorRegion.Width <= 0) || (scissorRegion.Height <= 0))
            {
                scissorRegion = Rectangle.Empty;
            }

            // All done, take over the new scissor rectangle
            this.scissorManager.Assign(ref scissorRegion);
            return this.scissorManager;
        }

        /// <summary>Draws a GUI element onto the drawing buffer</summary>
        /// <param name="frameName">Class of the element to draw</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <remarks>
        ///   <para>
        ///     GUI elements are the basic building blocks of a GUI:
        ///   </para>
        /// </remarks>
        public void DrawElement(string frameName, RectangleF bounds)
        {
            Frame frame = lookupFrame(frameName);

            // Draw all the regions defined for the element. Each region is a small bitmap
            // that needs to be blit somewhere into the element to form the element's
            // visual representation step by step.
            for (int index = 0; index < frame.Regions.Length; ++index)
            {
                Rectangle destinationRegion = calculateDestinationRectangle(
                  ref bounds, ref frame.Regions[index].DestinationRegion
                );

                this.spriteBatch.Draw(
                  frame.Regions[index].Texture,
                  destinationRegion,
                  frame.Regions[index].SourceRegion,
                  Color.White
                );
            }
        }

        /// <summary>Draws text into the drawing buffer for the specified element</summary>
        /// <param name="frameName">Class of the element for which to draw text</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        public void DrawString(string frameName, RectangleF bounds, string text)
        {
            Frame frame = lookupFrame(frameName);

            // Draw the text in all anchor locations defined by the skin
            for (int index = 0; index < frame.Texts.Length; ++index)
            {
                this.spriteBatch.DrawString(
                  frame.Texts[index].Font,
                  text,
                  positionText(ref frame.Texts[index], bounds, text),
                  frame.Texts[index].Color
                );
            }
        }

        /// <summary>Draws a caret for text input at the specified index</summary>
        /// <param name="frameName">Class of the element for which to draw a caret</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text for which a caret will be drawn</param>
        /// <param name="caretIndex">Index the caret will be drawn at</param>
        public void DrawCaret(
          string frameName, RectangleF bounds, string text, int caretIndex
        )
        {
            Frame frame = lookupFrame(frameName);

            this.stringBuilder.Remove(0, this.stringBuilder.Length);
            this.stringBuilder.Append(text, 0, caretIndex);

            Vector2 caretPosition, textPosition;
            for (int index = 0; index < frame.Texts.Length; ++index)
            {
                textPosition = positionText(ref frame.Texts[index], bounds, text);

                caretPosition = frame.Texts[index].Font.MeasureString(this.stringBuilder);
                caretPosition.X -= CaretWidth;
                caretPosition.Y = 0.0f;

                this.spriteBatch.DrawString(
                  frame.Texts[index].Font,
                  "|",
                  textPosition + caretPosition,
                  frame.Texts[index].Color
                );
            }
        }

        /// <summary>Draws a sprite</summary>
        /// <param name="sprite">Sprite</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be drawn</param>
        public void DrawSprite(Texture2D sprite, Rectangle bounds)
        {
            spriteBatch.Draw(sprite, bounds, Color.White);
        }

        /// <summary>Measures the extents of a string in the frame's area</summary>
        /// <param name="frameName">Class of the element whose text will be measured</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text that will be measured</param>
        /// <returns>
        ///   The size and extents of the specified string within the frame
        /// </returns>
        public RectangleF MeasureString(string frameName, RectangleF bounds, string text)
        {
            Frame frame = lookupFrame(frameName);

            Vector2 size;
            if (frame.Texts.Length > 0)
            {
                size = frame.Texts[0].Font.MeasureString(text);
            }
            else
            {
                size = Vector2.Zero;
            }

            return new RectangleF(0.0f, 0.0f, size.X, size.Y);
        }

        /// <summary>
        ///   Locates the closest gap between two letters to the provided position
        /// </summary>
        /// <param name="frameName">Class of the element in which to find the gap</param>
        /// <param name="bounds">Region that will be covered by the drawn element</param>
        /// <param name="text">Text in which the closest gap will be found</param>
        /// <param name="position">Position of which to determien the closest gap</param>
        /// <returns>The index of the gap the position is closest to</returns>
        public int GetClosestOpening(
          string frameName, RectangleF bounds, string text, Vector2 position
        )
        {
            Frame frame = lookupFrame(frameName);

            // TODO: Find the closest gap across multiple text anchors
            //   Frames can repeat their text in several places. Though this is probably
            //   not used very often (if at all), it should work here consistently.

            int closestGap = -1;

            for (int index = 0; index < frame.Texts.Length; ++index)
            {
                Vector2 textPosition = positionText(ref frame.Texts[index], bounds, text);
                position.X -= textPosition.X;
                position.Y -= textPosition.Y;

                float openingX = position.X;
                int openingIndex = this.openingLocator.FindClosestOpening(
                  frame.Texts[index].Font, text, position.X + CaretWidth
                );

                closestGap = openingIndex;
            }

            return closestGap;
        }

        /// <summary>Starts drawing on the sprite batch</summary>
        private void beginSpriteBatch()
        {
            spriteBatch.Begin(
              SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, rasterizerState
            );
        }

        /// <summary>Stops drawing on the sprite batch</summary>
        private void endSpriteBatch()
        {
            spriteBatch.End();
        }
    }
} // namespace Nuclex.UserInterface.Visuals.Flat