using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SnEventViewer
{
    [DebuggerDisplay("{EventLogFiles.Count} files: {Path}")]
    public class EventLogDirectory : EventLogEntryEnumerable<EventLogEntry>
    {
        public List<EventLogFile> EventLogFiles { get; }
        public string Path { get; }

        public EventLogDirectory(string path)
        {
            Path = path;
            var pattern = EventLogFile.SearchPattern ?? EventLogFile.DefaultSearchPattern;
            EventLogFiles = Directory.GetFiles(path, pattern).Select(p => new EventLogFile(p)).ToList();
        }


        /// <summary>
        /// Discovers the given directory and returns with full paths of subdirectories that contain files matching the pattern.
        /// </summary>
        /// <param name="rootPath">Full path of the root directory.</param>
        /// <param name="filter">File name pattern. Default: "eventlog_*.log".</param>
        public static EventLogDirectory[] SearchEventLogDirectories(string rootPath, string filter = null)
        {
            var paths = new List<EventLogDirectory>();
            var pattern = filter ?? EventLogFile.DefaultSearchPattern;
            CollectPaths(rootPath, pattern, paths);
            return paths.ToArray();
        }
        private static void CollectPaths(string rootPath, string pattern, List<EventLogDirectory> paths)
        {
            if (Directory.GetFiles(rootPath, pattern).Any())
                paths.Add(new EventLogDirectory(rootPath));
            foreach (var subPath in Directory.GetDirectories(rootPath))
                CollectPaths(subPath, pattern, paths);
        }
        public static string GetFullPath(string relativePath)
        {
            var x = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
            return x;
        }

        public override void Dispose()
        {
        }

        public override IEnumerator<EventLogEntry> GetEnumerator()
        {
            foreach (var eventLogFile in EventLogFiles)
                foreach (var entry in eventLogFile)
                    yield return entry;
        }
    }
}
