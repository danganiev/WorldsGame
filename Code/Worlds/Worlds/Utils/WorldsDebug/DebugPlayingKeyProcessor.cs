using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace WorldsGame.Utils.WorldsDebug
{
    internal class DebugPlayingKeyProcessor
    {
        private readonly World _world;
        private readonly WorldsGame _game;
        private readonly GraphicsDeviceManager _graphics;
        private TimeSpan _previousTime;

        private KeyboardState _oldKeyboardState;

        internal DebugPlayingKeyProcessor(GraphicsDeviceManager graphics, WorldsGame game, World world)
        {
            _graphics = graphics;
            _game = game;
            _world = world;
            _previousTime = TimeSpan.FromMilliseconds(0);
        }

        internal void Process(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            //freelook mode
            if (_oldKeyboardState.IsKeyUp(Keys.F1) && keyState.IsKeyDown(Keys.F1))
            {
                if ((gameTime.TotalGameTime - _previousTime).TotalMilliseconds > 100)
                {
                    _world.ClientPlayer.ToggleMovementBehaviourChange();

                    _previousTime = gameTime.TotalGameTime;
                }
            }

            //#if DEBUG
            //wireframe mode
            if (_oldKeyboardState.IsKeyUp(Keys.F7) && keyState.IsKeyDown(Keys.F7))
            {
                _world.ToggleRasterMode();
            }

            //diagnose mode
            if (_oldKeyboardState.IsKeyUp(Keys.F8) && keyState.IsKeyDown(Keys.F8))
            {
                SettingsManager.Settings.DiagnosticMode = !SettingsManager.Settings.DiagnosticMode;
            }

            //            //day cycle/dayMode
            //            if (_oldKeyboardState.IsKeyUp(Keys.F9) && keyState.IsKeyDown(Keys.F9))
            //            {
            //                _world.IsDayMode = !_world.IsDayMode;
            //            }
            //
            //            //day cycle/nightMode
            //            if (_oldKeyboardState.IsKeyUp(Keys.F10) && keyState.IsKeyDown(Keys.F10))
            //            {
            //                _world.IsNightMode = !_world.IsNightMode;
            //            }

            // fixed time step
            if (_oldKeyboardState.IsKeyUp(Keys.F3) && keyState.IsKeyDown(Keys.F3))
            {
                _graphics.SynchronizeWithVerticalRetrace = !_graphics.SynchronizeWithVerticalRetrace;
                _game.IsFixedTimeStep = !_game.IsFixedTimeStep;
                _graphics.ApplyChanges();
            }

            _oldKeyboardState = keyState;
            //#endif
        }
    }
}