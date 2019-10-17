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


            //Join(testDirectory, @"C:\Users\kavics\Desktop\trace.log");


            var xxxx = TestMethodTimes2(testDirectory);


            //GicogenBulkInsertTimes(
            //    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\1000MC_DbOnly",
            //    @"C:\Users\kavics\Desktop\1GC_doc\1000MC_DbOnly_BulkInsertTimes.txt");
            //GicogenBulkInsertTimes(
            //    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\100MC_DbOnly",
            //    @"C:\Users\kavics\Desktop\1GC_doc\100MC_DbOnly_BulkInsertTimes.txt");
            //GicogenBulkInsertTimes(
            //    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\10MC_DbOnly",
            //    @"C:\Users\kavics\Desktop\1GC_doc\10MC_DbOnly_BulkInsertTimes.txt");
            //GicogenBulkInsertTimes(
            //    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\1MC_DbOnly",
            //    @"C:\Users\kavics\Desktop\1GC_doc\1MC_DbOnly_BulkInsertTimes.txt");


            //GicogenIndexTimes(
            //    @"D:\projects\github\kavics\Gicogen\src\Gicogen\bin\Debug\App_Data\DetailedLog\!index_small_1000MC",
            //    @"C:\Users\kavics\Desktop\1GC_doc\index-import\500MD.txt");
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
        private static string TestMethodTimes2(string testRoot)
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
                        return (e.Message.StartsWith("TESTMETHOD: ") && e.Status != "Start");

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
                        Status = e.Status,
                        Name = e.Message.Substring("TESTMETHOD: ".Length),
                        Duration = e.Duration,
                    });

                var id = 0;
                foreach (dynamic item in transformedLogFlow)
                {
                    var name = item.Name;
                    var st = item.Status;
                    var dt = item.Duration;
                    var app = item.App;
                    writer.WriteLine($"{++id}\t{st}\t{dt}\t{app}\t{name}");
                }
            }


            var result = sb.ToString().Trim();
            return result;
        }

        private static void GicogenBulkInsertTimes(string inputFolder, string outputFile)
        {
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
                            collector.Set(theKey, "Total", e);
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
                        Total = e.Associations["Total"].Duration.TotalSeconds,
                    });

                var id = 0;
                writer.WriteLine("Id\tNodes\tVersions\tFlatProperties\tBinaryProperties\tFiles\tEFEntities\tTotal");
                foreach (dynamic item in transformedLogFlow)
                {
                    writer.WriteLine(
                        $"{++id}\t{item.Nodes}\t{item.Versions}\t{item.FlatProperties}\t{item.BinaryProperties}\t{item.Files}\t{item.EFEntities}\t{item.Total}");
                }
            }
        }


        private static void GicogenIndexTimes(string inputFolder, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile, false))
            using (var logFlow = Reader.Create(inputFolder, "*.log"))
            {
                var transformedLogFlow = logFlow
                    .Where((e) => e.Message.StartsWith("Reset memory tables"));

                var id = 0;
                writer.WriteLine("Id\tTime");
                Entry last = null;
                foreach (var item in transformedLogFlow)
                {
                    if (last != null)
                    {
                        var deltaTime = (item.Time- last.Time).TotalSeconds;
                        writer.WriteLine($"{++id}\t{deltaTime}");
                    }
                    last = item;
                }
            }
        }
    }
}