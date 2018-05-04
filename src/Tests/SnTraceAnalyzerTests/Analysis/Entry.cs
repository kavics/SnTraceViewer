using System;
using System.Globalization;
using System.Linq;

#pragma warning disable 1591

namespace SnTraceAnalyzerTests.Analysis
{
    /// <summary>
    /// Represents a line in the trace file.
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// True if this line is the first in the block that written to disk in one step.
        /// </summary>
        public bool BlockStart;
        /// <summary>
        /// Identifier number of the line. Unique is in the AppDomain.
        /// </summary>
        public int LineId;
        /// <summary>
        /// Creation time of the line.
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// Trace category
        /// </summary>
        public string Category;
        /// <summary>
        /// AppDomain name
        /// </summary>
        public string AppDomain;
        /// <summary>
        /// Current thread id.
        /// </summary>
        public int ThreadId;
        /// <summary>
        /// Id of the operation
        /// </summary>
        public int OpId;
        /// <summary>
        /// Value can be empty, "Start", "End", "UNTERMINATED" or "ERROR"
        /// </summary>
        public string Status;
        /// <summary>
        /// Duration if this line is the end of an operation
        /// </summary>
        public TimeSpan Duration;
        /// <summary>
        /// The subject of the line
        /// </summary>
        public string Message;
        /// <summary>
        /// Original line data.
        /// </summary>
        public string Raw;

        public Entry() { }
        public Entry(Entry sourceEntry)
        {
            CopyPropertiesFrom(sourceEntry);
        }

        protected void CopyPropertiesFrom(Entry fromEntry)
        {
            BlockStart = fromEntry.BlockStart;
            LineId = fromEntry.LineId;
            Time = fromEntry.Time;
            Category = fromEntry.Category;
            AppDomain = fromEntry.AppDomain;
            ThreadId = fromEntry.ThreadId;
            OpId = fromEntry.OpId;
            Status = fromEntry.Status;
            Duration = fromEntry.Duration;
            Message = fromEntry.Message;
            Raw = fromEntry.Raw;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            var block = BlockStart ? ">" : "";
            var time = Time.ToString("yyyy-MM-dd HH:mm:ss.fffff");
            var op = OpId > 0 ? "Op:" + OpId : "";
            return $"{block}{LineId}\t{time}\t{Category}\tA:{AppDomain}\tT:{ThreadId}\t{op}\t{Status}\t{Message}";
        }

    }
}
