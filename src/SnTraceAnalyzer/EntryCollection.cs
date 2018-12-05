using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    public abstract class EntryCollection<T>
    {
        public abstract void Add(T entry, string qualification);
        public abstract bool Finished();
    }
}
