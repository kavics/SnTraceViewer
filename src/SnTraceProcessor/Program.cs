using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.Diagnostics.Analysis;
using SenseNet.Tools;

namespace SnTraceProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new[] { "join", "-?" };
            args = new[] { "join", "-l", "-d" };

            Run(args);

            if (Debugger.IsAttached)
            {
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                Console.WriteLine();
            }
        }

        static void Run(string[] args)
        {
            var command = GetCommand(args.FirstOrDefault());
            if (command == null)
                return;

            try
            {
                var context = new CommandContext(command, args.Skip(1).ToArray());
                command.Context = context;
                command.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static readonly string[] HelpStrings = { "/?", "/h", "/help", "-?", "-h", "-help", "--help" };
        internal static ICommand GetCommand(string commandName)
        {
            if (commandName == null)
            {
                WriteHelp();
                return null;
            }
            if (HelpStrings.Contains(commandName.ToLowerInvariant()))
            {
                WriteHelp();
                return null;
            }

            var commandType =
                CommandTypes.FirstOrDefault(t =>
                    t.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase) ||
                    t.Name.Equals(commandName + "command", StringComparison.OrdinalIgnoreCase));
            if (commandType != null)
                return (ICommand)Activator.CreateInstance(commandType);

            WriteError($"Unknown command: {commandName}\r\n{GetAvailableCommandsMessage()}");
            return null;
        }

        private static void WriteError(string message)
        {
            Console.WriteLine(message);
        }

        private static string GetAvailableCommandsMessage()
        {
            return "Available commands:\r\n" + string.Join("\r\n",
                CommandTypes.Select(t => "  " + CommandContext.GetCommandName(t)).OrderBy(n => n));
        }

        private static readonly Type[] CommandTypes = new Lazy<Type[]>(() =>
            TypeResolver.GetTypesByInterface(typeof(ICommand))).Value;

        private static void WriteHelp()
        {

            var message = "SnTraceProcessor </? | -? | /h | -h | /help | -help | --help>\r\n" +
                          "SnTraceProcessor <command> [command-arguments]\r\n" +
                          "SnTraceProcessor <command> </? | -? | /h | -h | /help | -help | --help>\r\n" +
                          "\r\n" + GetAvailableCommandsMessage();

            Usage(message);

        }

        private static void Usage(string message)
        {
            Console.WriteLine();
            Console.WriteLine("SnTrace Processor V0.1");
            Console.WriteLine("======================");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine(message);
            Console.WriteLine();
        }

        /* ============================================================================= Old experimental code */

        ////static void Main0(string[] args)
        ////{
        ////    var testDirectory = @"D:\dev\github\sensenet\src\Tests";


        ////    //Join(testDirectory, @"C:\Users\kavics\Desktop\trace.log");


        ////    TestMethodTimes2(testDirectory, @"C:\Users\kavics\Desktop\trace.log");


        ////    ////GicogenBulkInsertTimes(
        ////    ////    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\1000MC_DbOnly",
        ////    ////    @"C:\Users\kavics\Desktop\1GC_doc\1000MC_DbOnly_BulkInsertTimes.txt");
        ////    ////GicogenBulkInsertTimes(
        ////    ////    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\100MC_DbOnly",
        ////    ////    @"C:\Users\kavics\Desktop\1GC_doc\100MC_DbOnly_BulkInsertTimes.txt");
        ////    ////GicogenBulkInsertTimes(
        ////    ////    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\10MC_DbOnly",
        ////    ////    @"C:\Users\kavics\Desktop\1GC_doc\10MC_DbOnly_BulkInsertTimes.txt");
        ////    ////GicogenBulkInsertTimes(
        ////    ////    @"D:\projects\_SN7_MC\_SN7_MC\App_Data\LogBackup\1MC_DbOnly",
        ////    ////    @"C:\Users\kavics\Desktop\1GC_doc\1MC_DbOnly_BulkInsertTimes.txt");


        ////    ////GicogenIndexTimes(
        ////    ////    @"D:\projects\github\kavics\Gicogen\src\Gicogen\bin\Debug\App_Data\DetailedLog\!index_small_1000MC",
        ////    ////    @"C:\Users\kavics\Desktop\1GC_doc\index-import\500MD.txt");


        ////    //var sampleFilesRoot = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName + "\\SampleFiles";
        ////    //WebRequests(
        ////    //    //@"D:\Desktop\1GC_doc\sn-benchmark\write\1MC\1\DetailedLog\detailedlog_20190301-053505Z.log",
        ////    //    $@"{sampleFilesRoot}\WebRequests", @"result.txt");
        ////}

        ///// <summary>
        ///// Aggregates test logs to one file.
        ///// </summary>
        //private static void Join(string testRoot, string targetFile)
        //{
        //    var dirs = TraceDirectory.SearchTraceDirectories(testRoot).Select(x => x.Path).ToArray();

        //    var collector = new Collector();

        //    using (var writer = new StreamWriter(targetFile, false))
        //    using (var logFlow = Reader.Create(dirs, null))
        //    {
        //        var aps = new AppDomainSimplifier("App-{0}");

        //        var transformedLogFlow = logFlow
        //            .Select(e =>
        //            {
        //                e.AppDomain = aps.Simplify(e.AppDomain);
        //                return e;
        //            });

        //        foreach (var item in transformedLogFlow)
        //            writer.WriteLine(item);
        //    }
        //}
        ///// <summary>
        ///// Calculates test method execution times.
        ///// </summary>
        //private static string TestMethodTimes(string testRoot)
        //{
        //    var dirs = TraceDirectory.SearchTraceDirectories(testRoot).Select(x => x.Path).ToArray();

        //    var sb = new StringBuilder();
        //    var collector = new Collector();

        //    using (var writer = new StringWriter(sb))
        //    using (var logFlow = Reader.Create(dirs, null))
        //    {
        //        var aps = new AppDomainSimplifier("App-{0}");

        //        var transformedLogFlow = logFlow
        //            .Where(e => e.Category == Category.Test)
        //            .Select(e =>
        //            {
        //                e.AppDomain = aps.Simplify(e.AppDomain);
        //                return e;
        //            })
        //            .Where((e) =>
        //            {
        //                if (e.Message.StartsWith("START test: "))
        //                {
        //                    var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("START test: ".Length)}";
        //                    collector.Set(key, "start", e);
        //                    return false;
        //                }
        //                if (e.Message.StartsWith("END test: "))
        //                {
        //                    var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("END test: ".Length)}";
        //                    // false if the [key] does not exist.
        //                    return collector.Finish(key, e);
        //                }
        //                return false;
        //            })
        //            .Select(e => new
        //            {
        //                App = e.AppDomain,
        //                Name = e.Message.Substring("END test: ".Length),
        //                Duration = e.Time - e.Associations["start"].Time,
        //            });

        //        var id = 0;
        //        foreach (dynamic item in transformedLogFlow)
        //        {
        //            var name = item.Name;
        //            var dt = item.Duration;
        //            var app = item.App;
        //            writer.WriteLine($"{++id}\t{dt}\t{app}\t{name}");
        //        }
        //    }


        //    var result = sb.ToString().Trim();
        //    return result;
        //}
        //private static void TestMethodTimes2(string testRoot, string targetFile)
        //{
        //    var dirs = TraceDirectory.SearchTraceDirectories(testRoot).Select(x => x.Path).ToArray();

        //    var sb = new StringBuilder();
        //    var collector = new Collector();

        //    using (var writer = new StreamWriter(targetFile, false))
        //    using (var logFlow = Reader.Create(dirs, null))
        //    {
        //        var aps = new AppDomainSimplifier("App-{0}");

        //        var transformedLogFlow = logFlow
        //            .Where(e => e.Category == Category.Test)
        //            .Select(e =>
        //            {
        //                e.AppDomain = aps.Simplify(e.AppDomain);
        //                return e;
        //            })
        //            .Where((e) =>
        //            {
        //                //return (e.Message.StartsWith("TESTMETHOD: ") && e.Status != "Start");

        //                if (e.Message.StartsWith("TESTMETHOD: ") && e.Status == "Start")
        //                {
        //                    var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("TESTMETHOD: ".Length)}";
        //                    collector.Set(key, "start", e);
        //                    return false;
        //                }
        //                if (e.Message.StartsWith("TESTMETHOD: ") && e.Status != "Start")
        //                {
        //                    var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("TESTMETHOD: ".Length)}";
        //                    // false if the [key] does not exist.
        //                    return collector.Finish(key, e);
        //                }
        //                return false;
        //            })
        //            .Select(e => new
        //            {
        //                App = e.AppDomain,
        //                Status = e.Status,
        //                Name = e.Message.Substring("TESTMETHOD: ".Length),
        //                Start = e.Associations["start"].Time,
        //                Duration = e.Duration,
        //            });

        //        var id = 0;
        //        foreach (dynamic item in transformedLogFlow)
        //        {
        //            var name = item.Name;
        //            var start = item.Start;
        //            var st = item.Status;
        //            var dt = item.Duration;
        //            var app = item.App;
        //            writer.WriteLine($"{++id}\t{start:yyyy-MM-dd HH:mm:ss.fffff}\t{st}\t{dt:c}\t{app}\t{name}");
        //        }

        //        foreach (var item in collector.Items)
        //        {
        //            var e = item.Value.Values.First();
        //            var name = e.Message.Substring("TESTMETHOD: ".Length);
        //            var start = e.Time;
        //            var st = "UNFINISHED";
        //            var dt = TimeSpan.Zero;
        //            var app = e.AppDomain;
        //            writer.WriteLine($"{++id}\t{start:yyyy-MM-dd HH:mm:ss.fffff}\t{st}\t{dt:c}\t{app}\t{name}");
        //        }
        //    }
        //}

        ///// <summary>
        ///// Collects periodical bulk insert times.
        ///// </summary>
        ///// <param name="inputFolder">Full path of the input directory containing *.log files.</param>
        ///// <param name="outputFileName">Local file name in the input directory (e.g. "result.txt").</param>
        //private static void GicogenBulkInsertTimes(string inputFolder, string outputFile)
        //{
        //    var collector = new Collector();

        //    using (var writer = new StreamWriter(outputFile, false))
        //    using (var logFlow = Reader.Create(inputFolder, "*.log"))
        //    {
        //        var theKey = "Bulk insert ";
        //        var finisher = "Writing to database";
        //        var transformedLogFlow = logFlow
        //            .Where((e) =>
        //            {
        //                if (e.Status == Status.End && e.Message.StartsWith(theKey))
        //                {
        //                    var subKey = e.Message.Substring(theKey.Length);
        //                    collector.Set(theKey, subKey, e);
        //                }
        //                if (e.Status == Status.End && e.Message == finisher)
        //                {
        //                    collector.Set(theKey, "Total", e);
        //                    collector.Finish(theKey, e);
        //                    return true;
        //                }
        //                return false;
        //            })
        //            .Select(e => new
        //            {
        //                Nodes = e.Duration.TotalSeconds,
        //                Versions = e.Associations["Versions"].Duration.TotalSeconds,
        //                FlatProperties = e.Associations["FlatProperties"].Duration.TotalSeconds,
        //                BinaryProperties = e.Associations["BinaryProperties"].Duration.TotalSeconds,
        //                Files = e.Associations["Files"].Duration.TotalSeconds,
        //                EFEntities = e.Associations["EFEntities"].Duration.TotalSeconds,
        //                Total = e.Associations["Total"].Duration.TotalSeconds,
        //            });

        //        var id = 0;
        //        writer.WriteLine("Id\tNodes\tVersions\tFlatProperties\tBinaryProperties\tFiles\tEFEntities\tTotal");
        //        foreach (dynamic item in transformedLogFlow)
        //        {
        //            writer.WriteLine(
        //                $"{++id}\t{item.Nodes}\t{item.Versions}\t{item.FlatProperties}\t{item.BinaryProperties}\t{item.Files}\t{item.EFEntities}\t{item.Total}");
        //        }
        //    }
        //}
        //private static void GicogenIndexTimes(string inputFolder, string outputFile)
        //{
        //    using (var writer = new StreamWriter(outputFile, false))
        //    using (var logFlow = Reader.Create(inputFolder, "*.log"))
        //    {
        //        var transformedLogFlow = logFlow
        //            .Where((e) => e.Message.StartsWith("Reset memory tables"));

        //        var id = 0;
        //        writer.WriteLine("Id\tTime");
        //        Entry last = null;
        //        foreach (var item in transformedLogFlow)
        //        {
        //            if (last != null)
        //            {
        //                var deltaTime = (item.Time - last.Time).TotalSeconds;
        //                writer.WriteLine($"{++id}\t{deltaTime}");
        //            }
        //            last = item;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Collect webrequest times between "PCM.OnEnter" and "PCM.OnEndRequest" on the same thread.
        ///// </summary>
        ///// <param name="inputFolder">Full path of the input directory containing *.log files.</param>
        ///// <param name="outputFileName">Local file name in the input directory (e.g. "result.txt").</param>
        //public static void WebRequests(string inputFolder, string outputFileName)
        //{
        //    var outputFile = Path.Combine(inputFolder, outputFileName);

        //    // PCM.OnEnter GET http://snpc007.sn.hu/odata.svc/Root/1GC/A/A/D('g.txt')?metadata=no&$select=Id,Path,Description&benchamrkId=P37A0x
        //    // PCM.OnEndRequest GET http://snpc007.sn.hu/odata.svc/Root/1GC/A/A/D('g.txt')?metadata=no&$select=Id,Path,Description&benchamrkId=P37A0x

        //    var starter = "PCM.OnEnter ";
        //    var finisher = "PCM.OnEndRequest ";
        //    string GetKey(Entry entry, bool isStarter)
        //    {
        //        var key = entry.Message.Substring((isStarter ? starter : finisher).Length);
        //        var p = key.IndexOf("?");
        //        if (p < 0)
        //            p = key.Length;
        //        key = key.Substring(0, p);

        //        return $"{entry.ThreadId} {key}";
        //    }

        //    var collector = new Collector();
        //    using (var writer = new StreamWriter(outputFile, false))
        //    using (var logFlow = Reader.Create(inputFolder, "*.log"))
        //    {
        //        var transformedLogFlow = logFlow
        //            .Where((e) =>
        //            {
        //                if (e != null)
        //                {
        //                    if (e.Category == Category.Web && e.Message.StartsWith(starter))
        //                    {
        //                        var key = GetKey(e, true);
        //                        collector.Set(key, "start", e);
        //                    }
        //                    if (e.Category == Category.Web && e.Message.StartsWith(finisher))
        //                    {
        //                        var key = GetKey(e, false);
        //                        collector.Finish(key, e);
        //                        return true;
        //                    }
        //                }
        //                return false;
        //            })
        //            .Select(e => new
        //            {
        //                Start = e.Associations?["start"].Time ?? DateTime.MinValue,
        //                End = e.Time,
        //                Thread = e.ThreadId,
        //                Request = e.Message.Substring(finisher.Length),
        //            });

        //        var id = 0;
        //        writer.WriteLine("\t\tTime\t\tTicks");
        //        writer.WriteLine("Id\tThread\tStart\tDuration\tStart\tDuration\tRequest");

        //        var first = transformedLogFlow.First();
        //        var startTime = first.Start;
        //        var startTicks = startTime.Ticks;
        //        foreach (var item in transformedLogFlow)
        //        {
        //            writer.WriteLine(
        //                $"{++id}\t{item.Thread}\t{item.Start - startTime}\t{item.End - item.Start}\t{item.Start.Ticks - startTicks}\t{item.End.Ticks - item.Start.Ticks}\t{item.Request}");
        //        }
        //    }
        //}
    }
}