using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldsGame.GUI.MainMenu;

namespace WorldsGame.Gamestates
{
    internal class MenuState : WorldsGameState
    {
        private View.GUI.GUI _gui;
        private SpriteBatch _spriteBatch;

        internal MenuState(WorldsGame game, bool isRestarted = false)
            : base(game)
        {
            Game.IsMouseVisible = true;

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            Game.OnAfterDraw += DrawAfterGUI;

            _gui = isRestarted ? (View.GUI.GUI)new SettingsGUI(game) : new MainMenuGUI(game);
            _gui.MenuState = this;
        }

        protected override void Initialize()
        {
            _gui.Start();
        }

        internal void SetGUI(View.GUI.GUI gui)
        {
            _gui = gui;
            _gui.MenuState = this;
            _gui.Start();
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.LightGray);
            _gui.Draw(gameTime, _spriteBatch);
        }

        private void DrawAfterGUI(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied, SamplerState.PointClamp,
                DepthStencilState.Default, RasterizerState.CullNone);

            _gui.DrawAfterGUI(gameTime, _spriteBatch);

            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            _gui.Update(gameTime);
        }

        internal void OnPush()
        {
            Game.OnAfterDraw -= DrawAfterGUI;
        }

        public override void Dispose()
        {
            base.Dispose();
            _spriteBatch.Dispose();
        }
    }
}