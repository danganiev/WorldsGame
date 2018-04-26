using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using WorldsGame.Utils.EventMessenger;
using WorldsGame.Utils.Input;

namespace WorldsGame.Utils
{
    internal class InputController
    {
        internal const int BACKSPACE = 8;
        internal const int ENTER = 13;

        private readonly GraphicsDeviceManager _graphics;
        private readonly Viewport _viewport;

        private MouseState _mouseMoveState;

        internal Cursor Cursor { get; private set; }

        internal KeyboardState CurrentKeyboardState { get; private set; }

        internal MouseState CurrentMouseState { get; private set; }

        internal MouseState PreviousMouseState { get; private set; }

        internal bool IsMouseCentered { get; set; }

        internal bool IsMouseUncenteredDetection { get; set; }

        internal event Action MousePositionChanged = () => { };

        // Pressed key events are sent every possible moment, until key is depressed.
        private static readonly Dictionary<Keys, string> PRESSED_EVENTS_NAMES = new Dictionary<Keys, string>
        {
//            {Keys.A, "AKeyPressed"},
//            {Keys.S, "SKeyPressed"},
//            {Keys.W, "WKeyPressed"},
//            {Keys.D, "DKeyPressed"},
//            {Keys.T, "TKeyPressed"},
            {Keys.E, "EKeyPressed"},
            {Keys.Q, "QKeyPressed"},
            {Keys.Z, "ZKeyPressed"},
//            {Keys.Space, "SpaceKeyPressed"},
            {Keys.Escape, "EscapeKeyPressed"},
            {Keys.PageUp, "PageUpKeyPressed"},
            {Keys.PageDown, "PageDownKeyPressed"},
        };

        // Released key events are sent only once
        private static readonly Dictionary<Keys, string> RELEASED_EVENTS_NAMES = new Dictionary<Keys, string>
        {
            //            {Keys.A, "AKeyReleased"},
            //            {Keys.S, "SKeyReleased"},
            //            {Keys.W, "WKeyReleased"},
            //            {Keys.D, "DKeyReleased"},
            //            {Keys.T, "TKeyReleased"},
            //            {Keys.Space, "SpaceKeyReleased"},
            //            {Keys.Escape, "EscapeKeyReleased"},
        };

        internal InputController(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
            _viewport = _graphics.GraphicsDevice.Viewport;
            CurrentMouseState = Mouse.GetState();
            Cursor = new Cursor(_graphics);
        }

        internal void Update()
        {
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
            CurrentKeyboardState = Keyboard.GetState();

            ProcessMouse();
        }

        private void ProcessMouse()
        {
            if (CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released)
            {
                Messenger.Invoke("MouseLeftButtonClick");
                Messenger.Invoke("KeyPressed", Keys.F22);
            }
            if (CurrentMouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Released)
            {
                Messenger.Invoke("MouseRightButtonClick");
                Messenger.Invoke("KeyPressed", Keys.F23);
            }
            if (CurrentMouseState.MiddleButton == ButtonState.Pressed && PreviousMouseState.MiddleButton == ButtonState.Released)
            {
                Messenger.Invoke("MouseMiddleButtonClick");
                Messenger.Invoke("KeyPressed", Keys.F24);
            }

            if (IsMouseCentered)
            {
                float mouseDX = CurrentMouseState.X - _mouseMoveState.X;
                float mouseDY = CurrentMouseState.Y - _mouseMoveState.Y;

                if (mouseDX != 0)
                {
                    Messenger.Invoke("MouseDeltaXChange", mouseDX);
                }
                if (mouseDY != 0)
                {
                    Messenger.Invoke("MouseDeltaYChange", mouseDY);
                }
                _mouseMoveState = new MouseState(_viewport.Width / 2, _viewport.Height / 2, 0, ButtonState.Released,
                                                 ButtonState.Released, ButtonState.Released, ButtonState.Released,
                                                 ButtonState.Released);

                Mouse.SetPosition(_mouseMoveState.X, _mouseMoveState.Y);
            }
            else if (IsMouseUncenteredDetection)
            {
                float mouseDX = CurrentMouseState.X - _mouseMoveState.X;
                float mouseDY = CurrentMouseState.Y - _mouseMoveState.Y;

                Cursor.Update(CurrentMouseState);

                if (mouseDX != 0 || mouseDY != 0)
                {
                    //                    MousePositionChanged();
                    Messenger.Invoke("MouseDeltaChange", mouseDX, mouseDY);
                }
            }
        }

        internal void MouseWheelRotated(float ticks)
        {
            Messenger.Invoke("ScrollWheelDeltaChanged", (int)ticks);
        }

        internal void MouseButtonPressed(MouseButtons buttons)
        {
            if (buttons == MouseButtons.Left)
            {
                Messenger.Invoke("MouseLeftButtonPressed");
            }

            if (buttons == MouseButtons.Right)
            {
                Messenger.Invoke("MouseRightButtonPressed");
            }

            //            if (buttons == MouseButtons.Middle)
            //            {
            //                Messenger.Invoke("MouseMiddleButtonPressed");
            //            }
        }

        internal void MouseButtonReleased(MouseButtons buttons)
        {
            if (buttons == MouseButtons.Left)
            {
                Messenger.Invoke("MouseLeftButtonReleased");
            }

            if (buttons == MouseButtons.Right)
            {
                Messenger.Invoke("MouseRightButtonReleased");
            }
            //            if (buttons == MouseButtons.Middle)
            //            {
            //                Messenger.Invoke("MouseMiddleButtonReleased");
            //            }
        }

        internal void KeyPressed(Keys key)
        {
            Messenger.Invoke("KeyPressed", key);

            if (key == Keys.LeftShift || key == (Keys)16 /*For some reasons shift doesn't work here*/)
            {
                Messenger.Invoke("ShiftKeyPressed");
            }

            if (PRESSED_EVENTS_NAMES.ContainsKey(key))
            {
                Messenger.Invoke(PRESSED_EVENTS_NAMES[key]);
            }
        }

        internal void KeyReleased(Keys key)
        {
            Messenger.Invoke("KeyReleased", key);

            if (key == Keys.LeftShift || key == (Keys)16)
            {
                Messenger.Invoke("ShiftKeyReleased");
            }

            if (RELEASED_EVENTS_NAMES.ContainsKey(key))
            {
                Messenger.Invoke(RELEASED_EVENTS_NAMES[key]);
            }
        }

        internal void CharacterEntered(char character)
        {
            Messenger.Invoke("CharacterEntered", character);
        }

        internal void ToggleMouseCentering()
        {
            IsMouseCentered = !IsMouseCentered;

            _mouseMoveState = new MouseState(CurrentMouseState.X, CurrentMouseState.Y, 0, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }
    }
}