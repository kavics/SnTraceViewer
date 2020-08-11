using SenseNet.Diagnostics.Analysis;
using SenseNet.Tools.CommandLineArguments;
using System;
using System.IO;
using System.Linq;

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

            var currentDirectory = Directory.GetCurrentDirectory();

            var sourceDirectories = _args.SourceDirectory == null
                ? new[] { currentDirectory }
                : _args.SourceDirectory.Split(',').Select(x => Path.GetFullPath(x)).ToArray();

            var targetDirectory = Path.GetFullPath(_args.TargetDirectory ?? currentDirectory);

            var pattern = _args.Pattern ?? "detailedlog_*.log";


            Console.WriteLine("Execute Join.");
            Console.WriteLine("Source: {0}", string.Join(" - ", sourceDirectories));
            Console.WriteLine("Pattern: {0}", pattern);
            Console.WriteLine("Target: {0}", targetDirectory);


            var traceDirs = sourceDirectories.SelectMany(x => TraceDirectory.SearchTraceDirectories(pattern)).ToArray();
            var sessions = TraceSession.Create(traceDirs);
            FileInfo outputFile = null;
            var first = true;
            foreach (var session in sessions)
            {
                if (first || !_args.AllSessions)
                {
                    outputFile = CreateOutputFile(session);
                    first = false;
                }
                foreach(var item in session)
                {
                    Write(item, outputFile);
                }
            }
        }
    }
}
