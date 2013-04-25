using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    internal class DefaultSoftEqualityComparer<T> : ISoftEqualityComparer<T>
    {
        private static readonly DefaultSoftEqualityComparer<T> _instance = new DefaultSoftEqualityComparer<T>();
        public static DefaultSoftEqualityComparer<T> Instance { get { return _instance; } }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }
}
