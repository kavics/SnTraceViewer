using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis2
{
    public class Collector
    {
        private Dictionary<string, Dictionary<string, Entry>> _storage = new Dictionary<string, Dictionary<string, Entry>>();

        public Entry Get(string key, string subKey)
        {
            if (!_storage.TryGetValue(key, out Dictionary<string, Entry> dict))
                return null;
            if (dict.TryGetValue(key, out Entry entry))
                return entry;
            return null;
        }
        public void Set(string key, string subKey, Entry e)
        {
            if (!_storage.TryGetValue(key, out Dictionary<string, Entry> dict))
                _storage.Add(key, dict = new Dictionary<string, Entry>());
            dict[subKey] = e;
        }
        public bool Finish(string key, Entry e)
        {
            if (!_storage.TryGetValue(key, out Dictionary<string, Entry> dict))
                return false;
            e.Associations = dict;
            return true;
        }
        public void Remove(string key)
        {
            _storage.Remove(key);
        }
    }
}
