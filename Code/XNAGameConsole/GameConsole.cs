using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAGameConsole.Commands;

namespace XNAGameConsole
{
    public class GameConsole : IDisposable
    {
        private readonly Game _game;

        public GameConsoleOptions Options { get { return GameConsoleOptions.Options; } }

        public List<IConsoleCommand> Commands { get { return GameConsoleOptions.Commands; } }

        public bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether the console is currently opened
        /// </summary>
        public bool Opened { get { return consoleComponent.IsOpen; } }

        private readonly GameConsoleComponent consoleComponent;

        // Because Singleton (THAT'S BAD PROBABLY EVERY TIME YOU THINK, BUT IT MAKES SENSE THIS TIME)
        // Poor mans singleton just cause I'm lazy.
        public static GameConsole ConsoleInstance { get; set; }

        public event Action ConsoleOpened = () => { };

        public event Action ConsoleClosed = () => { };

        public GameConsole(Game game, SpriteBatch spriteBatch)
            : this(game, spriteBatch, new IConsoleCommand[0], new GameConsoleOptions())
        {
        }

        public GameConsole(Game game, SpriteBatch spriteBatch, GameConsoleOptions options)
            : this(game, spriteBatch, new IConsoleCommand[0], options)
        {
        }

        public GameConsole(Game game, SpriteBatch spriteBatch, IEnumerable<IConsoleCommand> commands)
            : this(game, spriteBatch, commands, new GameConsoleOptions())
        {
        }

        public GameConsole(Game game, SpriteBatch spriteBatch, IEnumerable<IConsoleCommand> commands, GameConsoleOptions options)
        {
            _game = game;
            //            var content = new ContentManager(game.Services, "Content");
            //            if (options.Font == null)
            //            {
            //                options.Font = content.Load<SpriteFont>("Fonts/DefaultFont");
            //            }
            GameConsoleOptions.Options = options;
            GameConsoleOptions.Commands = commands.ToList();
            Enabled = true;
            consoleComponent = new GameConsoleComponent(this, game, spriteBatch);

            consoleComponent.ConsoleOpened += OnConsoleOpened;
            consoleComponent.ConsoleClosed += OnConsoleClosed;

            game.Services.AddService(typeof(GameConsole), this);
            game.Components.Add(consoleComponent);
        }

        private void OnConsoleOpened()
        {
            ConsoleOpened();
        }

        private void OnConsoleClosed()
        {
            ConsoleClosed();
        }

        /// <summary>
        /// Write directly to the output stream of the console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            consoleComponent.WriteLine(text);
        }

        /// <summary>
        /// Adds a new command to the console
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommand(params IConsoleCommand[] commands)
        {
            Commands.AddRange(commands);
        }

        /// <summary>
        /// Adds a new command to the console
        /// </summary>
        /// <param name="name">Name of the command</param>
        /// <param name="action"></param>
        public void AddCommand(string name, Func<string[], string> action)
        {
            AddCommand(name, action, "");
        }

        /// <summary>
        /// Adds a new command to the console
        /// </summary>
        /// <param name="name">Name of the command</param>
        /// <param name="action"></param>
        /// <param name="description"></param>
        public void AddCommand(string name, Func<string[], string> action, string description)
        {
            Commands.Add(new CustomCommand(name, action, description));
        }

        public void ClearCommands()
        {
            consoleComponent.ClearCommands();
        }

        public void KeyDown(Keys keyCode)
        {
            consoleComponent.InputProcessor.KeyDown(keyCode);
        }

        public void CharEntered(char character)
        {
            consoleComponent.InputProcessor.CharEntered(character);
        }

        public void Dispose()
        {
            consoleComponent.ConsoleOpened -= OnConsoleOpened;
            consoleComponent.ConsoleClosed -= OnConsoleClosed;

            _game.Services.RemoveService(typeof(GameConsole));
            _game.Components.Remove(consoleComponent);

            consoleComponent.Dispose();
        }
    }
}