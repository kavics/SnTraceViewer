using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//UNDONE: XMLDOC 2

namespace Transformer
{
    class Program
    {
        static void Main(string[] args)
        {
            var logDirectories = new[] {
                @"C:\Users\Gyebi\Desktop\benchmark\02\web1",
                @"C:\Users\Gyebi\Desktop\benchmark\02\web2",
            };
            var outputDir = @"C:\Users\Gyebi\Desktop\benchmark\02\";

            //Test1(logDirectories, outputDir);

            //Test2(logDirectories, outputDir);

            Test3(logDirectories, outputDir);

            //Test4(logDirectories, outputDir);

            if (Debugger.IsAttached)
            {
                Console.Write("press <enter> to exit...");
                Console.ReadLine();
            }
        }

        private static void Test1(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(outputDir + "events.txt"))
            using (var analyzator = Reader.Create(logDirectories, "detailedlog_*.log").Filter<Entry>(e => e.Category == "Event"))
            {
                foreach (var line in analyzator)
                    writer.WriteLine(line);
            }
        }

        private static void Test2(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(outputDir + "indexing.txt"))
            using (var analyzator = Reader.Create(logDirectories, "detailedlog_*.log")
                .Filter<Entry>(e => e.Category == "Index" && e.Message.EndsWith("Indexing node") && e.Status == "End")
                .Transform<Entry>(e => e.Duration.ToString().Replace("00:00:", "").Replace(".", ",")))
            {
                foreach (var line in analyzator)
                    writer.WriteLine(line);
            }
        }

        private static void Test3(string[] logDirectories, string outputDir)
        {
            using (var writer = new StreamWriter(outputDir + "indexing_debug.txt"))
            using (var analyzator = Reader.Create(logDirectories, "detailedlog_*.log")
                .Filter<Entry>(e => e.Category == "Index" || e.Category == "IndexQueue")
                .Filter<Entry>(
                    e => e.Message.StartsWith("ExecuteDistributedActivity:") ||
                    e.Message.Contains("arrived from another computer.")))
            {
                writer.WriteLine($"MIN\tSEC\tAPP\tACTIVITY\tMESSAGE");
                var activityIdRegex = new Regex(" [A#][0-9]+");
                foreach (var item in analyzator)
                {
                    // IAQ: A5142 arrived from another computer....
                    // ExecuteDistributedActivity: #5143
                    var regexMatch = activityIdRegex.Match(item.Message);
                    var activityId = regexMatch.Success ? regexMatch.Value.Trim().Substring(1) : "?";
                    writer.WriteLine($"{item.Time.ToString("mm\tss.fffff").Replace(".", ",")}\t{item.AppDomain}\tA{activityId}\t{item.Message}\t");
                }
            }
        }

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
    }
}
