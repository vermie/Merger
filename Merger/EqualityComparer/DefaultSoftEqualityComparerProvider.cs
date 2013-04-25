using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public class DefaultSoftEqualityComparerProvider : ISoftEqualityComparerProvider
    {
        public virtual ISoftEqualityComparer<T> Get<T>()
        {
            return DefaultSoftEqualityComparer<T>.Instance;
        }
    }
}
