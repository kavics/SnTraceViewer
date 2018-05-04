using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnTraceAnalyzerTests.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceAnalyzerTests
{
    [TestClass]
    public class OldTransformationTests
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

        [TestMethod]
        public void OldTransform_SimpleRead()
        {
            var logDirectories = new[] {
                @"D:\Desktop\benchmark\02\web1",
                @"D:\Desktop\benchmark\02\web2",
            };
            var outputDir = @"D:\Desktop\benchmark\02\";

            using (var writer = new StreamWriter(Path.Combine(outputDir, "webrequests1.log")))
            using (var logFlow = Reader.Create(logDirectories, "detailedlog_*.log"))
            {
                var transformedLogFlow = logFlow
                    .Take(3);
                foreach (var item in transformedLogFlow)
                    writer.WriteLine(item);
            }
        }
        [TestMethod]
        public void OldTransform_Filtering()
        {
            var logDirectories = new[] {
                @"D:\Desktop\benchmark\02\web1",
                @"D:\Desktop\benchmark\02\web2",
            };
            var outputDir = @"D:\Desktop\benchmark\02\";

            using (var writer = new StreamWriter(Path.Combine(outputDir, "webrequests2.log")))
            using (var logFlow = Reader.Create(logDirectories, "detailedlog_*.log"))
            {
                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web")
                    .Take(3);
                foreach (var item in transformedLogFlow)
                    writer.WriteLine(item);
            }
        }
        [TestMethod]
        public void OldTransform_Modifying()
        {
            var logDirectories = new[] {
                @"D:\Desktop\benchmark\02\web1",
                @"D:\Desktop\benchmark\02\web2",
            };
            var outputDir = @"D:\Desktop\benchmark\02\";

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
        [TestMethod]
        public void OldTransform_Collecting()
        {
            var logDirectories = new[] {
                @"D:\Desktop\benchmark\02\web1",
                @"D:\Desktop\benchmark\02\web2",
            };
            var outputDir = @"D:\Desktop\benchmark\02\";

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
