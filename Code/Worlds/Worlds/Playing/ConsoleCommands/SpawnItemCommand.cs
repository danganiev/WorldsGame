using System;

using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Terrain;
using XNAGameConsole;

namespace WorldsGame.Playing.ConsoleCommands
{
    internal class SpawnItemCommand : IConsoleCommand
    {
        public string Name
        {
            get { return "spawn_item"; }
        }

        public string Description
        {
            get { return "Spawns an item."; }
        }

        private readonly World _world;

        public SpawnItemCommand(World world)
        {
            _world = world;
        }

        public string Execute(string[] arguments)
        {
            try
            {
                if (arguments.Length != 5)
                {
                    return "Something went wrong.";
                }
                string name = arguments[0];
                int quantity = int.Parse(arguments[1]);
                var position = new Vector3(float.Parse(arguments[2]), float.Parse(arguments[3]),
                                           float.Parse(arguments[4]));

                ItemEntityTemplate.Instance.BuildEntity(_world.EntityWorld, name, quantity, position);

                return "Spawned " + name + " at " + position;
            }
            catch (Exception)
            {

                return "Something went wrong.";
            }
        }
    }
}