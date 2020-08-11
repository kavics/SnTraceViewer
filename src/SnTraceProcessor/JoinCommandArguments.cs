using SenseNet.Tools.CommandLineArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceProcessor
{
    internal class JoinCommandArguments
    {
        [CommandLineArgument(aliases: "S,Source", helpText: "Comma separated directory paths containing input files. Default: the current directory.")]
        public string SourceDirectory { get; set; }

        [CommandLineArgument(aliases: "P,Pattern", helpText: "Pattern of the input files. Default: \"detailedlog_*.log\"")]
        public string Pattern { get; set; }

        [CommandLineArgument(aliases: "R,Recursive", helpText: "Search files recursive.")]
        public bool Recursive { get; set; }

        [CommandLineArgument(aliases: "T,Target", helpText: "Directory path of the output files. Default: the current directory.")]
        public string TargetDirectory { get; set; }

        [CommandLineArgument(aliases: "A,All", helpText: "Create one file with all sessions (originally separates by sessions).")]
        public bool AllSessions { get; set; }

        [CommandLineArgument(aliases: "L,Last", helpText: "Process only the last session (originally all sessions processed).")]
        public bool LastSession { get; set; }

        //[CommandLineArgument(aliases: "D,Delete", helpText: "Delete processed files (originally all source files are kept).")]
        //public bool DeleteProcessedFiles { get; set; }

        //[CommandLineArgument(aliases: "DA,DeleteAll", helpText: "Delete all source files (in depth if the \"Recursive\" is active).")]
        //public bool DeleteAllFiles { get; set; }

    }
}
