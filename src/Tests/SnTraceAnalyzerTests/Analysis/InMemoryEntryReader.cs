using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceAnalyzerTests.Analysis
{
    public class InMemoryEntryReader : Reader
    {
        IEnumerable<string> _entrySource;

        public InMemoryEntryReader(IEnumerable<string> entrySource)
        {
            _entrySource = entrySource;
        }

        public override IEnumerator<Entry> GetEnumerator()
        {
            var parser = new EntryParser();
            foreach (var item in _entrySource)
            {
                var entry = parser.Parse(item);
                if (entry != null)
                    yield return entry;
            }
        }

        protected override void Dispose(bool disposing)
        {
            // do nothing
        }
    }
}
