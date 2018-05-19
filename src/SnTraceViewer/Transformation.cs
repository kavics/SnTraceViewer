using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewer
{
    public abstract class Transformation
    {
        public abstract string Name { get; }

        private string[] __columnNames;
        public string[] ColumnNames { get => __columnNames ?? (__columnNames = GetColumnNames()); }
        protected virtual string[] GetColumnNames()
        {
            var output = Output;
            if (output == null)
                return new string[0];

            var firstItem = output.FirstOrDefault();
            if (firstItem == null)
                return new string[0];

            return firstItem.GetType().GetProperties()
                .Select(p => p.Name)
                .ToArray();
        }

        private IEnumerable<Entry> _input;
        public IEnumerable<Entry> Input
        {
            get { return _input; }
            set
            {
                _input = value;
                Output = Transform(value);
            }
        }

        public IEnumerable<object> Output { get; private set; }

        protected abstract IEnumerable<object> Transform(IEnumerable<Entry> input);
    }

    public abstract class NativeTransformation : Transformation
    {
        public override string Name => GetType().Name;
    }

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
