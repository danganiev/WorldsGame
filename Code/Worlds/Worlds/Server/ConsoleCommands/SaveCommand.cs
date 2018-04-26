using Microsoft.Xna.Framework;
using WorldsGame.Gamestates;
using XNAGameConsole;

namespace WorldsGame.Server.ConsoleCommands
{
    internal class SaveCommand : IConsoleCommand
    {
        public string Name
        {
            get { return "save"; }
        }

        public string Description
        {
            get { return "Saves current world as it is."; }
        }

        private readonly ServerState _serverState;

        public SaveCommand(ServerState serverState)
        {
            _serverState = serverState;
        }

        public string Execute(string[] arguments)
        {
            _serverState.Save();
            return "Saving initiated.";
        }
    }
}