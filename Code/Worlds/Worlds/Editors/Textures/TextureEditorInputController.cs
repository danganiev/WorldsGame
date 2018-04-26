using System;
using Microsoft.Xna.Framework.Input;
using WorldsGame.Utils;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Editors.Textures
{
    internal class TextureEditorInputController : IDisposable
    {
        internal MouseState MouseState { get; set; }

        internal KeyboardState KeyboardState { get; set; }

        internal bool LeftMouseButtonPressed { get { return MouseState.LeftButton == ButtonState.Pressed; } }

        internal bool RightMouseButtonPressed { get { return MouseState.RightButton == ButtonState.Pressed; } }

        internal bool LeftShiftPressed { get { return KeyboardState.IsKeyDown(Keys.LeftShift); } }

        internal bool CtrlPressed { get { return KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl); } }

        internal event Action Undo = () => { };

        public TextureEditorInputController()
        {
            Initialize();
        }

        private void Initialize()
        {
            Messenger.On("ZKeyPressed", OnZKeyPressed);
        }

        private void OnZKeyPressed()
        {
            if (CtrlPressed)
            {
                Undo();
            }
        }

        internal void FixStates()
        {
            MouseState = Mouse.GetState();
            KeyboardState = Keyboard.GetState();
        }

        internal void Update()
        {
            FixStates();
        }

        public void Dispose()
        {
            Messenger.Off("ZKeyPressed", OnZKeyPressed);
            Undo = null;
        }
    }
}