using System;
using Microsoft.Xna.Framework;
using Nuclex.Graphics.SpecialEffects.Masks;
using WorldsGame.Gamestates;
using WorldsGame.GUI;

using WorldsGame.Utils.EventMessenger;

namespace WorldsGame.Playing.PauseMenu
{
    internal class ModelEditorPauseMenu : IDisposable
    {
        private readonly WorldsGame _game;

        private readonly BlockEditorState _blockEditorState;
        private readonly CharacterEditorState _characterEditorState;
        private readonly ItemEditorState _itemEditorState;

        private readonly ColorScreenMask _colorScreenMask;

        internal bool IsPaused { get; private set; }

        // Different gui for each editor state
        internal BlockEditorPauseMenuGUI BlockEditorGUI { get; private set; }

        internal CharacterEditorPauseMenuGUI CharacterEditorGUI { get; private set; }

        internal ItemEditorPauseMenuGUI ItemEditorGUI { get; private set; }

        internal ModelEditorPauseMenu(WorldsGame game)
        {
            _game = game;

            _colorScreenMask = ColorScreenMask.Create(_game.GraphicsDevice);

            Color color = Color.Black;
            color.A = 150;
            _colorScreenMask.Color = color;
        }

        internal ModelEditorPauseMenu(WorldsGame game, BlockEditorState blockEditorState)
            : this(game)
        {
            _blockEditorState = blockEditorState;
        }

        internal ModelEditorPauseMenu(WorldsGame game, CharacterEditorState characterEditorState)
            : this(game)
        {
            _characterEditorState = characterEditorState;
        }

        internal ModelEditorPauseMenu(WorldsGame game, ItemEditorState itemEditorState)
            : this(game)
        {
            _itemEditorState = itemEditorState;
        }

        internal void Start()
        {
            Messenger.Invoke("PauseMenuStart");

            if (_blockEditorState != null)
            {
                BlockEditorGUI = new BlockEditorPauseMenuGUI(_game, _blockEditorState);
                BlockEditorGUI.Start();
            }
            else if (_characterEditorState != null)
            {
                CharacterEditorGUI = new CharacterEditorPauseMenuGUI(_game, _characterEditorState);
                CharacterEditorGUI.Start();
            }
            else if (_itemEditorState != null)
            {
                ItemEditorGUI = new ItemEditorPauseMenuGUI(_game, _itemEditorState);
                ItemEditorGUI.Start();
            }

            IsPaused = true;
        }

        internal void Stop()
        {
            if (_blockEditorState != null)
            {
                BlockEditorGUI.Stop();
            }
            else if (_characterEditorState != null)
            {
                CharacterEditorGUI.Stop();
            }

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
        }
    }
}