using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewer.Analysis
{
    public class TraceDirectory
    {
        public List<TraceFile> TraceFiles { get; }
        public string Path { get; }

        public TraceDirectory(string path)
        {
            Path = path;
            var pattern = TraceFile.SearchPattern ?? TraceFile.DefaultSearchPattern;
            TraceFiles = Directory.GetFiles(path, pattern).Select(p => new TraceFile(p)).ToList();
        }


        /// <summary>
        /// Discovers the given directory and returns with full paths of subdirectories that contain files matching the pattern.
        /// </summary>
        /// <param name="rootPath">Full path of the root directory.</param>
        /// <param name="filter">File name pattern. Default: "detailedlog_*.log".</param>
        public static TraceDirectory[] SearchTraceDirectories(string rootPath, string filter = null)
        {
            var paths = new List<TraceDirectory>();
            var pattern = filter ?? "detailedlog_*.log";
            CollectPaths(rootPath, pattern, paths);
            return paths.ToArray();
        }
        private static void CollectPaths(string rootPath, string pattern, List<TraceDirectory> paths)
        {
            if (Directory.GetFiles(rootPath, pattern).Any())
                paths.Add(new TraceDirectory(rootPath));
            foreach (var subPath in Directory.GetDirectories(rootPath))
                CollectPaths(subPath, pattern, paths);
        }
        public static string GetFullPath(string relativePath)
        {
            var x = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
            return x;
        }

    }
}
