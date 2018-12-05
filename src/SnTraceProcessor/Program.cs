using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.Diagnostics.Analysis;

namespace SnTraceProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var testDirectory = @"D:\dev\github\sensenet\src\Tests";

            Join(testDirectory, @"C:\Users\kavics\Desktop\trace.log");

            var xxxx = TestMethodTimes(testDirectory);
        }

        private static void Join(string testRoot, string targetFile)
        {
            var dirs = TraceDirectory.SearchTraceDirectories(testRoot).Select(x => x.Path).ToArray();

            var collector = new Collector();

            using (var writer = new StreamWriter(targetFile, false))
            using (var logFlow = Reader.Create(dirs, null))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Select(e =>
                    {
                        e.AppDomain = aps.Simplify(e.AppDomain);
                        return e;
                    });

                foreach (var item in transformedLogFlow)
                   writer.WriteLine(item);
            }
        }

        private static string TestMethodTimes(string testRoot)
        {
            var dirs = TraceDirectory.SearchTraceDirectories(testRoot).Select(x => x.Path).ToArray();

            var sb = new StringBuilder();
            var collector = new Collector();

            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(dirs, null))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == Category.Test)
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Where((e) =>
                    {
                        if (e.Message.StartsWith("START test: "))
                        {
                            var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("START test: ".Length)}";
                            collector.Set(key, "start", e);
                            return false;
                        }
                        if (e.Message.StartsWith("END test: "))
                        {
                            var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("END test: ".Length)}";
                            // false if the [key] does not exist.
                            return collector.Finish(key, e);
                        }
                        return false;
                    })
                    .Select(e => new
                    {
                        App = e.AppDomain,
                        Name = e.Message.Substring("END test: ".Length),
                        Duration = e.Time - e.Associations["start"].Time,
                    });

                var id = 0;
                foreach (dynamic item in transformedLogFlow)
                {
                    var name = item.Name;
                    var dt = item.Duration;
                    var app = item.App;
                    writer.WriteLine($"{++id}\t{dt}\t{app}\t{name}");
                }
            }


            var result = sb.ToString().Trim();
            return result;
        }
    }
}
