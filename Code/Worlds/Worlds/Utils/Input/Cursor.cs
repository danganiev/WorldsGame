using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WorldsGame.Utils.Input
{
    internal class Cursor
    {
        private readonly GraphicsDeviceManager _graphics;

        // Position is the cursor position, and is in screen space.
        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
        }

        public Cursor(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
        }

        // LoadContent needs to load the cursor texture and find its center.
        // also, we need to create a SpriteBatch.
        protected void LoadContent()
        {
            // we want to default the cursor to start in the center of the screen
            Viewport vp = _graphics.GraphicsDevice.Viewport;
            _position.X = vp.X + (vp.Width / 2);
            _position.Y = vp.Y + (vp.Height / 2);
        }

        public void Update(MouseState mouseState)
        {
            // We use different input on each platform:
            // On Xbox, we use the GamePad's DPad and left thumbstick to move the cursor around the screen.
            // On Windows, we directly map the cursor to the location of the mouse.
            // On Windows Phone, we use the primary touch point for the location of the cursor.
            //#if XBOX
            //            UpdateXboxInput(gameTime);
            //#elif WINDOWS
            UpdateWindowsInput(mouseState);
            //#elif WINDOWS_PHONE
            //            UpdateWindowsPhoneInput();
            //#endif
        }

        /// <summary>
        /// Handles input for Xbox 360.
        /// </summary>
        //        private void UpdateXboxInput(GameTime gameTime)
        //        {
        //            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
        //
        //            // we'll create a vector2, called delta, which will store how much the
        //            // cursor position should change.
        //            Vector2 delta = currentState.ThumbSticks.Left;
        //
        //            // down on the thumbstick is -1. however, in screen coordinates, values
        //            // increase as they go down the screen. so, we have to flip the sign of the
        //            // y component of delta.
        //            delta.Y *= -1;
        //
        //            // check the dpad: if any of its buttons are pressed, that will change delta as well.
        //            if (currentState.DPad.Up == ButtonState.Pressed)
        //            {
        //                delta.Y = -1;
        //            }
        //            if (currentState.DPad.Down == ButtonState.Pressed)
        //            {
        //                delta.Y = 1;
        //            }
        //            if (currentState.DPad.Left == ButtonState.Pressed)
        //            {
        //                delta.X = -1;
        //            }
        //            if (currentState.DPad.Right == ButtonState.Pressed)
        //            {
        //                delta.X = 1;
        //            }
        //
        //            // normalize delta so that we know the cursor can't move faster than CursorSpeed.
        //            if (delta != Vector2.Zero)
        //            {
        //                delta.Normalize();
        //            }
        //
        //            // modify position using delta, the CursorSpeed constant defined above, and
        //            // the elapsed game time.
        //            position += delta * CursorSpeed *
        //                (float)gameTime.ElapsedGameTime.TotalSeconds;
        //
        //            // clamp the cursor position to the viewport, so that it can't move off the screen.
        //            Viewport vp = GraphicsDevice.Viewport;
        //            position.X = MathHelper.Clamp(position.X, vp.X, vp.X + vp.Width);
        //            position.Y = MathHelper.Clamp(position.Y, vp.Y, vp.Y + vp.Height);
        //        }

        /// <summary>
        /// Handles input for Windows.
        /// </summary>
        private void UpdateWindowsInput(MouseState mouseState)
        {
            _position.X = mouseState.X;
            _position.Y = mouseState.Y;
        }

        // CalculateCursorRay Calculates a world space ray starting at the camera's
        // "eye" and pointing in the direction of the cursor. Viewport.Unproject is used
        // to accomplish this. see the accompanying documentation for more explanation
        // of the math behind this function.
        internal Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            var nearSource = new Vector3(Position, 0f);
            var farSource = new Vector3(Position, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = _graphics.GraphicsDevice.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = _graphics.GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }
    }
}