using SenseNet.Tools.CommandLineArguments;
using System;
using System.IO;

namespace SnTraceProcessor
{
    internal class JoinCommand : ICommand
    {
        public string ShortInfo => "Concatenates log files from one or more sources.";

        public TextReader In { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TextWriter Out { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandContext Context { get; set; }

        private JoinCommandArguments _args;
        public void Execute()
        {
            try
            {
                if (!Context.ParseArguments<JoinCommandArguments>(out _args))
                    return;
            }
            catch (ParsingException e)
            {
                Console.WriteLine(e.FormattedMessage);
                return;
            }

            Console.WriteLine("Execute Join.");
        }
    }
}
