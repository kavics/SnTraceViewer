using SenseNet.Tools.CommandLineArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceProcessor
{
    public class CommandContext
    {
        public string[] Args { get; }
        public ICommand Command { get; }

        internal CommandContext(ICommand command, string[] args)
        {
            Command = command;
            Args = args;
        }

        public bool ParseArguments<T>(out T arguments) where T : class, new()
        {
            arguments = new T();

            var result = ArgumentParser.Parse(Args, arguments);

            if (!result.IsHelp)
                return true;

            Console.WriteLine(InsertCommandName(result.GetHelpText(), GetCommandName(Command.GetType()), Command.ShortInfo));
            arguments = null;
            return false;
        }

        private string InsertCommandName(string helpText, string name, string shortInfo)
        {
            var p0 = helpText.IndexOf("Usage:", StringComparison.Ordinal);
            var p1 = helpText.IndexOf("SnTraceProcessor", p0, StringComparison.Ordinal) + 17;
            var start = helpText.Substring(0, p0);
            var mid = helpText.Substring(p0, p1 - p0);
            var end = helpText.Substring(p1);
            return start + "Command: " + name.ToUpperInvariant() + "\r\n" + shortInfo
                  + "\r\n\r\n" + mid + name + " " + end;
        }

        /* ============================================================================= Tools */

        public static string GetCommandName(Type type)
        {
            var name = type.Name;
            return name.EndsWith("Command")
                ? name.Substring(0, name.Length - 7)
                : name;
        }
    }
}