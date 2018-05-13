using SenseNet.Diagnostics.Analysis2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewer
{
    public interface ITransformation
    {
        string Name { get; }
        string[] ColumnNames { get; }
        IEnumerable<Entry> Input { get; }
        IEnumerable<object> Output { get; }
    }

    public abstract class NativeTransformation : ITransformation
    {
        private string __name;
        public virtual string Name { get => __name ?? (__name = GetType().Name); }

        private string[] __columnNames;
        public virtual string[] ColumnNames { get => __columnNames ?? (__columnNames = GetColumnNames()); }
        protected virtual string[] GetColumnNames()
        {
            var input = Input;
            if (input == null)
                return new string[0];

            var firstItem = input.FirstOrDefault();
            if (firstItem == null)
                return new string[0];

            return firstItem.GetType().GetProperties()
                .Select(p => p.Name)
                .ToArray();
        }

        public IEnumerable<Entry> Input { get; protected set; }

        private IEnumerable<object> _cachedOutput;
        public IEnumerable<object> Output { get => _cachedOutput ?? (_cachedOutput = Transform(Input)); }
        protected abstract IEnumerable<object> Transform(IEnumerable<Entry> input);
    }

    public class TestMethodTimes : NativeTransformation
    {
        public TestMethodTimes(IEnumerable<Entry> input)
        {
            Input = input;
        }

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
