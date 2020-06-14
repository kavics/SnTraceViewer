using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SnEventViewer
{
    /// <summary>
    /// Represents an event-log entry
    /// EXPERIMENTAL FEATURE
    /// </summary>
    public class EventLogEntry
    {
        private static class Field
        {
            public const string Timestamp = "Timestamp";
            public const string Message = "Message";
            public const string Category = "Category";
            public const string Priority = "Priority";
            public const string EventId = "EventId";
            public const string Severity = "Severity";
            public const string Title = "Title";
            public const string Machine = "Machine";
            public const string AppDomain = "Application Domain";
            public const string ProcessId = "Process Id";
            public const string ProcessName = "Process Name";
            public const string ThreadId = "Managed Thread Id";
            public const string ThreadName = "Thread Name";
            public const string ExtendedProperties = "Extended Properties";
        }

        private DateTime _timestamp;
        public DateTime Timestamp { get => _timestamp; set { _timestamp = value; _raw = null; } }

        private string _message;
        public string Message { get => _message; set { _message = value; _raw = null; } }

        private string _category;
        public string Category { get => _category; set { _category = value; _raw = null; } }

        private int _priority;
        public int Priority { get => _priority; set { _priority = value; _raw = null; } }

        private int _eventId;
        public int EventId { get => _eventId; set { _eventId = value; _raw = null; } }

        private TraceEventType _severity;
        public TraceEventType Severity { get => _severity; set { _severity = value; _raw = null; } }

        private string _title;
        public string Title { get => _title; set { _title = value; _raw = null; } }

        private string _machine;
        public string Machine { get => _machine; set { _machine = value; _raw = null; } }

        private string _appDomain;
        public string AppDomain { get => _appDomain; set { _appDomain = value; _raw = null; } }

        private int _processId;
        public int ProcessId { get => _processId; set { _processId = value; _raw = null; } }

        private string _processName;
        public string ProcessName { get => _processName; set { _processName = value; _raw = null; } }

        private int _threadId;
        public int ThreadId { get => _threadId; set { _threadId = value; _raw = null; } }

        private string _threadName;
        public string ThreadName { get => _threadName; set { _threadName = value; _raw = null; } }

        private IDictionary<string, string> _extendedProperties = new Dictionary<string, string>();
        public IDictionary<string, string> ExtendedProperties => _extendedProperties;

        private object _sync = new object();
        private string _raw;

        /// <summary>
        /// Original data.
        /// </summary>
        public string Raw
        {
            get
            {
                if (_raw == null)
                    lock (_sync)
                        if (_raw == null)
                            _raw = $"{Field.Timestamp}: {Timestamp:yyyy-MM-dd HH:mm:ss.fffff}\r\n" +
                                   $"{Field.Message}: {Message}\r\n" +
                                   $"{Field.Category}: {Category}\r\n" +
                                   $"{Field.Priority}: {Priority}\r\n" +
                                   $"{Field.EventId}: {EventId}\r\n" +
                                   $"{Field.Severity}: {Severity}\r\n" +
                                   $"{Field.Title}: {Title}\r\n" +
                                   $"{Field.Machine}: {Machine}\r\n" +
                                   $"{Field.AppDomain}: {AppDomain}\r\n" +
                                   $"{Field.ProcessId}: {ProcessId}\r\n" +
                                   $"{Field.ProcessName}: {ProcessName}\r\n" +
                                   $"{Field.ThreadId}: {ThreadId}\r\n" +
                                   $"{Field.ThreadName}: {ThreadName}\r\n" +
                                   $"{Field.ExtendedProperties}: {ExtendedProperties}\r\n";
                return _raw;
            }
        }

        public Dictionary<string, EventLogEntry> Associations { get; set; }

        private EventLogEntry() { }
        public EventLogEntry(EventLogEntry sourceEntry)
        {
            CopyPropertiesFrom(sourceEntry);
        }

        public static EventLogEntry Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;
            if (raw.StartsWith("--"))
                return null;

            var entry = new EventLogEntry();

            using (var reader = new StringReader(raw))
            {
                string line;
                var extension = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (extension)
                    {
                        var p = line.IndexOf(" - ", StringComparison.Ordinal);
                        var key = line.Substring(0, p);
                        var value = line.Substring(p + 3).Trim();
                        entry._extendedProperties.Add(key, value);
                    }
                    else
                    {
                        var p = line.IndexOf(':');
                        if (p > 0)
                        {
                            var key = line.Substring(0, p);
                            var value = line.Substring(p + 1).Trim();
                            switch (key)
                            {
                                case Field.Timestamp: entry.Timestamp = ParseDateTime(value); break;
                                case Field.Message: entry.Message = value; break;
                                case Field.Category: entry.Category = value; break;
                                case Field.Priority: entry.Priority = ParseInt(value); break;
                                case Field.EventId: entry.EventId = ParseInt(value); break;
                                case Field.Severity: entry.Severity = ParseSeverity(value); break;
                                case Field.Title: entry.Title = value; break;
                                case Field.Machine: entry.Machine = value; break;
                                case Field.AppDomain: entry.AppDomain = value; break;
                                case Field.ProcessId: entry.ProcessId = ParseInt(value); break;
                                case Field.ProcessName: entry.ProcessName = value; break;
                                case Field.ThreadId: entry.ThreadId = ParseInt(value); break;
                                case Field.ThreadName: entry.ThreadName = value; break;
                                case Field.ExtendedProperties:
                                    extension = true;
                                    p = value.IndexOf(" - ", StringComparison.Ordinal);
                                    key = value.Substring(0, p);
                                    value = value.Substring(p + 3).Trim();
                                    entry._extendedProperties.Add(key, value);
                                    break;
                            }
                        }
                    }
                }
            }

            entry._raw = raw;
            return entry;
        }
        private static DateTime ParseDateTime(string source)
        {
            DateTime.TryParse(source, out var result);
            return result;
        }
        private static int ParseInt(string source)
        {
            int.TryParse(source, out var result);
            return result;
        }
        private static TraceEventType ParseSeverity(string source)
        {
            Enum.TryParse(source, out TraceEventType result);
            return result;
        }

        protected void CopyPropertiesFrom(EventLogEntry fromEntry)
        {
            _timestamp = fromEntry.Timestamp;
            _message = fromEntry.Message;
            _category = fromEntry.Category;
            _priority = fromEntry.Priority;
            _eventId = fromEntry.EventId;
            _severity = fromEntry.Severity;
            _title = fromEntry.Title;
            _machine = fromEntry.Machine;
            _appDomain = fromEntry.AppDomain;
            _processId = fromEntry.ProcessId;
            _processName = fromEntry.ProcessName;
            _threadId = fromEntry.ThreadId;
            _threadName = fromEntry.ThreadName;
            _extendedProperties = new EventExtensionDictionary(Changed);
            foreach (var item in fromEntry.ExtendedProperties)
                _extendedProperties[item.Key] = item.Value;
            _raw = fromEntry.Raw;
        }

        private void Changed()
        {
            _raw = null;
        }
        public override string ToString()
        {
            return Raw;
        }

    }
}
