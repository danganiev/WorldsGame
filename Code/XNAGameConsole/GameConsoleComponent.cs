using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAGameConsole.Commands;

namespace XNAGameConsole
{
    internal class GameConsoleComponent : DrawableGameComponent
    {
        public bool IsOpen
        {
            get
            {
                return renderer.IsOpen;
            }
        }

        private readonly GameConsole console;
        private readonly SpriteBatch spriteBatch;
        private readonly Renderer renderer;
        private static IConsoleCommand[] _inbuiltCommands;

        public InputProcessor InputProcessor { get; private set; }

        public event Action ConsoleOpened = () => { };

        public event Action ConsoleClosed = () => { };

        public GameConsoleComponent(GameConsole console, Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            this.console = console;
            this.spriteBatch = spriteBatch;
            AddPresetCommands();
            InputProcessor = new InputProcessor(new CommandProcesser());
            InputProcessor.Open += OnConsoleOpen;
            InputProcessor.Close += OnConsoleClose;

            renderer = new Renderer(game, spriteBatch, InputProcessor);

            _inbuiltCommands = new IConsoleCommand[] { new ClearScreenCommand(InputProcessor), new ExitCommand(game), new HelpCommand() };

            GameConsoleOptions.Commands.AddRange(_inbuiltCommands);
        }

        private void OnConsoleClose(object s, EventArgs e)
        {
            renderer.Close();
            ConsoleClosed();
        }

        private void OnConsoleOpen(object s, EventArgs e)
        {
            renderer.Open();
            ConsoleOpened();
        }

        public void ClearCommands()
        {
            console.Commands.Clear();
            console.Commands.AddRange(_inbuiltCommands);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!console.Enabled)
            {
                return;
            }
            spriteBatch.Begin();
            renderer.Draw(gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (!console.Enabled)
            {
                return;
            }
            renderer.Update(gameTime);
            base.Update(gameTime);
        }

        public void WriteLine(string text)
        {
            InputProcessor.AddToOutput(text);
        }

        private void AddPresetCommands()
        {
            // No absolute default commands yet
        }

        protected override void Dispose(bool disposing)
        {
            ConsoleOpened = null;
            ConsoleClosed = null;

            base.Dispose(disposing);
        }
    }
}