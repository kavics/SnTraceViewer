using System;
using System.Collections;
using System.Collections.Generic;

namespace SnEventViewer
{
    public abstract class EventLogEntryEnumerable<T> : IDisposable, IEnumerable, IEnumerable<T> where T : EventLogEntry
    {
        public abstract void Dispose();

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
