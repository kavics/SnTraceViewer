using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// Represents a file section that created by one AppDomain in a row.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class TraceSession : EntryEnumerable<Entry>
    {
        private List<TraceFile> _files = new List<TraceFile>();
        public TraceFile[] Files => _files.ToArray();

        public string AppDomain { get; private set; }
        public int FirstLineId { get; private set; }
        public int LastLineId { get; private set; }
        public DateTime FirstTime { get; private set; }
        public DateTime LastTime { get; private set; }

        public static TraceSession[] Create(TraceDirectory[] traceDirs)
        {
            var allFiles = traceDirs
                .SelectMany(d => d.TraceFiles, (d, f) => f)
                .Where(f => f.FirstEntry != null)
                .OrderBy(f => f.FirstEntry.AppDomain)
                .ThenBy(f => f.FirstEntry.Time)
                .ToArray();

            //  1-542     | "01:13:42.2279700"-"01:14:22.2085500": "UnitTestAdapter: Running test"	SnTraceViewer.Analysis.TraceFile
            //  543-1109  | "01:14:22.3208800"-"01:14:25.2267600": "UnitTestAdapter: Running test"	SnTraceViewer.Analysis.TraceFile
            //  1110-1595 | "01:14:25.2307700"-"01:14:27.3200000": "UnitTestAdapter: Running test"	SnTraceViewer.Analysis.TraceFile
            //  1-21      | "06:20:08.7282800"-"06:20:25.6107500": "UnitTestAdapter: Running test"	SnTraceViewer.Analysis.TraceFile

            var sessions = new List<TraceSession>();
            foreach(var file in allFiles)
            {
                var session = sessions.Where(s =>
                        s.LastLineId == file.FirstEntry.LineId - 1 && 
                        s.AppDomain == file.FirstEntry.AppDomain &&
                        (file.FirstEntry.Time - s.LastTime) < TimeSpan.FromMinutes(1))
                    .FirstOrDefault();
                if (session == null)
                    sessions.Add(session = new TraceSession());
                session.AddFile(file);
            }

            return sessions.ToArray();
        }
        private void AddFile(TraceFile file)
        {
            if (_files.Count == 0)
            {
                AppDomain = file.FirstEntry.AppDomain;
                FirstLineId = file.FirstEntry.LineId;
                FirstTime = file.FirstEntry.Time;
            }

            _files.Add(file);

            LastLineId = file.LastEntry.LineId;
            LastTime = file.LastEntry.Time;
        }

        public override void Dispose()
        {
            foreach(var file in _files)
                file.Dispose();
            _files.Clear();

            FirstLineId = 0;
            LastLineId = 0;
            FirstTime = DateTime.MinValue;
            LastTime = DateTime.MinValue;
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            foreach (var file in _files)
                foreach (var entry in file)
                    yield return entry;
        }

        public override string ToString()
        {
            var now = DateTime.Now;
            var duration = LastTime - FirstTime;
            return $"{_files.Count} files | {FirstTime.ToDisplayString(now)} | {duration.ToDisplayString()}";
        }
    }
}
