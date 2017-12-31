using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using SenseNet.Diagnostics.Analysis;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SnTraceAnalyzerTests
{
    [TestClass]
    public class AnalysisTests
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
        private class DistributedIndexingActivityCollection : EntryCollection<Entry>
        {
            public static class Q
            {
                public const string Start1 = "Start1";
                public const string Start2 = "Start2";
                public const string Dequeue = "Dequeue";
                public const string ExecStart = "ExecStart";
                public const string End = "End";
            }

            public Entry Start1;
            public Entry Start2;
            public Entry Dequeue1;
            public Entry Dequeue2;
            public Entry ExecStart1;
            public Entry ExecStart2;
            public Entry End1;
            public Entry End2;
            private bool _sorted;

            public override void Add(Entry entry, string qualification)
            {
                switch (qualification)
                {
                    case Q.Start1: Start1 = entry; break;
                    case Q.Start2: Start2 = entry; break;
                    case Q.Dequeue: if (Dequeue1 == null) Dequeue1 = entry; else Dequeue2 = entry; break;
                    case Q.ExecStart: if (ExecStart1 == null) ExecStart1 = entry; else ExecStart2 = entry; break;
                    case Q.End: if (End1 == null) End1 = entry; else End2 = entry; break;
                }
            }
            public override bool Finished()
            {
                if( End1 != null && End2 != null)
                {
                    if (!_sorted)
                        TidyUp();
                    return true;
                }
                return false;
            }
            private void TidyUp()
            {
                Entry e;
                if (Dequeue1.AppDomain != Start1.AppDomain)
                {
                    e = Start1; Start1 = Start2; Start2 = e;
                }
                if (ExecStart1.AppDomain != Start1.AppDomain)
                {
                    e = ExecStart1; ExecStart1 = ExecStart2; ExecStart2 = e;
                }
                if (End1.AppDomain != Start1.AppDomain)
                {
                    e = End1; End1 = End2; End2 = e;
                }
                _sorted = true;
            }
        }

        [TestMethod]
        public void Analysis_SimpleRead()
        {
            Entry[] entries;
            using (var logFlow = new InMemoryEntryReader(_logForSimpleRead))
            {
                entries = logFlow.Skip(4).Take(3).ToArray();
            }

            Assert.AreEqual(3, entries.Length);
            Assert.AreEqual(5, entries[0].LineId);
            Assert.AreEqual(6, entries[1].LineId);
            Assert.AreEqual(7, entries[2].LineId);
        }
        #region Data for Analysis_SimpleRead
        private string[] _logForSimpleRead = new[]
        {
            "----",
            ">1\t2017-11-14 02:22:51.49021\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\t\t\t\tPCM.OnEnter GET http://snbweb02.sn.hu/",
            "2\t2017-11-14 02:22:51.50583\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:1\tStart\t\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT NodeId FROM Nodes WHERE  NodeTypeId = 58",
            "3\t2017-11-14 02:22:51.52145\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:1\tEnd\t00:00:00.015623\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT NodeId FROM Nodes WHERE  NodeTypeId = 58",
            "4\t2017-11-14 02:22:51.56833\tQuery\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:2\tStart\t\tContentQuery: +InTree:/Root/System/Devices +TypeIs:Device .AUTOFILTERS:OFF | Top:0 Skip:0 Sort:[null] Mode:Default AllVersions:False",
            "5\t2017-11-14 02:22:51.66208\tEvent\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\t\t\t\tINFORMATION #e9ef4821-ad4b-4919-89f8-58860bbb8abd: TemplateReplacers created, see supported templates below.",
            "6\t2017-11-14 02:22:51.78708\tQuery\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:3\tStart\t\tLucQuery: +InTree:/root/system/devices +TypeIs:device .TOP:2147483647 .AUTOFILTERS:OFF",
            "7\t2017-11-14 02:22:51.78708\tQuery\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:4\tStart\t\tLuceneQueryExecutor. CQL:+InTree:/root/system/devices +TypeIs:device .TOP:2147483647 .AUTOFILTERS:OFF",
            "8\t2017-11-14 02:22:51.78708\tQuery\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:4\tEnd\t00:00:00.000000\tLuceneQueryExecutor. CQL:+InTree:/root/system/devices +TypeIs:device .TOP:2147483647 .AUTOFILTERS:OFF",
            "9\t2017-11-14 02:22:51.78708\tQuery\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\t\t\t\tLucQuery.Execute total count: 0",
            "10\t2017-11-14 02:22:51.78708\tQuery\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:9\tOp:3\tEnd\t00:00:00.000000\tLucQuery: +InTree:/root/system/devices +TypeIs:device .TOP:2147483647 .AUTOFILTERS:OFF",
        };
        #endregion

        [TestMethod]
        public void Analysis_Filtering()
        {
            Entry[] entries;
            using (var logFlow = new InMemoryEntryReader(_logForFiltering))
            {
                entries = logFlow
                    .Where(e => e.Category == "Web") //UNDONE: use constants
                    .ToArray();
            }

            var expected = "3335, 3336, 3341, 3342, 3343, 3344";
            var actual = string.Join(", ", entries.Select(e => e.LineId).ToArray());
            Assert.AreEqual(expected, actual);
        }
        #region Data for Analysis_Filtering
        private string[] _logForFiltering = new[]
        {
            "3333\t2017-11-14 02:25:52.55453\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:26\tOp:1448\tEnd\t00:00:00.093752\tLM: Commit. reopenReader:True",
            "3334\t2017-11-14 02:25:52.55453\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:26\tOp:1446\tEnd\t00:00:00.093752\tIAQ: A5136 EXECUTION.",
            "3335\t2017-11-14 02:25:52.57016\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tPCM.OnEnter POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/SystemFolder-20171114022535/('5')/Upload?create=1&metadata=no",
            "3336\t2017-11-14 02:25:52.58579\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tHTTP Action.ActionType: RemapHttpAction, TargetNode: [null], AppNode: [null], HttpHandlerType:ODataHandler",
            ">3337\t2017-11-14 02:25:53.41391\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\tOp:1453\tStart\t\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "3338\t2017-11-14 02:25:53.41391\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\tOp:1453\tEnd\t00:00:00.000000\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "3339\t2017-11-14 02:25:53.41391\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\tOp:1454\tStart\t\tContent.CreateNew",
            "3340\t2017-11-14 02:25:53.41391\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\tOp:1454\tEnd\t00:00:00.000000\tContent.CreateNew",
            "3341\t2017-11-14 02:25:53.41391\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/OData.svc/Root/Benchmark/SystemFolder-20171114022535/('5')/Upload?create=1&metadata=no, StatusCode:200",
            "3342\t2017-11-14 02:25:53.41391\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tPCM.OnEndRequest POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/SystemFolder-20171114022535/('5')/Upload?create=1&metadata=no",
            "3343\t2017-11-14 02:25:53.42954\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tPCM.OnEnter POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/SystemFolder-20171114022535/('5')/Upload?metadata=no",
            "3344\t2017-11-14 02:25:53.42954\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tHTTP Action.ActionType: RemapHttpAction, TargetNode: [null], AppNode: [null], HttpHandlerType:ODataHandler",
            ">3345\t2017-11-14 02:25:54.22642\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1455\tStart\t\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "3346\t2017-11-14 02:25:54.22642\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1455\tEnd\t00:00:00.000000\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "3347\t2017-11-14 02:25:54.22642\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1456\tStart\t\tContent.CreateNew",
        };
        #endregion

        [TestMethod]
        public void Analysis_Modify()
        {
            var logs = new[] { _log1ForModifyTest, _log2ForModifyTest, _log3ForModifyTest };

            // action
            string actual;
            using (var logFlow = Reader.Create(logs))
            {
                var aps = new AppDomainSimplifier("App{0}");
                actual = string.Join(",", logFlow
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .ToArray()
                    .Select(e => e.AppDomain));
            }

            //  assert
            var expected = "App1,App1,App2,App2,App3,App3";
            Assert.AreEqual(expected, actual);
        }
        #region Logs for Analysis_Modify
        string[] _log1ForModifyTest = new[]
        {
            "10\t2017-11-13 03:55:40.00000\tTest\tA:AppDomain-A\tT:42\t\t\t\tMsg400",
            "11\t2017-11-13 03:55:41.00000\tTest\tA:AppDomain-A\tT:42\t\t\t\tMsg410",
        };
        string[] _log2ForModifyTest = new[]
        {
            "10\t2017-11-13 03:55:42.00000\tTest\tA:AppDomain-B\tT:42\t\t\t\tMsg420",
            "11\t2017-11-13 03:55:43.00000\tTest\tA:AppDomain-B\tT:42\t\t\t\tMsg430",
        };
        string[] _log3ForModifyTest = new[]
        {
            "10\t2017-11-13 03:55:44.00000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg440",
            "11\t2017-11-13 03:55:45.00000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg450",
        };
        #endregion

        [TestMethod]
        public void Analysis_Merge()
        {
            var logs = new[] { _log1ForMergeTest, _log2ForMergeTest, _log3ForMergeTest };

            // action
            string actual;
            using (var logFlow = Reader.Create(logs))
                actual = string.Join(",", logFlow.Select(e => e.Message));

            //  assert
            var expected = "Msg400,Msg405,Msg410,Msg415,Msg417,Msg420,Msg422,Msg427,Msg430,Msg435,Msg440,Msg445,Msg450";
            Assert.AreEqual(expected, actual);
        }
        #region Logs for Analysis_Merge
        string[] _log1ForMergeTest = new[]
        {
            "10\t2017-11-13 03:55:40.00000\tTest\tA:AppDomain-A\tT:42\t\t\t\tMsg400",
            "11\t2017-11-13 03:55:42.00000\tTest\tA:AppDomain-A\tT:42\t\t\t\tMsg420",
            "12\t2017-11-13 03:55:44.00000\tTest\tA:AppDomain-A\tT:42\t\t\t\tMsg440",
        };
        string[] _log2ForMergeTest = new[]
        {
            "10\t2017-11-13 03:55:41.00000\tTest\tA:AppDomain-B\tT:42\t\t\t\tMsg410",
            "11\t2017-11-13 03:55:43.00000\tTest\tA:AppDomain-B\tT:42\t\t\t\tMsg430",
            "12\t2017-11-13 03:55:45.00000\tTest\tA:AppDomain-B\tT:42\t\t\t\tMsg450",
        };
        string[] _log3ForMergeTest = new[]
        {
            "10\t2017-11-13 03:55:40.50000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg405",
            "11\t2017-11-13 03:55:41.50000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg415",
            "12\t2017-11-13 03:55:41.70000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg417",
            "13\t2017-11-13 03:55:42.20000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg422",
            "14\t2017-11-13 03:55:42.70000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg427",
            "15\t2017-11-13 03:55:43.50000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg435",
            "16\t2017-11-13 03:55:44.50000\tTest\tA:AppDomain-C\tT:42\t\t\t\tMsg445",
        };
        #endregion

        [TestMethod]
        public void Analysis_SimpleCollect()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(_logForSimpleCollectTest))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web") //UNDONE: use constants
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect<Entry, WebRequestEntryCollection>((e) =>
                    {
                        if (e.Message.StartsWith("PCM.OnEnter "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", WebRequestEntryCollection.Q.Start);
                        else if (e.Message.StartsWith("PCM.OnEndRequest "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", WebRequestEntryCollection.Q.End);
                        return null;
                    });

                foreach (var item in transformedLogFlow)
                {
                    var app = item.StartEntry.AppDomain;
                    var time = item.StartEntry.Time.ToString("HH:mm:ss.fffff");
                    var req = item.StartEntry.Message.Substring("PCM.OnEnter ".Length);
                    var dt = item.EndEntry.Time - item.StartEntry.Time;
                    writer.WriteLine($"{app}\t{time}\t{dt}\t{req}");
                }
            }

            var expected = string.Join(Environment.NewLine, new[] {
                    "App-1	02:25:28.25307	00:00:00.0156500	GET http://snbweb01.sn.hu/",
                    "App-1	02:25:28.47362	00:00:00	GET http://snbweb01.sn.hu/favicon.ico",
                    "App-1	02:25:34.95093	00:00:02.9687600	POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark?benchamrkId=P5A0x",
                    "App-1	02:25:38.55033	00:00:00.8593700	POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?create=1&metadata=no",
                });
            var actual = sb.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Analysis2_SimpleCollect()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(_logForSimpleCollectTest))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web") //UNDONE: use constants
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect2((e) =>
                    {
                        if (e.Message.StartsWith("PCM.OnEnter "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", "StartEntry");
                        else if (e.Message.StartsWith("PCM.OnEndRequest "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", "EndEntry");
                        return null;
                    }, (c) =>
                    {
                        var d = (IDictionary<string, object>)c;
                        if (c.StartEntry == null || !d.ContainsKey("EndEntry"))
                            return null;
                        return new
                        {
                            App = c.StartEntry.AppDomain,
                            Time = c.StartEntry.Time,
                            Request = c.StartEntry.Message.Substring("PCM.OnEnter ".Length),
                            Duration = c.EndEntry.Time - c.StartEntry.Time,
                        };
                    });

                foreach (dynamic item in transformedLogFlow)
                {
                    var app = item.App;
                    var time = item.Time.ToString("HH:mm:ss.fffff");
                    var req = item.Request;
                    var dt = item.Duration;
                    writer.WriteLine($"{app}\t{time}\t{dt}\t{req}");
                }
            }

            var expected = string.Join(Environment.NewLine, new[] {
                    "App-1	02:25:28.25307	00:00:00.0156500	GET http://snbweb01.sn.hu/",
                    "App-1	02:25:28.47362	00:00:00	GET http://snbweb01.sn.hu/favicon.ico",
                    "App-1	02:25:34.95093	00:00:02.9687600	POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark?benchamrkId=P5A0x",
                    "App-1	02:25:38.55033	00:00:00.8593700	POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?create=1&metadata=no",
                });
            var actual = sb.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }
        #region Data for Analysis_SimpleCollect
        private string[] _logForSimpleCollectTest = new[]
        {
            ">2533\t2017-11-14 02:25:19.14251\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tCPU: 0,1836666%, RAM: 607016 KBytes available (working set: 299585536 bytes)",
            ">2534\t2017-11-14 02:25:28.25307\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:10\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/",
            "2535\t2017-11-14 02:25:28.25307\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:10\t\t\t\tHTTP Action.ActionType: DefaultHttpAction, TargetNode: [null], AppNode: [null], RequestUrl:http://snbweb01.sn.hu/",
            "2536\t2017-11-14 02:25:28.26872\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:10\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/, StatusCode:200",
            "2537\t2017-11-14 02:25:28.26872\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:10\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/",
            "2538\t2017-11-14 02:25:28.47362\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:6\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/favicon.ico",
            "2539\t2017-11-14 02:25:28.47362\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:6\t\t\t\tHTTP Action.ActionType: DefaultHttpAction, TargetNode: [null], AppNode: [null], RequestUrl:http://snbweb01.sn.hu/favicon.ico",
            "2540\t2017-11-14 02:25:28.47362\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:6\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/favicon.ico, StatusCode:200",
            "2541\t2017-11-14 02:25:28.47362\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:6\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/favicon.ico",
            ">2542\t2017-11-14 02:25:29.14242\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tCPU: 0,1337686%, RAM: 607004 KBytes available (working set: 299618304 bytes)",
            ">2543\t2017-11-14 02:25:34.95093\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tPCM.OnEnter POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark?benchamrkId=P5A0x",
            "2544\t2017-11-14 02:25:34.95093\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tHTTP Action.ActionType: RemapHttpAction, TargetNode: [null], AppNode: [null], HttpHandlerType:ODataHandler",
            ">2545\t2017-11-14 02:25:35.04468\tEvent\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tINFORMATION #7688ffd2-f7f1-412c-95d0-eb1e8c42ff93: MembershipProvider instantiated: SenseNet.ContentRepository.Security.SenseNetMembershipProvider",
            "2546\t2017-11-14 02:25:35.04468\tRepository\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tSenseNetMembershipProvider initialized: SenseNet.ContentRepository.Security.SenseNetMembershipProvider",
            "2547\t2017-11-14 02:25:35.04468\tEvent\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tINFORMATION #64a7d636-3513-4c0b-9aff-83c3b5c0417e: DirectoryProvider not present.",
            "---",
            "2659\t2017-11-14 02:25:37.79469\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\tOp:1253\tEnd\t00:00:00.000000\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT PropertyTypeId, Value FROM TextPropertiesNText WHERE VersionId = @VersionId AND PropertyTypeId IN (@Prop0, @Prop1, @Prop2, @Prop3, @Prop4, @Prop5, @Prop6)",
            "2660\t2017-11-14 02:25:37.91969\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/OData.svc/Root/Benchmark?benchamrkId=P5A0x, StatusCode:200",
            "2661\t2017-11-14 02:25:37.91969\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tPCM.OnEndRequest POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark?benchamrkId=P5A0x",
            ">2662\t2017-11-14 02:25:38.55033\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tPCM.OnEnter POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?create=1&metadata=no",
            "2663\t2017-11-14 02:25:38.58157\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tHTTP Action.ActionType: RemapHttpAction, TargetNode: [null], AppNode: [null], HttpHandlerType:ODataHandler",
            ">2664\t2017-11-14 02:25:39.14407\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:8\t\t\t\tCPU: 32,28711%, RAM: 575492 KBytes available (working set: 316006400 bytes)",
            "2665\t2017-11-14 02:25:39.39407\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1254\tStart\t\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "2666\t2017-11-14 02:25:39.40970\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1254\tEnd\t00:00:00.015630\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "2667\t2017-11-14 02:25:39.40970\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1255\tStart\t\tContent.CreateNew",
            "2668\t2017-11-14 02:25:39.40970\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1255\tEnd\t00:00:00.000000\tContent.CreateNew",
            "2669\t2017-11-14 02:25:39.40970\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?create=1&metadata=no, StatusCode:200",
            "2670\t2017-11-14 02:25:39.40970\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tPCM.OnEndRequest POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?create=1&metadata=no",
            "2671\t2017-11-14 02:25:39.42533\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tPCM.OnEnter POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?metadata=no",
            "2672\t2017-11-14 02:25:39.42533\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tHTTP Action.ActionType: RemapHttpAction, TargetNode: [null], AppNode: [null], HttpHandlerType:ODataHandler",
            ">2673\t2017-11-14 02:25:40.19095\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1256\tStart\t\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
            "2674\t2017-11-14 02:25:40.19095\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1256\tEnd\t00:00:00.000000\tSqlProcedure.ExecuteReader (tran:0): Command: SELECT...",
        };
        #endregion

        [TestMethod]
        public void Analysis2_SimpleMapReduce()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(_logForSimpleMapReduce))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web") //UNDONE: use constants
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect2((e) =>
                    {
                        if (e.Message.StartsWith("PCM.OnEnter "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", "StartEntry");
                        else if (e.Message.StartsWith("PCM.OnEndRequest "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", "EndEntry");
                        return null;
                    }, (c) =>
                    {
                        var d = (IDictionary<string, object>)c;
                        if (c.StartEntry == null || !d.ContainsKey("EndEntry"))
                            return null;
                        return new
                        {
                            Request = c.StartEntry.Message.Substring("PCM.OnEnter ".Length),
                            Time = c.StartEntry.Time,
                            Duration = c.EndEntry.Time - c.StartEntry.Time,
                        };
                    });

                foreach (dynamic item in transformedLogFlow)
                {
                    var time = item.Time.ToString("HH:mm:ss.fffff");
                    var req = item.Request;
                    var dt = item.Duration;
                    writer.WriteLine($"{time}\t{dt}\t{req}");
                }
            }

            var expected = string.Join(Environment.NewLine, new[] {
                "02:22:49.34658	00:00:01.7031400	GET http://snbweb01.sn.hu/",
                "02:22:51.29972	00:00:00.0000100	GET http://snbweb01.sn.hu/favicon.ico",
                "02:25:28.25307	00:00:00.0156500	GET http://snbweb01.sn.hu/",
                "02:25:28.47362	00:00:00.0000300	GET http://snbweb01.sn.hu/favicon.ico",
            });

            var actual = sb.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void Analysis2_SimpleMapReduce_Statistics()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(_logForSimpleMapReduce))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Web") //UNDONE: use constants
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect2((e) =>
                    {
                        if (e.Message.StartsWith("PCM.OnEnter "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", "StartEntry");
                        else if (e.Message.StartsWith("PCM.OnEndRequest "))
                            return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", "EndEntry");
                        return null;
                    }, (c) =>
                    {
                        var d = (IDictionary<string, object>)c;
                        if (c.StartEntry == null || !d.ContainsKey("EndEntry"))
                            return null;
                        return new
                        {
                            Request = c.StartEntry.Message.Substring("PCM.OnEnter ".Length),
                            Time = c.StartEntry.Time,
                            Duration = c.EndEntry.Time - c.StartEntry.Time,
                        };
                    })
                    .Statistics<dynamic>(o => o.Request, o => o.Duration.Ticks);

                foreach (dynamic item in transformedLogFlow)
                {
                    var req = item.Key;
                    var count = item.Count;
                    var min = TimeSpan.FromTicks(item.Min);
                    var max = TimeSpan.FromTicks(item.Max);
                    var avg = TimeSpan.FromTicks(Convert.ToInt64(item.Average));
                    writer.WriteLine($"{req}\t{count}\t{min}\t{max}\t{avg}");
                }
            }

            var expected = string.Join(Environment.NewLine, new[] {
                "GET http://snbweb01.sn.hu/	2	00:00:00.0156500	00:00:01.7031400	00:00:00.8593950",
                "GET http://snbweb01.sn.hu/favicon.ico	2	00:00:00.0000100	00:00:00.0000300	00:00:00.0000200",
            });

            var actual = sb.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }
        #region Data for Analysis_SimpleCollect
        private string[] _logForSimpleMapReduce = new[]
        {
            ">1\t2017-11-14 02:22:49.34658\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:8\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/",
            ">12\t2017-11-14 02:22:49.50283\tEvent\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:8\t\t\t\tINFORMATION #57d3ae04-0b56-4fd1-9984-498092df19bd: TemplateReplacers created, see supported templates below.",
            "32\t2017-11-14 02:22:51.04972\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:8\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/",
            "57\t2017-11-14 02:22:51.29972\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:8\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/favicon.ico",
            "60\t2017-11-14 02:22:51.29973\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:8\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/favicon.ico",
            ">2533\t2017-11-14 02:25:19.14251\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:9\t\t\t\tCPU: 0,1836666%, RAM: 607016 KBytes available (working set: 299585536 bytes)",
            ">2534\t2017-11-14 02:25:28.25307\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:10\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/",
            "2537\t2017-11-14 02:25:28.26872\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:10\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/",
            "2538\t2017-11-14 02:25:28.47362\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:6\t\t\t\tPCM.OnEnter GET http://snbweb01.sn.hu/favicon.ico",
            "2541\t2017-11-14 02:25:28.47365\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:6\t\t\t\tPCM.OnEndRequest GET http://snbweb01.sn.hu/favicon.ico",
            ">2542\t2017-11-14 02:25:29.14242\tSystem\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tCPU: 0,1337686%, RAM: 607004 KBytes available (working set: 299618304 bytes)",
        };
        #endregion

        [TestMethod]
        public void Analysis_ComplexCollect()
        {
            var logs = new[] { _log1ForComplexCollectTest, _log2ForComplexCollectTest };

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            using (var logFlow = Reader.Create(logs))
            {
                var aps = new AppDomainSimplifier("App-{0}");

                var transformedLogFlow = logFlow
                    .Where(e => e.Category == "Index" || e.Category == "IndexQueue") //UNDONE: use constants
                    .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                    .Collect<Entry, DistributedIndexingActivityCollection>((e) =>
                    {
                        //       ExecuteDistributedActivity: #6607
                        //       IAQ: A6607 dequeued.
                        // Start IAQ: A6607 EXECUTION.
                        // End   IAQ: A6607 EXECUTION.
                        // -----------------------------
                        //       IAQ: A6607 arrived from another computer.
                        //       IAQ: A6607 dequeued.
                        // Start IAQ: A6607 EXECUTION.
                        // End   IAQ: A6607 EXECUTION.

                        if (e.Message.StartsWith("ExecuteDistributedActivity: #"))
                        {
                            var key = e.Message.Replace("ExecuteDistributedActivity: #", "A");
                            return new Tuple<string, string>(key, DistributedIndexingActivityCollection.Q.Start1);
                        }
                        if (e.Message.StartsWith("IAQ: A"))
                        {
                            if (e.Message.Contains(" arrived from another computer."))
                            {
                                var p = e.Message.IndexOf(" arrived from another computer.");
                                var key = e.Message.Substring(5, p - 5);
                                return new Tuple<string, string>(key, DistributedIndexingActivityCollection.Q.Start2);
                            }
                            if (e.Message.EndsWith(" dequeued."))
                            {
                                var key = e.Message.Substring(5).Replace(" dequeued.", "");
                                return new Tuple<string, string>(key, DistributedIndexingActivityCollection.Q.Dequeue);
                            }
                            if (e.Message.EndsWith(" EXECUTION."))
                            {
                                var key = e.Message.Substring(5).Replace(" EXECUTION.", "");
                                return new Tuple<string, string>(key, e.Status == "Start"
                                    ? DistributedIndexingActivityCollection.Q.ExecStart
                                    : DistributedIndexingActivityCollection.Q.End);
                            }
                        }
                        return null;
                    }
                );

                var items = transformedLogFlow.ToArray();
                Assert.AreEqual(1, items.Length);
                var item = items.First();
                Assert.IsNotNull(item.Start1);
                Assert.IsNotNull(item.Start2);
                Assert.IsNotNull(item.Dequeue1);
                Assert.IsNotNull(item.Dequeue2);
                Assert.IsNotNull(item.ExecStart1);
                Assert.IsNotNull(item.ExecStart2);
                Assert.IsNotNull(item.End1);
                Assert.IsNotNull(item.End2);
            }
        }
        #region Data for Analysis_ComplexCollect
        private string[] _log1ForComplexCollectTest = new[]
        {
            "2708\t2017-11-14 02:25:40.30033\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1274\tStart\t\tSqlProcedure.ExecuteScalar (tran:0): Command: INSERT INTO [IndexingActivities]...",
            "2709\t2017-11-14 02:25:40.30033\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1274\tEnd\t00:00:00.000000\tSqlProcedure.ExecuteScalar (tran:0): Command: INSERT INTO [IndexingActivities]...",
            "2710\t2017-11-14 02:25:40.30033\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tExecuteDistributedActivity: #5127",
            "2711\t2017-11-14 02:25:40.30033\tMessaging\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tSending a 'SenseNet.ContentRepository.Search.Indexing.Activities.AddDocumentActivity' message",
            "2712\t2017-11-14 02:25:40.30033\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tIAQ: A5127 arrived. AddDocumentActivity, /root/benchmark/systemfolder-20171114022535/test500b.txt",
            "2713\t2017-11-14 02:25:40.30033\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tIAQ: A5127 enqueued.",
            "2714\t2017-11-14 02:25:40.30033\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tIAQ: A5127 blocks the T33",
            "2715\t2017-11-14 02:25:40.30033\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tIAQ: A5127 dequeued.",
            "2716\t2017-11-14 02:25:40.30033\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1275\tStart\t\tIAQ: A5127 EXECUTION.",
            "2717\t2017-11-14 02:25:40.30033\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tLM: AddDocumentActivity: [5254/3473], /root/benchmark/systemfolder-20171114022535/test500b.txt. ActivityId:5127, ExecutingUnprocessedActivities:False",
            "2718\t2017-11-14 02:25:40.30033\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1276\tStart\t\tLM: DocumentIndexingActivity.CreateDocument (VersionId:3473)",
            "2719\t2017-11-14 02:25:40.30033\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1276\tEnd\t00:00:00.000000\tLM: DocumentIndexingActivity.CreateDocument (VersionId:3473)",
            "2720\t2017-11-14 02:25:40.31596\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tIAQ: waiting resource released T33.",
            "2722\t2017-11-14 02:25:40.31596\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tIndexingActivity A5127 finished.",
            "2721\t2017-11-14 02:25:40.31596\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1273\tEnd\t00:00:00.015625\tDocumentPopulator.CommitPopulateNode. Version: V1.0.A, VersionId: 3473, Path: /Root/Benchmark/SystemFolder-20171114022535/Test500B.txt",
            "2723\t2017-11-14 02:25:40.31596\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tIAQ: State after finishing A5127: 5127()",
            "2724\t2017-11-14 02:25:40.31596\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tLM: ActivityFinished: 5127",
            "2725\t2017-11-14 02:25:40.31596\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1272\tEnd\t00:00:00.015625\tIndexing node",
            "2726\t2017-11-14 02:25:40.31596\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1262\tEnd\t00:00:00.093748\tSaving Node#0, /Root/Benchmark/SystemFolder-20171114022535/Test500B.txt",
            "2727\t2017-11-14 02:25:40.31596\tDatabase\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1261\tEnd\t00:00:00.093748\tSaveNodeData",
            "2728\t2017-11-14 02:25:40.31596\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tLM: WriteActivityStatusToIndex: 5127()",
            "2729\t2017-11-14 02:25:40.31596\tSecurity\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1277\tStart\t\tCreateSecurityEntity id:5254, parent:5253, owner:1",
            "2730\t2017-11-14 02:25:40.31596\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1278\tStart\t\tLM: Commit. reopenReader:True",
            "2731\t2017-11-14 02:25:40.31596\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tLM: Committing_writer. commitState: 5127()",
            "2732\t2017-11-14 02:25:40.31596\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tSAQ: SA6974 arrived. CreateSecurityEntityActivity",
            "2733\t2017-11-14 02:25:40.31596\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tSAQ: SA6974 enqueued.",
            "2734\t2017-11-14 02:25:40.31596\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tSAQ: SA6974 dequeued.",
            "2735\t2017-11-14 02:25:40.31596\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1279\tStart\t\tSAQ: EXECUTION START SA6974 .",
            "2736\t2017-11-14 02:25:40.33158\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\tOp:1279\tEnd\t00:00:00.015626\tSAQ: EXECUTION START SA6974 .",
            "2737\t2017-11-14 02:25:40.33158\tSecurity\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1277\tEnd\t00:00:00.015626\tCreateSecurityEntity id:5254, parent:5253, owner:1",
            "2738\t2017-11-14 02:25:40.33158\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:32\t\t\t\tSAQ: State after finishing SA6974: 6974()",
            "2739\t2017-11-14 02:25:40.33158\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tNode created. Id:5254, Path:/Root/Benchmark/SystemFolder-20171114022535/Test500B.txt",
            "2740\t2017-11-14 02:25:40.33158\tEvent\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tAudit: ContentCreated, Id:5254, Path:/Root/Benchmark/SystemFolder-20171114022535/Test500B.txt",
            "2741\t2017-11-14 02:25:40.34721\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1280\tStart\t\tLM: ReopenReader",
            "2742\t2017-11-14 02:25:40.34721\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1259\tEnd\t00:00:00.140626\tNODE.SAVE Id: 0, VersionId: 0, Version: V1.0.A, Name: Test500B.txt, ParentPath: /Root/Benchmark/SystemFolder-20171114022535",
            "2743\t2017-11-14 02:25:40.34721\tContentOperation\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\tOp:1258\tEnd\t00:00:00.140626\tGC.Save: Mode:RaiseVersion, VId:0, Path:/Root/Benchmark/SystemFolder-20171114022535/File-20171114022540",
            "2744\t2017-11-14 02:25:40.34721\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\t\t\t\tRecently used reader frames from last reopening reader: 1",
            "2745\t2017-11-14 02:25:40.34721\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1280\tEnd\t00:00:00.000000\tLM: ReopenReader",
            "2746\t2017-11-14 02:25:40.34721\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1278\tEnd\t00:00:00.031252\tLM: Commit. reopenReader:True",
            "2747\t2017-11-14 02:25:40.34721\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:35\tOp:1275\tEnd\t00:00:00.046877\tIAQ: A5127 EXECUTION.",
            "2748\t2017-11-14 02:25:40.36283\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tPortalAuthenticationModule.OnEndRequest. Url:http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?metadata=no, StatusCode:200",
            "2749\t2017-11-14 02:25:40.36283\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997594246478\tT:33\t\t\t\tPCM.OnEndRequest POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?metadata=no",
        };
        private string[] _log2ForComplexCollectTest = new[]
        {
            ">776\t2017-11-14 02:25:40.31170\tMessaging\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:23\t\t\t\tReceived a 'SenseNet.ContentRepository.Search.Indexing.Activities.AddDocumentActivity' message.",
            "777\t2017-11-14 02:25:40.38982\tMessaging\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:21\t\t\t\tProcessing a 'SenseNet.ContentRepository.Search.Indexing.Activities.AddDocumentActivity' message. IsMe: False",
            "778\t2017-11-14 02:25:40.38982\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:21\t\t\t\tIAQ: A5127 arrived from another computer. AddDocumentActivity, /root/benchmark/systemfolder-20171114022535/test500b.txt",
            "779\t2017-11-14 02:25:40.38982\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:21\t\t\t\tIAQ: A5127 enqueued.",
            "780\t2017-11-14 02:25:40.38982\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:31\t\t\t\tIAQ: A5127 dequeued.",
            "781\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:339\tStart\t\tIAQ: A5127 EXECUTION.",
            "782\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tLM: AddDocumentActivity: [5254/3473], /root/benchmark/systemfolder-20171114022535/test500b.txt. ActivityId:5127, ExecutingUnprocessedActivities:False",
            "783\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:340\tStart\t\tLM: DocumentIndexingActivity.CreateDocument (VersionId:3473)",
            "784\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:340\tEnd\t00:00:00.000000\tLM: DocumentIndexingActivity.CreateDocument (VersionId:3473)",
            "785\t2017-11-14 02:25:40.38982\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tIndexingActivity A5127 finished.",
            "786\t2017-11-14 02:25:40.38982\tIndexQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tIAQ: State after finishing A5127: 5127()",
            "787\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tLM: ActivityFinished: 5127",
            "788\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tLM: WriteActivityStatusToIndex: 5127()",
            "789\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:341\tStart\t\tLM: Commit. reopenReader:True",
            "790\t2017-11-14 02:25:40.38982\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tLM: Committing_writer. commitState: 5127()",
            "791\t2017-11-14 02:25:40.43669\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:14\t\t\t\tSAQ: SA6974 arrived from another computer. CreateSecurityEntityActivity",
            "792\t2017-11-14 02:25:40.43669\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:14\t\t\t\tSAQ: SA6974 enqueued.",
            "793\t2017-11-14 02:25:40.43669\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:6\t\t\t\tSAQ: SA6974 dequeued.",
            "794\t2017-11-14 02:25:40.43669\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:12\tOp:342\tStart\t\tSAQ: EXECUTION START SA6974 .",
            "795\t2017-11-14 02:25:40.43669\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:12\tOp:342\tEnd\t00:00:00.000000\tSAQ: EXECUTION START SA6974 .",
            "796\t2017-11-14 02:25:40.43669\tSecurityQueue\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:12\t\t\t\tSAQ: State after finishing SA6974: 6974()",
            "797\t2017-11-14 02:25:40.43669\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:343\tStart\t\tLM: ReopenReader",
            "798\t2017-11-14 02:25:40.43669\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\t\t\t\tRecently used reader frames from last reopening reader: 0",
            "799\t2017-11-14 02:25:40.43669\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:343\tEnd\t00:00:00.000000\tLM: ReopenReader",
            "800\t2017-11-14 02:25:40.43669\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:341\tEnd\t00:00:00.046868\tLM: Commit. reopenReader:True",
            "801\t2017-11-14 02:25:40.43669\tIndex\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:25\tOp:339\tEnd\t00:00:00.046868\tIAQ: A5127 EXECUTION.",
            "802\t2017-11-14 02:25:40.56170\tWeb\tA:/LM/W3SVC/9/ROOT-1-131550997621776476\tT:31\t\t\t\tPCM.OnEnter POST http://snbweb02.sn.hu/OData.svc/Root/Benchmark/SystemFolder-20171114022535?benchamrkId=P5A5x",
        };
        #endregion

        /* ============================================================================== Remote */

        [TestMethod]
        public void Analysis_Remote_SimpleRead()
        {
            var server = new AnalyzatorServer("uri?", new InMemoryEntryReader(_logForSimpleRead));
            var entries = server.Entries.Skip(4).Take(3).ToArray();

            Assert.AreEqual(3, entries.Length);
            Assert.AreEqual(5, entries[0].LineId);
            Assert.AreEqual(6, entries[1].LineId);
            Assert.AreEqual(7, entries[2].LineId);
        }

        [TestMethod]
        public void Analysis_Remote_SimpleCollect()
        {
            var server = new AnalyzatorServer("uri?", new InMemoryEntryReader(_logForSimpleCollectTest));
            var logFlow = server.Entries
                .Where(e => e.Category == "Web") //UNDONE: use constants
                .Collect2((e) =>
                {
                    if (e.Message.StartsWith("PCM.OnEnter "))
                        return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEnter ".Length)}", "StartEntry");
                    else if (e.Message.StartsWith("PCM.OnEndRequest "))
                        return new Tuple<string, string>($"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("PCM.OnEndRequest ".Length)}", "EndEntry");
                    return null;
                }, (c) =>
                {
                    var d = (IDictionary<string, object>)c;
                    if (c.StartEntry == null || !d.ContainsKey("EndEntry"))
                        return null;
                    return new
                    {
                        Time = c.StartEntry.Time,
                        Request = c.StartEntry.Message.Substring("PCM.OnEnter ".Length),
                        Duration = c.EndEntry.Time - c.StartEntry.Time,
                    };
                })
                .ToArray();

            string actual;
            using (var writer = new StringWriter())
            {
                foreach (dynamic item in logFlow)
                {
                    var time = item.Time.ToString("HH:mm:ss.fffff");
                    var req = item.Request;
                    var dt = item.Duration;
                    writer.WriteLine($"{time}\t{dt}\t{req}");
                }
                actual = writer.GetStringBuilder().ToString().Trim();
            }

            var expected = string.Join(Environment.NewLine, new[] {
                    "02:25:28.25307	00:00:00.0156500	GET http://snbweb01.sn.hu/",
                    "02:25:28.47362	00:00:00	GET http://snbweb01.sn.hu/favicon.ico",
                    "02:25:34.95093	00:00:02.9687600	POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark?benchamrkId=P5A0x",
                    "02:25:38.55033	00:00:00.8593700	POST http://snbweb01.sn.hu/OData.svc/Root/Benchmark/('SystemFolder-20171114022535')/Upload?create=1&metadata=no",
                });
            Assert.AreEqual(expected, actual);
        }

    }
}
