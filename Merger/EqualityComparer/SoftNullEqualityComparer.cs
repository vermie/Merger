using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public class SoftNullEqualityComparer<T> : ISoftEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if (x == null || y == null)
                return true;

            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }
}
