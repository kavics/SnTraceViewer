//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SenseNet.Diagnostics.Analysis2
//{
//    public class GenericCollector2<TSource> : IEnumerable<object>
//    {
//        private IEnumerable<TSource> _input;
//        private readonly Func<TSource, Tuple<string, string>> _keySelector;
//        private readonly Func<dynamic, dynamic> _finalizer;

//        public GenericCollector2(Func<TSource, Tuple<string, string>> keySelector, Func<dynamic, dynamic> finalizer = null)
//        {
//            _keySelector = keySelector;
//            _finalizer = finalizer;
//        }
//        public void Initialize(IEnumerable<TSource> input)
//        {
//            _input = input;
//        }
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//        public IEnumerator<dynamic> GetEnumerator()
//        {
//            foreach (TSource entry in _input)
//            {
//                var keySelectorResult = _keySelector(entry);
//                dynamic collection;
//                dynamic outputEntry;
//                if (keySelectorResult != null)
//                {
//                    var key = keySelectorResult.Item1;
//                    var qualification = keySelectorResult.Item2;
//                    collection = Collect(key, qualification, entry);
//                    if (collection != null)
//                    {
//                        outputEntry = _finalizer(collection);
//                        if (outputEntry != null)
//                        {
//                            RemoveCollection(key);
//                            yield return outputEntry;
//                        }
//                    }
//                }
//            }
//        }

//        private dynamic Collect(string key, string qualification, TSource entry)
//        {
//            var collection = GetCollection(key);
//            var d = collection as IDictionary<string, object>;
//            d[qualification] = entry;
//            return collection;
//        }

//        private Dictionary<string, dynamic> Collections { get; } = new Dictionary<string, dynamic>();
//        internal dynamic GetCollection(string key)
//        {
//            dynamic collection;
//            if (!Collections.TryGetValue(key, out collection))
//            {
//                collection = new System.Dynamic.ExpandoObject();
//                Collections[key] = collection;
//            }
//            return collection;
//        }
//        internal void RemoveCollection(string key)
//        {
//            Collections.Remove(key);
//        }

//    }
//}
