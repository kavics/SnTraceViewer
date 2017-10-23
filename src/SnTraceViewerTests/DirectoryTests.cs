using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SnTraceViewer.Analysis;

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
                 GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.ContentRepository.Tests\bin\Debug\App_Data\DetailedLog"),
                 GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.Search.IntegrationTests\bin\Debug\App_Data\DetailedLog"),
                 GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.SearchImpl.Tests\bin\Debug\App_Data\DetailedLog"),
            };

            var actualPaths = TraceDirectory.SearchTraceDirectories(GetFullPath(@"..\..\SampleFilesForSnTraceView"));

            Assert.AreEqual(3, actualPaths.Length);
            for (var i = 0; i < 3; i++)
                Assert.AreEqual(expectedPaths[i], actualPaths[i].Path);
        }
    }
}
