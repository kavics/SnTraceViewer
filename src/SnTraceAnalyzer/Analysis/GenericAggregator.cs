using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SenseNet.Diagnostics.Analysis
{
    public class Statistics<T> where T : IConvertible, IComparable
    {
        public string Key { get; set; }
        public int Count { get; private set; }
        public T Min { get; private set; }
        public T Max { get; private set; }
        public double Average { get; private set; }

        public void Aggregate(T value)
        {
            if (Count == 0)
            {
                Min = value;
                Max = value;
                Average = Convert.ToDouble(value);
                Count = 1;
                return;
            }
            if (value.CompareTo(Min) < 0) Min = value;
            if (value.CompareTo(Max) > 0) Max = value;
            Average = Average + (Convert.ToDouble(value) - Average) / ++Count;
        }
    }

    internal class GenericAggregator<TSource, TResult> : IEnumerable<Statistics<TResult>> where TResult : IConvertible, IComparable
    {
        private IEnumerable<TSource> _input;
        private Func<TSource, string> _keySelector;
        private Func<TSource, TResult> _valueSelector;

        public GenericAggregator(Func<TSource, string> keySelector, Func<TSource, TResult> valueSelector)
        {
            _keySelector = keySelector;
            _valueSelector = valueSelector;
        }
        public void Initialize(IEnumerable<TSource> input)
        {
            _input = input;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<Statistics<TResult>> GetEnumerator()
        {
            return new GenericAggregatorEnumerator(_input, _keySelector, _valueSelector);
        }

        private class GenericAggregatorEnumerator : IEnumerator<Statistics<TResult>>
        {
            private IEnumerable<TSource> _input;
            private Func<TSource, string> _keySelector;
            private Func<TSource, TResult> _valueSelector;
            private int _currentIndex;
            private Statistics<TResult>[] _aggregations;

            public GenericAggregatorEnumerator(IEnumerable<TSource> _input, Func<TSource, string> _keySelector, Func<TSource, TResult> _valueSelector)
            {
                this._input = _input;
                this._keySelector = _keySelector;
                this._valueSelector = _valueSelector;
            }

            object IEnumerator.Current { get { return Current; } }
            public Statistics<TResult> Current
            {
                get { return _aggregations[_currentIndex]; }
            }

            public void Dispose()
            {
                // do nothing
            }
            public void Reset()
            {
                throw new NotImplementedException();
            }

            public bool MoveNext()
            {
                if(_aggregations == null)
                {
                    var aggregations  = new Dictionary<string, Statistics<TResult>>();
                    foreach (var item in _input)
                    {
                        var key = _keySelector(item);
                        var value = _valueSelector(item);

                        Statistics<TResult> stat;
                        if(!aggregations.TryGetValue(key, out stat))
                        {
                            stat = new Statistics<TResult> { Key = key };
                            aggregations.Add(key, stat);
                        }
                        stat.Aggregate(value);
                    }
                    _aggregations = aggregations.Values.ToArray();
                    _currentIndex = -1;
                }
                _currentIndex++;
                return _currentIndex < _aggregations.Length;
            }
        }
    }
}