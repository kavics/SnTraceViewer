using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using SenseNet.Diagnostics.Analysis;
using System.Linq;
using System.Collections.Generic;

namespace TransformerTests
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
                    .Where(e => e.Category == "Web")
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
                    .Where(e => e.Category == "Web")
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
    }
}
