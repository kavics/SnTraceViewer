using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewer.Analysis
{
    [DebuggerDisplay("{FirstEntry.LineId}-{LastEntry.LineId} | {FirstEntry.Time.TimeOfDay.ToString()}-{LastEntry.Time.TimeOfDay.ToString()}: {FirstEntry.AppDomain}")]
    public class TraceFile : EntryEnumerable<Entry>
    {
        public static readonly string DefaultSearchPattern = "detailedlog_*.log";
        public static string SearchPattern { get; } = DefaultSearchPattern;

        public string Path { get; }

        private bool _scanned;

        private int _entryCount;
        public int EntryCount
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _entryCount;
            }
        }

        private Entry _firstEntry;
        public Entry FirstEntry
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _firstEntry;
            }
        }

        private Entry _lastEntry;
        public Entry LastEntry
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _lastEntry;
            }
        }

        private string[] _categories;
        public string[] Categories
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _categories;
            }
        }

        private int _errors;
        public int Errors
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _errors;
            }
        }

        private int _unterminatedLines;
        public int UnterminatedLines
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _unterminatedLines;
            }
        }

        public TraceFile(string path)
        {
            Path = path;
        }

        private object _scanSync = new object();
        private void Scan()
        {
            lock (_scanSync)
            {
                if (!_scanned)
                {
                    var entries = this.ToArray();
                    _entryCount = entries.Length;
                    if (_entryCount > 0)
                    {
                        _firstEntry = entries[0];
                        _lastEntry = entries[_entryCount - 1];
                        _categories = entries.Select(e => e.Category).OrderBy(c => c).Distinct().ToArray();
                        _errors = entries.Count(e => e.Status == "ERROR");
                        _unterminatedLines = entries.Count(e => e.Status == "UNTERMINATED");
                    }
                    _scanned = true;
                }
            }
        }

        public override void Dispose()
        {
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            string line;
            using (var reader = new StreamReader(Path))
                while ((line = reader.ReadLine()) != null)
                    if (line.Length > 0 && !line.StartsWith("--") && !line.StartsWith("MaxPdiff:"))
                        yield return Entry.Parse(line);
        }
    }
}
