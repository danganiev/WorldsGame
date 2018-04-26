using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Entities.Templates;
using WorldsGame.Terrain;
using WorldsGame.Terrain.Blocks.Types;
using WorldsLib;
using XNAGameConsole;

namespace WorldsGame.Playing.ConsoleCommands
{
    internal class SpawnBlockCommand : IConsoleCommand
    {
        public string Name
        {
            get { return "spawn_block"; }
        }

        public string Description
        {
            get { return "Spawns the block."; }
        }

        private readonly World _world;

        public SpawnBlockCommand(World world)
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
                var position = new Vector3i(int.Parse(arguments[1]), int.Parse(arguments[2]), int.Parse(arguments[3]));

                var block = BlockTypeHelper.Get(name);

                if (block == BlockTypeHelper.AIR_BLOCK_TYPE && name != BlockTypeHelper.AIR_BLOCK_TYPE.Name)
                {
                    return "Block with such name wasn't found";                    
                }

                _world.SetBlock(position, BlockTypeHelper.Get(name));

                return "Spawned " + name + " at " + position;
            }
            catch (Exception)
            {

                return "Something went wrong.";
            }
        }
    }
}