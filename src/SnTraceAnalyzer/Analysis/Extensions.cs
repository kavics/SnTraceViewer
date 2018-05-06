using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis2
{
    public static class Extensions
    {
        public static string ToDisplayString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffff") + (dateTime.Kind == DateTimeKind.Utc ? "Z" : "");
        }
        public static string ToDisplayString(this DateTime dateTime, DateTime now)
        {
            var ci = new CultureInfo("en-US");

            if (now.Year != dateTime.Year)
                return dateTime.ToDisplayString();

            if (now.Month - dateTime.Month > 1)
                return dateTime.ToString("MM-dd HH:mm:ss.fffff", ci);
            if (now.Month - dateTime.Month == 1)
                return "Last month " + dateTime.ToString("dd HH:mm:ss.fffff", ci);

            if (now.Day - dateTime.Day > 6)
                return dateTime.ToString("dd HH:mm:ss.fffff", ci);
            if (now.Day - dateTime.Day > 1)
                return dateTime.ToString("dddd HH:mm:ss.fffff", ci);
            if (now.Day - dateTime.Day == 1)
                return "Yesterday " + dateTime.ToString("HH:mm:ss.fffff", ci);

            return "Today " + dateTime.ToString("HH:mm:ss.fffff", ci);
        }

        ////UNDONE: not serialized.
        //public static IEnumerable<TResult> Collect<TSource, TResult>(this IEnumerable<TSource> input, Func<TSource, Tuple<string, string>> keySelector, Func<TResult, TResult> finalizer = null) where TSource : Entry where TResult : EntryCollection<TSource>, new()
        //{
        //    var collector = new GenericCollector<TSource, TResult>(keySelector, finalizer);
        //    collector.Initialize(input);
        //    return collector;
        //}

        ////UNDONE: not serialized.
        //public static IEnumerable<object> Collect2<TSource>(this IEnumerable<TSource> input, Func<TSource, Tuple<string, string>> keySelector, Func<dynamic, dynamic> finalizer)
        //{
        //    var collector = new GenericCollector2<TSource>(keySelector, finalizer);
        //    collector.Initialize(input);
        //    return collector;
        //}

        ////UNDONE: not serialized.
        //public static IEnumerable<Statistics<long>> Statistics<TSource>(this IEnumerable<TSource> input, Func<TSource, string> keySelector, Func<TSource, long> valueSelector)
        //{
        //    var aggregator = new GenericAggregator<TSource, long>(keySelector, valueSelector);
        //    aggregator.Initialize(input);
        //    return aggregator;
        //}
    }
}
