using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.Game.States;
using Nuclex.UserInterface;

namespace WorldsGame.Gamestates
{
    internal class WorldsGameState : DrawableGameState, IDisposable
    {
        protected WorldsGame Game { get; set; }

        protected ContentManager Content { get; set; }

        protected GraphicsDevice GraphicsDevice { get { return Graphics.GraphicsDevice; } }

        protected GraphicsDeviceManager Graphics { get; set; }

        protected GuiManager GUIManager { get; set; }

        protected bool IsDisposed { get; set; }

        internal WorldsGameState(WorldsGame game)
        {
            Game = game;
            Content = game.Content;
            // http://stackoverflow.com/questions/16225701/how-to-prevent-graphicsdevice-from-being-disposed-when-applying-new-settings
            //            GraphicsDevice = game.GraphicsDevice;
            Graphics = game.Graphics;
            GUIManager = game.GUIManager;
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
        {
        }

        public virtual void Dispose()
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
        }

        protected override void OnEntered()
        {
            Initialize();
            LoadContent();
            base.OnEntered();
        }

        protected override void OnLeaving()
        {
            UnloadContent();
            Dispose();
            base.OnLeaving();
        }
    }
}