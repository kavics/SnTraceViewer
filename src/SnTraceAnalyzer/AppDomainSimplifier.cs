using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis2
{
    public class AppDomainSimplifier
    {
        private readonly string _format;
        private List<string> _keys = new List<string>();

        public AppDomainSimplifier(string format = null)
        {
            _format = format ?? "App-{0}";
        }

        public string Simplify(string key)
        {
            var i = _keys.IndexOf(key);
            if (i < 0)
            {
                i = _keys.Count;
                _keys.Add(key);
            }
            return string.Format(_format, (i + 1));
        }
    }
}
