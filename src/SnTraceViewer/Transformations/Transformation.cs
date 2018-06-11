using SenseNet.Diagnostics.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace SnTraceViewer.Transformations
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
}
