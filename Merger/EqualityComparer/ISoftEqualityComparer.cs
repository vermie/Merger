using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public interface ISoftEqualityComparer<T>
    {
        bool Equals(T x, T y);
    }
}
