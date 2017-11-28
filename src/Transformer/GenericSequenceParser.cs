using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformer
{
    public enum SequenceState { Watch, Recording, Finished, Unfinished }

    internal class GenericSequenceParser<Input, Output> : SequenceParser<Input, Output> where Input : Entry where Output : Entry
    {
        private Func<Input, Output> _rootEntrySelector;
        private Func<Input, string> _scanner;
        private Func<Input, Output, SequenceState> _recorder;
        private SequenceState _state;

        private Dictionary<string, Output> _records = new Dictionary<string, Output>();

        public GenericSequenceParser(Func<Input, Output> rootEntrySelector) : base() { }
        public GenericSequenceParser(
            IEnumerable<Input> input,
            Func<Input, Output> rootEntrySelector,
            Func<Input, string> scanner,
            Func<Input, Output, SequenceState> recorder
        ) : base(input)
        {
            _rootEntrySelector = rootEntrySelector;
            _scanner = scanner;
            _recorder = recorder;
        }

        protected override Output Process(Input input)
        {
            Output record;
            string key;

            record = _rootEntrySelector(input);
            key = _scanner(input);
            if (key == null)
                return null;

            if (record != null)
            {
                _records[key] = record;
                return null;
            }

            if (!_records.TryGetValue(key, out record))
                return null;

            var state = _recorder(input, record);
            if (state == SequenceState.Watch)
                throw new InvalidOperationException("Invalid state: the Recorder cannot return with SequenceState.Watch");
            if (state == SequenceState.Recording)
                return null;

            _records.Remove(key);
            return record;
        }
    }
}
