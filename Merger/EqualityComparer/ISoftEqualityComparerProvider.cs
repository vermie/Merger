using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public interface ISoftEqualityComparerProvider
    {
        ISoftEqualityComparer<T> Get<T>();
    }
}
