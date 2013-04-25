using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    internal interface IMatchEvaluator<T>
    {
        int Evaluate(T x, T y);
    }

    internal class MatchEvaluator<T, TProperty> : IMatchEvaluator<T>
    {
        private Func<T, TProperty> _propertyAccessor;
        private int _weight;
        private ISoftEqualityComparer<TProperty> _comparer;

        public MatchEvaluator(Func<T, TProperty> keyAccessor, int weight, ISoftEqualityComparer<TProperty> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException();

            _propertyAccessor = keyAccessor;
            _weight = weight;
            _comparer = comparer;
        }

        public int Evaluate(T x, T y)
        {
            return _comparer.Equals(_propertyAccessor(x), _propertyAccessor(y))
                    ? _weight
                    : 0;
        }
    }
}
