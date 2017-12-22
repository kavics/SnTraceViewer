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
        private class WebRequestEntryCollection : EntryCollection
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
    }
}
