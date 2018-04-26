using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAGameConsole.Commands
{
    internal class CustomCommand : IConsoleCommand
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        private readonly Func<string[], string> _action;

        public CustomCommand(string name, Func<string[], string> action, string description)
        {
            Name = name;
            Description = description;
            _action = action;
        }

        public string Execute(string[] arguments)
        {
            return _action(arguments);
        }
    }
}