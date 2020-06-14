using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SnEventViewer
{
    public class DisplayEntry
    {
        private EventLogEntry _entry;

        public DateTime Timestamp => _entry.Timestamp;
        public string Message => _entry.Message;
        public string Category => _entry.Category;
        public int Priority => _entry.Priority;
        public int EventId => _entry.EventId;
        public TraceEventType Severity => _entry.Severity;
        public string Title => _entry.Title;
        public string Machine => _entry.Machine;
        public string AppDomain => _entry.AppDomain;
        public int ProcessId => _entry.ProcessId;
        public string ProcessName => _entry.ProcessName;
        public int ThreadId => _entry.ThreadId;
        public string ThreadName => _entry.ThreadName;
        public IDictionary<string, string> ExtendedProperties => _entry.ExtendedProperties;
        public string Raw => _entry.Raw;


        public string StatusColor { get; set; }
        public string StatusWeight { get; set; }

        public DisplayEntry(EventLogEntry entry)
        {
            _entry = entry;
            switch (entry.Severity)
            {
                default:
                    StatusColor = "#FFFFFF";
                    StatusWeight = "Normal";
                    break;
                case TraceEventType.Information:
                    StatusColor = "#FFFFFF";
                    StatusWeight = "Bold";
                    break;
                case TraceEventType.Warning:
                    StatusColor = "#FFFFFF";
                    StatusWeight = "Bold";
                    break;
                case TraceEventType.Error:
                    StatusColor = "#FFBB99";
                    StatusWeight = "Bold";
                    break;
                case TraceEventType.Critical:
                    StatusColor = "#FFFFBB";
                    StatusWeight = "Bold";
                    break;
            }
        }
    }
}
