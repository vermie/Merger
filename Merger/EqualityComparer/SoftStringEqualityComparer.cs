using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    [Flags]
    public enum StringComparisonOptions
    {
        CaseInsensitive = 0x01,
        IgnoreWhitespace = 0x02,
    }

    public class SoftStringEqualityComparer : ISoftEqualityComparer<string>
    {
        private StringComparisonOptions _stringOptions;
        private bool _nullEqualsAnything;

        public SoftStringEqualityComparer()
        {
        }

        public SoftStringEqualityComparer(StringComparisonOptions stringOptions)
        {
            _stringOptions = stringOptions;
        }

        public SoftStringEqualityComparer(StringComparisonOptions stringOptions, bool nullEqualsAnything)
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
}
