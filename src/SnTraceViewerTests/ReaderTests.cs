using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SnTraceViewer.Analysis;

namespace SnTraceViewerTests
{
    [TestClass]
    public class ReaderTests
    {
        [TestMethod]
        public void Reader_SearchTraceDirectories()
        {
            var expectedPaths = new[]
            {
                 GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.ContentRepository.Tests\bin\Debug\App_Data\DetailedLog"),
                 GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.Search.IntegrationTests\bin\Debug\App_Data\DetailedLog"),
                 GetFullPath(@"..\..\SampleFilesForSnTraceView\SenseNet.SearchImpl.Tests\bin\Debug\App_Data\DetailedLog"),
            };

            var actualPaths = Reader.SearchTraceDirectories(GetFullPath(@"..\..\SampleFilesForSnTraceView"));

            Assert.AreEqual(3, actualPaths.Length);
            for (var i = 0; i < 3; i++)
                Assert.AreEqual(expectedPaths[i], actualPaths[i]);
        }

        /* ============================================================================ */

        private string GetFullPath(string relativePath)
        {
            var x = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
            return x;
        }

    }
}
