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
            var sampleFilesRoot = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName + "\\SampleFiles";

            //var testDirectory = @"D:\dev\github\sensenet\src\Tests";


            //Join(testDirectory, @"C:\Users\kavics\Desktop\trace.log");


            //var xxxx = TestMethodTimes(testDirectory);


            ////BulkInsertTimes(
            ////    @"D:\projects\github\kavics\Gicogen\src\Gicogen\bin\Debug\App_Data\DetailedLog\withVarbinaryMax",
            ////    @"D:\projects\github\kavics\Gicogen\src\Gicogen\bin\Debug\App_Data\DetailedLog\withVarbinaryMax\1.table.txt");
            //var bulkInsertTimesRoot = sampleFilesRoot + "\\BulkInsertTimes";
            //BulkInsertTimes(bulkInsertTimesRoot, "result.txt");


            WebRequests(
                //@"D:\Desktop\1GC_doc\sn-benchmark\write\1MC\1\DetailedLog\detailedlog_20190301-053505Z.log",
                $@"{sampleFilesRoot}\WebRequests", @"result.txt");
        }

        /// <summary>
        /// Aggregates test logs to one file.
        /// </summary>
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

        /// <summary>
        /// Calculates test method execution times.
        /// </summary>
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
                    .Select(e =>
                    {
                        e.AppDomain = aps.Simplify(e.AppDomain);
                        return e;
                    })
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

        /// <summary>
        /// Collects periodical bulk insert times.
        /// </summary>
        /// <param name="inputFolder">Full path of the input directory containing *.log files.</param>
        /// <param name="outputFileName">Local file name in the input directory (e.g. "result.txt").</param>
        public static void BulkInsertTimes(string inputFolder, string outputFileName)
        {
            var outputFile = Path.Combine(inputFolder, outputFileName);

            var collector = new Collector();

            using (var writer = new StreamWriter(outputFile, false))
            using (var logFlow = Reader.Create(inputFolder, "*.log"))
            {
                var theKey = "Bulk insert ";
                var finisher = "Writing to database";
                var transformedLogFlow = logFlow
                    .Where((e) =>
                    {
                        if (e.Status == Status.End && e.Message.StartsWith(theKey))
                        {
                            var subKey = e.Message.Substring(theKey.Length);
                            collector.Set(theKey, subKey, e);
                        }
                        if (e.Status == Status.End && e.Message == finisher)
                        {
                            collector.Finish(theKey, e);
                            return true;
                        }
                        return false;
                    })
                    .Select(e => new
                    {
                        Nodes = e.Duration.TotalSeconds,
                        Versions = e.Associations["Versions"].Duration.TotalSeconds,
                        FlatProperties = e.Associations["FlatProperties"].Duration.TotalSeconds,
                        BinaryProperties = e.Associations["BinaryProperties"].Duration.TotalSeconds,
                        Files = e.Associations["Files"].Duration.TotalSeconds,
                        EFEntities = e.Associations["EFEntities"].Duration.TotalSeconds,
                    });

                var id = 0;
                writer.WriteLine("Id\tNodes\tVersions\tFlatProperties\tBinaryProperties\tFiles\tEFEntities");
                foreach (dynamic item in transformedLogFlow)
                {
                    writer.WriteLine(
                        $"{++id}\t{item.Nodes}\t{item.Versions}\t{item.FlatProperties}\t{item.BinaryProperties}\t{item.Files}\t{item.EFEntities}");
                }
            }
        }

        /// <summary>
        /// Collect webrequest times between "PCM.OnEnter" and "PCM.OnEndRequest" on the same thread.
        /// </summary>
        /// <param name="inputFolder">Full path of the input directory containing *.log files.</param>
        /// <param name="outputFileName">Local file name in the input directory (e.g. "result.txt").</param>
        public static void WebRequests(string inputFolder, string outputFileName)
        {
            var outputFile = Path.Combine(inputFolder, outputFileName);

            // PCM.OnEnter GET http://snpc007.sn.hu/odata.svc/Root/1GC/A/A/D('g.txt')?metadata=no&$select=Id,Path,Description&benchamrkId=P37A0x
            // PCM.OnEndRequest GET http://snpc007.sn.hu/odata.svc/Root/1GC/A/A/D('g.txt')?metadata=no&$select=Id,Path,Description&benchamrkId=P37A0x

            var starter = "PCM.OnEnter ";
            var finisher = "PCM.OnEndRequest ";
            string GetKey(Entry entry, bool isStarter)
            {
                var key = entry.Message.Substring((isStarter ? starter : finisher).Length);
                var p = key.IndexOf("?");
                if (p < 0)
                    p = key.Length;
                key = key.Substring(0, p);

                return $"{entry.ThreadId} {key}";
            }

            var collector = new Collector();
            using (var writer = new StreamWriter(outputFile, false))
            using (var logFlow = Reader.Create(inputFolder, "*.log"))
            {
                var transformedLogFlow = logFlow
                    .Where((e) =>
                    {
                        if (e != null)
                        {
                            if (e.Category == Category.Web && e.Message.StartsWith(starter))
                            {
                                var key = GetKey(e, true);
                                collector.Set(key, "start", e);
                            }
                            if (e.Category == Category.Web && e.Message.StartsWith(finisher))
                            {
                                var key = GetKey(e, false);
                                collector.Finish(key, e);
                                return true;
                            }
                        }
                        return false;
                    })
                    .Select(e => new
                    {
                        Start = e.Associations?["start"].Time ?? DateTime.MinValue,
                        End = e.Time,
                        Thread = e.ThreadId,
                        Request = e.Message.Substring(finisher.Length),
                    });

                var id = 0;
                writer.WriteLine("\t\tTime\t\tTicks");
                writer.WriteLine("Id\tThread\tStart\tDuration\tStart\tDuration\tRequest");

                var first = transformedLogFlow.First();
                var startTime = first.Start;
                var startTicks = startTime.Ticks;
                foreach (var item in transformedLogFlow)
                {
                    writer.WriteLine(
                        $"{++id}\t{item.Thread}\t{item.Start - startTime}\t{item.End - item.Start}\t{item.Start.Ticks - startTicks}\t{item.End.Ticks - item.Start.Ticks}\t{item.Request}");
                }
            }
        }
    }
}