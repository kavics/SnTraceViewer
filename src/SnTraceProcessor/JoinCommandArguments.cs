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
        [CommandLineArgument(aliases: "A,All,AllSessions", helpText: "Create one file with all sessions (originally separates by sessions).")]
        public bool AllSessions { get; set; }

        [CommandLineArgument(aliases: "L,Last,LastSession", helpText: "Process only the last session (originally all sessions processed).")]
        public bool LastSession { get; set; }

        [CommandLineArgument(aliases: "D,Delete", helpText: "Delete processed files (originally all source files are kept).")]
        public bool DeleteOriginalFiles { get; set; }

        [CommandLineArgument(aliases: "DA,DeleteAll", helpText: "Delete all source files (originally all source files are kept).")]
        public bool DeleteAllFiles { get; set; }

        // S Source: Source directory (originally the current directory)
        // F FileName: File pattern (original pattern: "detailedlog*.log")
        // R Recursive: Search in deep (originally use the current directory)
        // T Target: Target directory (originally create joined file(s) to the source directory)

    }
}
