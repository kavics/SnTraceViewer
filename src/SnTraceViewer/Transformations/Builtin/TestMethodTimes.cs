using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewer.Transformations.Builtin
{
    public class TestMethodTimes : NativeTransformation
    {
        protected override IEnumerable<object> Transform(IEnumerable<Entry> input)
        {
            var collector = new Collector();

            var aps = new AppDomainSimplifier("App-{0}");

            var transformedLogFlow = Input
                .Where(e => e.Category == Category.Test)
                .Select(e => { e.AppDomain = aps.Simplify(e.AppDomain); return e; })
                .Where((e) =>
                {
                    if (e.Message.StartsWith("START test: "))
                    {
                        var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("START test: ".Length)}";
                        collector.Set(key, "start", e);
                        return false;
                    }
                    if (e.Message.StartsWith("END test: "))
                    {
                        var key = $"{e.AppDomain}|{e.ThreadId}|{e.Message.Substring("END test: ".Length)}";
                        // false if the [key] does not exist.
                        return collector.Finish(key, e);
                    }
                    return false;
                })
                .Select(e => new
                {
                    Name = e.Message.Substring("END test: ".Length),
                    Duration = e.Time - e.Associations["start"].Time,
                });

            return transformedLogFlow;
        }
    }
}
