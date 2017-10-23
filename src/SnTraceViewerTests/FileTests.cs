using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnTraceViewer.Analysis;

namespace SnTraceViewerTests
{
    [TestClass]
    public class FileTests : TestBase
    {
        [TestMethod]
        public void File_Scanning()
        {
            var file = new TraceFile(GetFullPath(@"..\..\..\SnTraceViewer\SampleFiles\detailedlog_20171015-062009Z.log"));

            Assert.AreEqual(22, file.EntryCount);
            Assert.AreEqual(1, file.Errors);
            Assert.AreEqual(4, file.UnterminatedLines);
            Assert.AreEqual("ContentOperation, Index, Repository, Test", string.Join(", ", file.Categories));
        }
    }
}
