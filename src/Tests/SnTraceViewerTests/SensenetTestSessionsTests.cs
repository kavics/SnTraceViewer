﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Diagnostics.Analysis2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewerTests
{
    [DebuggerDisplay("#{Index}: {FileNames.Count} files. {FirstTime} - {LastTime}.")]
    public class SessionInfo
    {
        /*
        DIRECTORIES

        D1
            F1	 0,  1
            F2	10, 12
            F3	14, 16
        D2
            F1	11, 13
            F2	15, 17
            F3	20, 21

        FILES

                0,  1,  ...    10, 11, 12, 13, 14, 15, 16, 17,   ...    20, 21
        D1/F1   |====|
        D1/F2                  |=========|
        D1/F3                                  |========|
        D2/F1                      |=========|
        D2/F2                                      |=========|
        D2/F3                                                           |====|

        SESSIONS

        S1      |====|
        S2                     |=============================|
        S3                                                              |====|
        */



        public int Index { get; set; }
        public DateTime FirstTime => FirstEntry.Time;
        public DateTime LastTime => LastEntry.Time;
        //public IEnumerable<TraceFile> Files { get; } = new List<TraceFile>();
        public List<string> FileNames { get; } = new List<string>();
        public Entry FirstEntry { get; set; }
        public Entry LastEntry { get; set; }

        public static SessionInfo[] SearchSessions(string searchRoot)
        {
            var traceDirs = TraceDirectory.SearchTraceDirectories(searchRoot);

            var files = traceDirs
                .Where(d => d.TraceFiles.Count > 0)
                .SelectMany(d => d.TraceFiles)
                .Where(f => f.EntryCount > 0)
                .OrderBy(f => f.FirstEntry.Time)
                .ToArray();

            var sessions = new List<SessionInfo>();
            var session = new SessionInfo { Index = 0 };
            sessions.Add(session);
            session.FileNames.Add(files[0].Path);

            for (int i = 1; i < files.Length; i++)
            {
                var lastFile = files[i - 1];
                var currentFile = files[i];

                if (SameSession(lastFile, currentFile))
                {
                    session.FileNames.Add(currentFile.Path);
                }
                else
                {
                    session = new SessionInfo { Index = session.Index + 1 };
                    sessions.Add(session);
                    session.FileNames.Add(currentFile.Path);
                }
            }

            return sessions.ToArray();
        }

        private static TimeSpan _continuousTimeLimit = TimeSpan.FromSeconds(60);
        private static bool SameSession(TraceFile lastFile, TraceFile currentFile)
        {
            var sameDir = Path.GetDirectoryName(lastFile.Path) == Path.GetDirectoryName(currentFile.Path);
            var continuous = lastFile.LastEntry.LineId == currentFile.FirstEntry.LineId - 1;
            var sameApp = lastFile.FirstEntry.AppDomain == currentFile.FirstEntry.AppDomain;
            var dt = currentFile.FirstEntry.Time - lastFile.LastEntry.Time;
            var sameTime = dt >= TimeSpan.Zero && dt < _continuousTimeLimit;

            if (!sameApp)
                return false;
            if (!sameTime)
                return false;
            if (!continuous && sameDir)
                return false;

            return true;
        }
    }

    [TestClass]
    public class SensenetTestSessionsTests
    {
        private string SearchRoot = @"D:\Dev10\github\sensenet\src";

        [TestMethod]
        public void _Search()
        {
            var sessions = SessionInfo.SearchSessions(SearchRoot);

            Assert.Inconclusive();
        }

    }
}
