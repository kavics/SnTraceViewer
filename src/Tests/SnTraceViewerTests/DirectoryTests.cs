using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using SenseNet.Diagnostics.Analysis;

namespace SnTraceViewerTests
{
    [TestClass]
    public class DirectoryTests : TestBase
    {
        [TestMethod]
        public void Directory_Search()
        {
            var expectedPaths = new[]
            {
                 TraceDirectory.GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.ContentRepository.Tests\bin\Debug\App_Data\DetailedLog"),
                 TraceDirectory.GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.Search.IntegrationTests\bin\Debug\App_Data\DetailedLog"),
                 TraceDirectory.GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.SearchImpl.Tests\bin\Debug\App_Data\DetailedLog"),
            };

            var actualDirs = TraceDirectory.SearchTraceDirectories(TraceDirectory.GetFullPath(@"..\..\SampleFilesForSnTraceView"));

            Assert.AreEqual(3, actualDirs.Length);
            for (var i = 0; i < 3; i++)
                Assert.AreEqual(expectedPaths[i], actualDirs[i].Path);
        }
        [TestMethod]
        public void Directory_GetFiles()
        {
            var rootPath = TraceDirectory.GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.ContentRepository.Tests");
            var traceDir = TraceDirectory.SearchTraceDirectories(rootPath).First();
            var expectedCount = Directory.GetFiles(traceDir.Path).Length;
            if (expectedCount == 0)
                Assert.Inconclusive("Test cannot be conclusive if the directory contains no files.");

            Assert.AreEqual(expectedCount, traceDir.TraceFiles.Count);
        }
        [TestMethod]
        public void Session_GetSessionsFromOneDirectory()
        {
            var rootPath = TraceDirectory.GetFullPath(@"..\..\..\..\SnTraceViewer\SampleFiles\session");
            var traceDirs = TraceDirectory.SearchTraceDirectories(rootPath);
            var sessions = TraceSession.Create(traceDirs);

            Assert.AreEqual(2, sessions.Length);
            Assert.AreEqual(1595, sessions[0].LastLineId);
            Assert.AreEqual(1595, sessions[0].Last().LineId);
            Assert.AreEqual(21, sessions[1].LastLineId);
            Assert.AreEqual(21, sessions[1].Last().LineId);
        }
    }
}
