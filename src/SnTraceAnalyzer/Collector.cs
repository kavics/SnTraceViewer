using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    public class Collector
    {
        public Dictionary<string, Dictionary<string, Entry>> Items { get; } =
            new Dictionary<string, Dictionary<string, Entry>>();

        public Entry Get(string key, string subKey)
        {
            if (!Items.TryGetValue(key, out Dictionary<string, Entry> dict))
                return null;
            if (dict.TryGetValue(key, out Entry entry))
                return entry;
            return null;
        }
        public void Set(string key, string subKey, Entry e)
        {
            if (!Items.TryGetValue(key, out Dictionary<string, Entry> dict))
                Items.Add(key, dict = new Dictionary<string, Entry>());
            dict[subKey] = e;
        }
        public bool Finish(string key, Entry e, bool remove = true)
        {
            if (!Items.TryGetValue(key, out Dictionary<string, Entry> dict))
                return false;
            if (remove)
                Items.Remove(key);
            e.Associations = dict;
            return true;
        }
        public void Remove(string key)
        {
            Items.Remove(key);
        }
    }
}
