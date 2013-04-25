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

    public interface ISoftEqualityComparerProvider
    {
        ISoftEqualityComparer<T> Get<T>();
    }

    public class DefaultSoftEqualityComparerProvider : ISoftEqualityComparerProvider
    {
        public virtual ISoftEqualityComparer<T> Get<T>()
        {
            return DefaultSoftEqualityComparer<T>.Instance;
        }
    }

    internal class DefaultSoftEqualityComparer<T> : ISoftEqualityComparer<T>
    {
        private static readonly DefaultSoftEqualityComparer<T> _instance = new DefaultSoftEqualityComparer<T>();
        public static DefaultSoftEqualityComparer<T> Instance { get { return _instance; } }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }

    [Flags]
    public enum StringComparisonOptions
    {
        CaseInsensitive = 0x01,
        IgnoreWhitespace = 0x02,
    }

    public class StringSoftEqualityComparer : ISoftEqualityComparer<string>
    {
        private StringComparisonOptions _stringOptions;
        private bool _nullEqualsAnything;

        public StringSoftEqualityComparer()
        {
        }

        public StringSoftEqualityComparer(StringComparisonOptions stringOptions)
        {
            _stringOptions = stringOptions;
        }

        public StringSoftEqualityComparer(StringComparisonOptions stringOptions, bool nullEqualsAnything)
            : this(stringOptions)
        {
            _nullEqualsAnything = nullEqualsAnything;
        }

        public bool Equals(string x, string y)
        {
            if (_nullEqualsAnything)
            {
                if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y))
                    return true;
            }
            if (_stringOptions.HasFlag(StringComparisonOptions.IgnoreWhitespace))
            {
                if (x != null)
                    x = x.Trim();
                if (y != null)
                    y = y.Trim();
            }

            var comparer = _stringOptions.HasFlag(StringComparisonOptions.CaseInsensitive)
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            return comparer.Equals(x, y);
        }
    }

    public class NullAwareSoftEqualityComparer<T> : ISoftEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if (x == null || y == null)
                return true;

            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }
}
