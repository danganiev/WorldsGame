using System;
using Microsoft.Xna.Framework;
using WorldsGame.Playing.Items.Inventory;
using WorldsGame.Terrain;
using XNAGameConsole;

namespace WorldsGame.Playing.ConsoleCommands
{
    internal class AddItemCommand : IConsoleCommand
    {
        public string Name
        {
            get { return "add_item"; }
        }

        public string Description
        {
            get { return "Adds item to player inventory"; }
        }

        private readonly World _world;

        public AddItemCommand(World world)
        {
            _world = world;
        }

        public string Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                return "Please enter item name and quantity";
            }
            if (arguments.Length == 1)
            {
                return "Please enter quantity";
            }

            string name = arguments[0];
            int quantity = int.Parse(arguments[1]);

            if (quantity > Inventory.MAX_STACK_COUNT)
            {
                return "No items added. Quantity must not be more than 64.";
            }

            try
            {
                _world.ClientPlayer.AddItem(name, quantity);
            }
            catch (InvalidOperationException e)
            {
                return e.Message;
            }

            return "Added item(s) " + name;
        }
    }
}