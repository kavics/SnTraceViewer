//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SenseNet.Diagnostics.Analysis2
//{
//    public abstract class EntryCollection<T>
//    {
//        public abstract void Add(T entry, string qualification);
//        public abstract bool Finished();
//    }

//    public class GenericCollector<TSource, TResult> : IEnumerable<TResult> where TResult : EntryCollection<TSource>, new()
//    {
//        private IEnumerable<TSource> _input;
//        private readonly Func<TSource, Tuple<string, string>> _keySelector;
//        private readonly Func<TResult, TResult> _finalizer;
//        private static readonly Func<TResult, TResult> DefaultFinalizer = (c) => { return c.Finished() ? c : null; };

//        public GenericCollector(Func<TSource, Tuple<string, string>> keySelector, Func<TResult, TResult> finalizer = null)
//        {
//            _keySelector = keySelector;
//            _finalizer = finalizer ?? DefaultFinalizer;
//        }
//        public void Initialize(IEnumerable<TSource> input)
//        {
//            _input = input;
//        }
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }
//        public IEnumerator<TResult> GetEnumerator()
//        {
//            foreach (TSource entry in _input)
//            {
//                var keySelectorResult = _keySelector(entry);
//                TResult collection;
//                TResult outputEntry;
//                if (keySelectorResult != null)
//                {
//                    var key = keySelectorResult.Item1;
//                    var qualification = keySelectorResult.Item2;
//                    collection = Collect<TResult>(key, qualification, entry);
//                    if (collection != null)
//                    {
//                        outputEntry = _finalizer(collection);
//                        //outputEntry = Build<TResult>(collection);
//                        if (outputEntry != null)
//                        {
//                            RemoveCollection(key);
//                            yield return outputEntry;
//                        }
//                    }
//                }
//            }
//        }

//        private T Collect<T>(string key, string qualification, TSource entry) where T : EntryCollection<TSource>, new()
//        {
//            var collection = GetCollection<T>(key);
//            collection.Add(entry, qualification);
//            return collection;
//        }

//        private Dictionary<string, EntryCollection<TSource>> Collections { get; } = new Dictionary<string, EntryCollection<TSource>>();
//        internal T GetCollection<T>(string key) where T : EntryCollection<TSource>, new()
//        {
//            EntryCollection<TSource> collection;
//            if (!Collections.TryGetValue(key, out collection))
//            {
//                collection = new T();
//                Collections[key] = collection;
//            }
//            return (T)collection;
//        }
//        internal void RemoveCollection(string key)
//        {
//            Collections.Remove(key);
//        }

//    }
//}
