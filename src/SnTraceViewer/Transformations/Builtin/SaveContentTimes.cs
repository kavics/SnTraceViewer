using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.Diagnostics.Analysis;

namespace SnTraceViewer.Transformations.Builtin
{
    public class SaveContentTimes : NativeTransformation
    {
        // Start                  GC.Save: Mode:RaiseVersion, VId:0, Path:/Root/SystemFolder-20180429131946
        // End   00:00:00.306672  GC.Save: Mode:RaiseVersion, VId:0, Path:/Root/SystemFolder-20180429131946
        protected override IEnumerable<object> Transform(IEnumerable<Entry> input)
        {
            var collector = new Collector();

            var aps = new AppDomainSimplifier("App-{0}");

            var transformedLogFlow = Input
                .Where(e => e.Category == Category.ContentOperation)
                .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                .Where((e) =>
                {
                    if (e.Message.StartsWith("GC.Save: ") && e.Status == Status.Start)
                    {
                        var key = $"{e.AppDomain}|{e.ThreadId}|{e.OpId}";
                        collector.Set(key, "start", e);
                        return false;
                    }
                    if (e.Message.StartsWith("GC.Save: ") && e.Status == Status.End)
                    {
                        var key = $"{e.AppDomain}|{e.ThreadId}|{e.OpId}";
                        // false if the [key] does not exist.
                        return collector.Finish(key, e);
                    }
                    return false;
                })
                .Select(e => new
                {
                    Name = e.Message.Substring(e.Message.IndexOf("Path:") + 5),
                    Duration = e.Time - e.Associations["start"].Time,
                });

            return transformedLogFlow;
        }
    }
}
