using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WorldsGame.Gamestates;

using XNAGameConsole;

namespace WorldsGame.Server.ConsoleCommands
{
    internal class SetSpawnPointCommand : IConsoleCommand
    {
        public string Name
        {
            get { return "set_spawn_point"; }
        }

        public string Description
        {
            get { return "Sets the default spawn point for new players."; }
        }

        private readonly ServerState _serverState;

        public SetSpawnPointCommand(ServerState serverState)
        {
            _serverState = serverState;
        }

        public string Execute(string[] arguments)
        {
            var newPosition = new Vector3(float.Parse(arguments[0]), float.Parse(arguments[1]), float.Parse(arguments[2]));
            _serverState.SpawnPoint = newPosition;
            return "New spawn point at " + newPosition;
        }
    }
}