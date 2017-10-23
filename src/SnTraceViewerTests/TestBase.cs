using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewerTests
{
    public abstract class TestBase
    {
        protected string GetFullPath(string relativePath)
        {
            var x = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
            return x;
        }
    }
}
