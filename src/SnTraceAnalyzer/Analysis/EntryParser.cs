using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    internal class EntryParser
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

        /// <summary>
        /// Creates an SnTraceEntry from one line of the trace file
        /// </summary>
        public Entry Parse(string oneLine)
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
                Raw = oneLine,
                BlockStart = ParseBlockStart(data[(int)Field.LineId]),
                LineId = ParseLineId(data[(int)Field.LineId]),
                Time = ParseTime(data[(int)Field.Time]),
                Category = data[(int)Field.Category],
                AppDomain = ParseAppDomain(data[(int)Field.AppDomain]),
                ThreadId = ParseThread(data[(int)Field.ThreadId]),
                OpId = ParseOperationId(data[(int)Field.OpId]),
                Status = data[(int)Field.Status],
                Duration = ParseDuration(data[(int)Field.Duration]),
                Message = string.Join("\t", data.Skip((int)Field.Message))
            };
        }

        private bool ParseBlockStart(string src)
        {
            if (string.IsNullOrEmpty(src))
                return false;
            return src[0] == '>';
        }
        private int ParseLineId(string src)
        {
            if (string.IsNullOrEmpty(src))
                return 0;
            if (src.StartsWith(">"))
                src = src.Substring(1);
            return int.Parse(src);
        }
        private DateTime ParseTime(string src)
        {
            return DateTime.Parse(src, CultureInfo.InvariantCulture);
        }
        private string ParseAppDomain(string src)
        {
            return src.StartsWith("A:") ? src.Substring(2) : src;
        }
        private int ParseThread(string src)
        {
            if (src.StartsWith("T:"))
                src = src.Substring(2);
            return src.Length == 0 ? default(int) : int.Parse(src);
        }
        private int ParseOperationId(string src)
        {
            if (src.StartsWith("Op:"))
                src = src.Substring(3);
            return src.Length == 0 ? default(int) : int.Parse(src);
        }
        private TimeSpan ParseDuration(string src)
        {
            return src.Length == 0 ? default(TimeSpan) : TimeSpan.Parse(src, CultureInfo.InvariantCulture);
        }
    }
}
