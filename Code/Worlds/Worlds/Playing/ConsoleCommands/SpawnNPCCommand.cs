using System;

using Microsoft.Xna.Framework;
using WorldsGame.Gamestates;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Terrain;
using XNAGameConsole;

namespace WorldsGame.Playing.ConsoleCommands
{
    internal class SpawnNPCCommand : IConsoleCommand
    {
        public string Name
        {
            get { return "spawn_npc"; }
        }

        public string Description
        {
            get { return "Spawns the NPC."; }
        }

        private readonly World _world;

        public SpawnNPCCommand(World world)
        {
            _world = world;
        }

        public string Execute(string[] arguments)
        {
            try
            {
                if (arguments.Length != 4)
                {
                    return "Something went wrong.";
                }

                string name = arguments[0];
                var position = new Vector3(float.Parse(arguments[1]), float.Parse(arguments[2]),
                                           float.Parse(arguments[3]));

                NPCEntityTemplate.Instance.BuildEntity(_world.EntityWorld, name, position);

                return "Spawned " + name + " at " + position;
            }
            catch (Exception)
            {
                return "Something went wrong.";
            }
        }
    }
}