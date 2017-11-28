using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformer
{
    internal abstract class SequenceParser<Input, Output> : EntryEnumerable<Output> where Input : Entry where Output : Entry
    {
        private IEnumerable<Input> _input;

        public SequenceParser()
        {
        }
        public SequenceParser(IEnumerable<Input> input)
        {
            _input = input;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            var disposable = _input as IDisposable;
            disposable?.Dispose();
        }

        public override IEnumerator<Output> GetEnumerator()
        {
            Output output;
            foreach (var item in _input)
                if ((output = Process(item)) != null)
                    yield return output;
        }

        protected abstract Output Process(Input input);

        public void Initialize(IEnumerable<Input> input)
        {
            _input = input;
        }
    }
}
