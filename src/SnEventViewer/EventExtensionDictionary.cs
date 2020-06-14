using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnEventViewer
{
    public class EventExtensionDictionary:IDictionary<string, string>
    {
        private Action __changed;
        private Dictionary<string, string> _data = new Dictionary<string, string>();

        public EventExtensionDictionary(Action changed)
        {
            __changed = changed;
        }

        private void Changed()
        {
            __changed?.Invoke();
        }

        private ICollection<KeyValuePair<string, string>> _collection =>
            (ICollection<KeyValuePair<string, string>>) _data;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            _collection.Add(item);
            Changed();
        }

        public void Clear()
        {
            _data.Clear();
            Changed();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            return _collection.Contains(item);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            Changed();
            return _collection.Remove(item);
        }

        public int Count => _data.Count;
        public bool IsReadOnly => _collection.IsReadOnly;
        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public void Add(string key, string value)
        {
            _data.Add(key, value);
            Changed();
        }

        public bool Remove(string key)
        {
            Changed();
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _data.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get => _data[key];
            set
            {
                _data[key] = value;
                Changed();
            }
        }

        public ICollection<string> Keys => _data.Keys;
        public ICollection<string> Values => _data.Values;


        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in _data)
                sb.Append(item.Key).AppendLine(item.Value);
            return sb.ToString();
        }
    }
}
