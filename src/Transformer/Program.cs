using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Transformer
{
    class Program
    {
        private class AppDomainSimplifier
        {
            private readonly string _format;
            private List<string> _keys = new List<string>();

            public AppDomainSimplifier(string format = null)
            {
                _format = format ?? "App-{0}";
            }

            public string Simplify(string key)
            {
                var i = _keys.IndexOf(key);
                if (i < 0)
                {
                    i = _keys.Count;
                    _keys.Add(key);
                }
                return string.Format(_format, (i + 1));
            }
        }
        private class WebRequestEntryCollection : EntryCollection<Entry>
        {
            public static class Q
            {
                public const string Start = "start";
                public const string End = "end";
            }

            public Entry StartEntry;
            public Entry EndEntry;

            public override void Add(Entry e, string qualification)
            {
                switch (qualification)
                {
                    case Q.Start:
                        StartEntry = e;
                        break;
                    case Q.End:
                        EndEntry = e;
                        break;
                }
            }

            public override bool Finished()
            {
                return StartEntry != null && EndEntry != null;
            }
        }

        static void Main(string[] args)
        {
            var logDirectories = new[] {
                @"C:\Users\Gyebi\Desktop\benchmark\02\web1",
                @"C:\Users\Gyebi\Desktop\benchmark\02\web2",
            };
            var outputDir = @"C:\Users\Gyebi\Desktop\benchmark\02\";

            Test1_SimpleRead(logDirectories, outputDir);
            Test2_Filtering(logDirectories, outputDir);
            Test3_Modifying(logDirectories, outputDir);
            Test4_Collecting(logDirectories, outputDir);

            if (Debugger.IsAttached)
            {
                Console.Write("press <enter> to exit...");
                Console.ReadLine();
            }
        }

        private static void Test1_SimpleRead(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(Path.Combine(outputDir, "webrequests1.log")))
            using (var logFlow = Reader.Create(logDirectories, "detailedlog_*.log"))
            {
                var transformedLogFlow = logFlow
                    .Take(3);
                foreach (var item in transformedLogFlow)
                    writer.WriteLine(item);
            }
        }
        private static void Test2_Filtering(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(Path.Combine(outputDir, "webrequests2.log")))
            using (var logFlow = Reader.Create(logDirectories, "detailedlog_*.log")            )
            {
                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web")
                    .Take(3);
                foreach (var item in transformedLogFlow)
                    writer.WriteLine(item);
            }
        }
        private static void Test3_Modifying(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(Path.Combine(outputDir, "webrequests3.log")))
            using (var logFlow = Reader.Create(logDirectories, "detailedlog_*.log"))
            {
                var aps = new AppDomainSimplifier("AppDomain-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web")
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Take(3);
                foreach (var item in transformedLogFlow)
                {
                    writer.WriteLine(item);
                }
            }
        }
        private static void Test4_Collecting(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(Path.Combine(outputDir, "webrequests4.log")))
            using (var logFlow = Reader.Create(logDirectories, "detailedlog_*.log"))
            {
                var aps = new AppDomainSimplifier("AppDomain-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web")
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect<Entry, WebRequestEntryCollection>((e) =>
                    {
                        if (e.Message.StartsWith("PCM.OnEnter "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", WebRequestEntryCollection.Q.Start);
                        else if (e.Message.StartsWith("PCM.OnEndRequest "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", WebRequestEntryCollection.Q.End);
                        return null;
                    })
                    .Take(3);
                foreach (var item in transformedLogFlow)
                {
                    var app = item.StartEntry.AppDomain;
                    var time = item.StartEntry.Time.ToString("HH:mm:ss.fffff");
                    var req = item.StartEntry.Message.Substring("PCM.OnEnter ".Length);
                    var dt = item.EndEntry.Time - item.StartEntry.Time;
                    writer.WriteLine($"{app}\t{time}\t{dt}\t{req}");
                }
            }
        }

        /*
        private static void Test4(string[] logDirectories, string outputDir)
        {
            string key;
            var regex = new Regex("IAQ: A[0-9]+");
            Match regexMatch;
            using (var writer = new StreamWriter(outputDir + "indexing.txt"))
            using (var analyzator = Reader.Create(logDirectories, "detailedlog_*.log")
                .Filter<Entry>(e => e.Category == "Index" || e.Category == "IndexQueue")
                .ParseSequence<Entry, DistributedIndexingActivityEntry>(
                    startEntrySelector: (e) => {
                        if (e.Message.StartsWith("ExecuteDistributedActivity: #"))
                            return new DistributedIndexingActivityEntry(e);
                        return null;
                    },
                    scanner: (e) => {
                        if (e.Message.StartsWith("ExecuteDistributedActivity: #"))
                        {
                            key = e.Message.Replace("ExecuteDistributedActivity: #", "A");
                            return key;
                        }

                        regexMatch = regex.Match(e.Message);
                        if (regexMatch.Success)
                        {
                            key = (regexMatch.Value.Substring("IAQ: ".Length));
                            return key;
                        }

                        return null;
                    },
                    recorder: (entry, record) => {
                        if (entry.Message.Contains("arrived from another computer."))
                        {
                            record.ArrivedOnOther = entry;
                        }
                        else if (entry.Message.EndsWith("dequeued."))
                        {
                            if (entry.AppDomain == record.AppDomain)
                                record.DequeuedOnSame = entry;
                            else
                                record.DequeuedOnOther = entry;
                        }
                        else if (entry.Message.EndsWith("EXECUTION."))
                        {
                            if (entry.AppDomain == record.AppDomain)
                            {
                                if (entry.Status == "Start")
                                    record.StartOnSame = entry;
                                else
                                    record.EndOnSame = entry;
                            }
                            else
                            {
                                if (entry.Status == "Start")
                                    record.StartOnOther = entry;
                                else
                                    record.EndOnOther = entry;
                            }
                        }

                        return (record.EndOnSame != null && record.EndOnOther != null)
                            ? SequenceState.Finished
                            : SequenceState.Recording;
                    }
                )
                .Transform<Entry>((r) => {
                    if (r.ArrivedOnOther == null)
                    {
                        int q = 1;
                    }
                    var t0 = (r.DequeuedOnSame.Time - r.Time).Milliseconds;
                    var t1 = (r.StartOnSame.Time - r.DequeuedOnSame.Time).Milliseconds;
                    var t2 = (r.EndOnSame.Time - r.StartOnSame.Time).Milliseconds;
                    var t3 = (r.ArrivedOnOther.Time - r.Time).Milliseconds;
                    var t4 = (r.DequeuedOnOther.Time - r.ArrivedOnOther.Time).Milliseconds;
                    var t5 = (r.StartOnOther.Time - r.DequeuedOnOther.Time).Milliseconds;
                    var t6 = (r.EndOnOther.Time - r.StartOnOther.Time).Milliseconds;
                    return $"{r.AppDomain}\t{r.ActivityId}\t{t0}\t{t1}\t{t2}\t|\t{t3}\t{t4}\t{t5}\t{t6}";
                })
            )
            {
                var lastDisplay = DateTime.Now;
                var waitTime = TimeSpan.FromSeconds(1);
                foreach (var item in analyzator)
                {
                    if (DateTime.Now - lastDisplay > waitTime)
                    {
                        Console.Write(".");
                        lastDisplay = DateTime.Now;
                    }
                    writer.WriteLine(item);
                }
                Console.WriteLine();
            }
        }

        //       ExecuteDistributedActivity: #6607
        //       IAQ: A6607 dequeued.
        // Start IAQ: A6607 EXECUTION.
        // End   IAQ: A6607 EXECUTION.
        // -----------------------------
        //       IAQ: A6607 arrived from another computer.
        //       IAQ: A6607 dequeued.
        // Start IAQ: A6607 EXECUTION.
        // End   IAQ: A6607 EXECUTION.

        private class DistributedIndexingActivityEntry : Entry
        {
            public string ActivityId { get; }
            public Entry DequeuedOnSame { get; internal set; }
            public Entry StartOnSame { get; internal set; }
            public Entry EndOnSame { get; internal set; }
            public Entry ArrivedOnOther { get; internal set; }
            public Entry DequeuedOnOther { get; internal set; }
            public Entry StartOnOther { get; internal set; }
            public Entry EndOnOther { get; internal set; }

            public DistributedIndexingActivityEntry(Entry sourceEntry) : base(sourceEntry)
            {
                ActivityId = sourceEntry.Message.Replace("ExecuteDistributedActivity: #", "A");
            }
        }
        */
    }
}
