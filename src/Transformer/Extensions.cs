using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformer
{
    static class Extensions
    {
        public static EntryEnumerable<Q> ParseSequence<T, Q>(this EntryEnumerable<T> input,
            Func<T, Q> startEntrySelector,
            Func<T, string> scanner,
            Func<T, Q, SequenceState> recorder) where T : Entry where Q : Entry
        {
            return new GenericSequenceParser<T, Q>(input, startEntrySelector, scanner, recorder);
        }
        public static EntryEnumerable<Q> SequenceParser<T, Q>(this EntryEnumerable<T> input,
            SequenceParser<T, Q> instance) where T : Entry where Q : Entry
        {
            instance.Initialize(input);
            return instance;
        }
    }
}
