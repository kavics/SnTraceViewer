using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SnEventViewer
{
    [DebuggerDisplay("{FirstEntry.Timestamp.TimeOfDay.ToString()}-{LastEntry.Timestamp.TimeOfDay.ToString()}: {FirstEntry.AppDomain} {Path}")]
    public class EventLogFile : EventLogEntryEnumerable<EventLogEntry>
    {
        public static readonly string DefaultSearchPattern = "eventlog_*.log";
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

        private EventLogEntry _firstEntry;
        public EventLogEntry FirstEntry
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _firstEntry;
            }
        }

        private EventLogEntry _lastEntry;
        public EventLogEntry LastEntry
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _lastEntry;
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

        private int _warnings;
        public int Warnings
        {
            get
            {
                if (!_scanned)
                    Scan();
                return _warnings;
            }
        }

        public EventLogFile(string path)
        {
            Path = path;
        }

        private object _scanSync = new object();
        internal void Scan()
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
                        _errors = entries.Count(e => e.Severity == TraceEventType.Error || e.Severity == TraceEventType.Critical);
                        _warnings = entries.Count(e => e.Severity == TraceEventType.Warning);
                    }
                    _scanned = true;
                }
            }
        }

        public override void Dispose()
        {
        }

        public override IEnumerator<EventLogEntry> GetEnumerator()
        {
            string line;
            var block = new StringBuilder();
            using (var reader = GetReader(Path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        if (!line.StartsWith("--"))
                            block.AppendLine(line);
                    }
                    else
                    {
                        var entry = EventLogEntry.Parse(block.ToString());
                        if (entry != null)
                            yield return entry;
                        block.Clear();
                    }
                }
            }

            if (block.Length > 0)
            {
                yield return EventLogEntry.Parse(block.ToString());
            }
        }
        public virtual TextReader GetReader(string path)
        {
            return new StreamReader(path);
        }
    }
}
