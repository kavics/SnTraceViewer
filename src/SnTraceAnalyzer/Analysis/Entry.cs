using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// Represents a line in the trace file.
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Field index helper
        /// </summary>
        public enum Field
        {
            /// <summary>Value = 0</summary>
            LineId = 0,
            /// <summary>Value = 1</summary>
            Time,
            /// <summary>Value = 2</summary>
            Category,
            /// <summary>Value = 3</summary>
            AppDomain,
            /// <summary>Value = 4</summary>
            ThreadId,
            /// <summary>Value = 5</summary>
            OpId,
            /// <summary>Value = 6</summary>
            Status,
            /// <summary>Value = 7</summary>
            Duration,
            /// <summary>Value = 8</summary>
            Message
        }

        private bool _blockStart;
        /// <summary>
        /// True if this line is the first in the block that written to disk in one step.
        /// </summary>
        public bool BlockStart { get => _blockStart; set { _blockStart = value; _raw = null; } }

        private int _lineId;
        /// <summary>
        /// Identifier number of the line. Unique is in the AppDomain.
        /// </summary>
        public int LineId { get => _lineId; set { _lineId = value; _raw = null; } }

        private DateTime _time;
        /// <summary>
        /// Creation time of the line.
        /// </summary>
        public DateTime Time { get => _time; set { _time = value; _raw = null; } }

        private string _category;
        /// <summary>
        /// Trace category
        /// </summary>
        public string Category { get => _category; set { _category = value; _raw = null; } }

        private string _appDomain;
        /// <summary>
        /// AppDomain name
        /// </summary>
        public string AppDomain { get => _appDomain; set { _appDomain = value; _raw = null; } }

        private int _threadId;
        /// <summary>
        /// Current thread id.
        /// </summary>
        public int ThreadId { get => _threadId; set { _threadId = value; _raw = null; } }

        private int _opId;
        /// <summary>
        /// Id of the operation
        /// </summary>
        public int OpId { get => _opId; set { _opId = value; _raw = null; } }

        private string _status;
        /// <summary>
        /// Value can be empty, "Start", "End", "UNTERMINATED" or "ERROR"
        /// </summary>
        public string Status { get => _status; set { _status = value; _raw = null; } }

        private TimeSpan _duration;
        /// <summary>
        /// Duration if this line is the end of an operation
        /// </summary>
        public TimeSpan Duration { get => _duration; set { _duration = value; _raw = null; } }

        private string _message;
        /// <summary>
        /// The subject of the line
        /// </summary>
        public string Message { get => _message; set { _message = value; _raw = null; } }

        private object _sync = new object();
        private string _raw;

        /// <summary>
        /// Original line data.
        /// </summary>
        public string Raw
        {
            get
            {
                if (_raw == null)
                    lock (_sync)
                        if (_raw == null)
                            _raw = $"{(BlockStart ? ">" : "")}" +
                                   $"{LineId}\t" +
                                   $"{Time:yyyy-MM-dd HH:mm:ss.fffff}\t" +
                                   $"{Category}\t" +
                                   $"A:{AppDomain}\t" +
                                   $"T:{ThreadId}\t" +
                                   $"{(OpId == 0 ? "" : "Op:" + OpId)}\t" +
                                   $"{Status}\t" +
                                   $"{(Duration == default(TimeSpan) ? "" : Duration.ToString())}\t" +
                                   $"{Message}";
                return _raw;
            }
        }

        public Dictionary<string, Entry> Associations { get; set; }

        private Entry() { }
        public Entry(Entry sourceEntry)
        {
            CopyPropertiesFrom(sourceEntry);
        }

        /// <summary>
        /// Creates an SnTraceEntry from one line of the trace file
        /// </summary>
        public static Entry Parse(string oneLine)
        {
            // 0        1                           2       3           4       5       6   7               8
            // >11929	2016-04-07 01:59:57.42589	Index	A:/LM..231	T:46	Op:2743	End	00:00:00.000000	IAQ: A160064 EXECUTION.

            if (string.IsNullOrEmpty(oneLine))
                return null;
            if (oneLine.StartsWith("--") || oneLine.StartsWith("MaxPdiff:", StringComparison.OrdinalIgnoreCase))
                return null;

            var data = oneLine.Split('\t');
            if (data.Length < (int)Field.Message)
                return null;

            return new Entry
            {
                BlockStart = ParseBlockStart(data[(int)Field.LineId]),
                LineId = ParseLineId(data[(int)Field.LineId]),
                Time = ParseTime(data[(int)Field.Time]),
                Category = data[(int)Field.Category],
                AppDomain = ParseAppDomain(data[(int)Field.AppDomain]),
                ThreadId = ParseThread(data[(int)Field.ThreadId]),
                OpId = ParseOperationId(data[(int)Field.OpId]),
                Status = data[(int)Field.Status],
                Duration = ParseDuration(data[(int)Field.Duration]),
                Message = string.Join("\t", data.Skip((int)Field.Message)),
                _raw = oneLine,
            };
        }

        private static bool ParseBlockStart(string src)
        {
            if (string.IsNullOrEmpty(src))
                return false;
            return src[0] == '>';
        }
        private static int ParseLineId(string src)
        {
            if (string.IsNullOrEmpty(src))
                return 0;
            if (src.StartsWith(">"))
                src = src.Substring(1);
            return int.Parse(src);
        }
        private static DateTime ParseTime(string src)
        {
            return DateTime.Parse(src, CultureInfo.InvariantCulture);
        }
        private static string ParseAppDomain(string src)
        {
            return src.StartsWith("A:") ? src.Substring(2) : src;
        }
        private static int ParseThread(string src)
        {
            if (src.StartsWith("T:"))
                src = src.Substring(2);
            return src.Length == 0 ? default(int) : int.Parse(src);
        }
        private static int ParseOperationId(string src)
        {
            if (src.StartsWith("Op:"))
                src = src.Substring(3);
            return src.Length == 0 ? default(int) : int.Parse(src);
        }
        private static TimeSpan ParseDuration(string src)
        {
            return src.Length == 0 ? default(TimeSpan) : TimeSpan.Parse(src, CultureInfo.InvariantCulture);
        }

        protected void CopyPropertiesFrom(Entry fromEntry)
        {
            _blockStart = fromEntry.BlockStart;
            _lineId = fromEntry.LineId;
            _time = fromEntry.Time;
            _category = fromEntry.Category;
            _appDomain = fromEntry.AppDomain;
            _threadId = fromEntry.ThreadId;
            _opId = fromEntry.OpId;
            _status = fromEntry.Status;
            _duration = fromEntry.Duration;
            _message = fromEntry.Message;
            _raw = fromEntry.Raw;
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return Raw;
        }

    }
}
