using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public class CompareResult<T>
    {
        public T Instance { get; private set; }
        public IEnumerable<Conflict> Conflicts { get; private set; }

        public CompareResult(T instance, IEnumerable<Conflict> conflicts)
        {
            Instance = instance;
            Conflicts = conflicts;
        }
    }
}
