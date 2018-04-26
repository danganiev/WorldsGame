using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAGameConsole
{
    internal class CommandProcesser
    {
        public string Process(string buffer)
        {
            string commandName = GetCommandName(buffer);
            IConsoleCommand command = GameConsoleOptions.Commands.Where(c => c.Name == commandName).FirstOrDefault();
            var arguments = GetArguments(buffer);
            if (command == null)
            {
                return "ERROR: Command not found";
            }
            string commandOutput;
//            #if !DEBUG
            try
            {
                commandOutput = command.Execute(arguments);
            }
            catch (Exception ex)
            {
                commandOutput = "Something went wrong";
            }
//            #else
//            commandOutput = command.Execute(arguments);
            //#endif
            return commandOutput;
        }

        private static string GetCommandName(string buffer)
        {
            var firstSpace = buffer.IndexOf(' ');
            return buffer.Substring(0, firstSpace < 0 ? buffer.Length : firstSpace);
        }

        private static string[] GetArguments(string buffer)
        {
            var firstSpace = buffer.IndexOf(' ');
            if (firstSpace < 0)
            {
                return new string[0];
            }

            var args = buffer.Substring(firstSpace, buffer.Length - firstSpace).Split(' ');
            return args.Where(a => a != "").ToArray();
        }
    }
}