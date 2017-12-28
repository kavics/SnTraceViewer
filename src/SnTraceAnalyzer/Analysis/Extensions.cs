using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    public static class Extensions
    {
        public static IEnumerable<TResult> Collect<TSource, TResult>(this IEnumerable<TSource> input, Func<TSource, Tuple<string, string>> keySelector, Func<TResult, TResult> finalizer = null) where TSource : Entry where TResult : EntryCollection<TSource>, new()
        {
            var collector = new GenericCollector<TSource, TResult>(keySelector, finalizer);
            collector.Initialize(input);
            return collector;
        }

        public static IEnumerable<object> Collect2<TSource>(this IEnumerable<TSource> input, Func<TSource, Tuple<string, string>> keySelector, Func<dynamic, dynamic> finalizer) //where TSource : Entry
        {
            var collector = new GenericCollector2<TSource>(keySelector, finalizer);
            collector.Initialize(input);
            return collector;
        }

        public static IEnumerable<Statistics<long>> Statisctics<TSource>(this IEnumerable<TSource> input, Func<TSource, string> keySelector, Func<TSource, long> valueSelector)
        {
            var aggregator = new GenericAggregator<TSource, long>(keySelector, valueSelector);
            aggregator.Initialize(input);
            return aggregator;
        }
    }
}
