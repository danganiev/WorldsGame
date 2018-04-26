using System;
using Microsoft.Xna.Framework;
using Nuclex.Graphics.SpecialEffects.Masks;
using Nuclex.UserInterface;
using WorldsGame.Gamestates;
using WorldsGame.GUI;
using WorldsGame.Terrain;
using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.PauseMenu
{
    internal class PauseMenu : IDisposable
    {
        private PauseMenuGUI _pauseMenuGUI;
        private readonly WorldsGame _game;
        private readonly WorldType _worldType;
        private readonly PlayingState _playingState;
        private readonly Screen _screen;
        private readonly ColorScreenMask _colorScreenMask;

        internal bool IsPaused { get; private set; }

        // This is set from the ObjectCreationState
        internal SelectionGrid SelectionGrid { get; set; }

        internal PauseMenu(WorldsGame game, WorldType worldType, PlayingState playingState, Screen screen)
        {
            _game = game;
            _worldType = worldType;
            _playingState = playingState;
            _screen = screen;
            _colorScreenMask = ColorScreenMask.Create(_game.GraphicsDevice);

            Color color = Color.Black;
            color.A = 150;
            _colorScreenMask.Color = color;
        }

        internal void Start()
        {
            Messenger.Invoke("PauseMenuStart");

            if (_worldType == WorldType.LocalWorld || _worldType == WorldType.NetworkWorld)
            {
                _pauseMenuGUI = new PauseMenuGUI(_game, _playingState, _screen);
            }
            else if (_worldType == WorldType.ObjectCreationWorld)
            {
                _pauseMenuGUI = new ObjectEditorPauseMenuGUI(_game, SelectionGrid, _playingState, _screen);
            }

            _pauseMenuGUI.Start();
            _game.IsMouseVisible = true;

            Messenger.Invoke("ToggleMouseCentering");

            IsPaused = true;
        }

        internal void Stop()
        {
            Messenger.Invoke("ToggleMouseCentering");

            _game.IsMouseVisible = false;
            _pauseMenuGUI.Stop();

            IsPaused = false;

            Messenger.Invoke("PauseMenuStop");
        }

        internal void Draw()
        {
            if (IsPaused)
            {
                _colorScreenMask.Draw();
            }
        }

        public void Dispose()
        {
            _colorScreenMask.Dispose();
        }
    }
}