using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merger
{
    public interface IMatchAlgorithm<T>
    {
        void SetDefaultComparer(ISoftEqualityComparerProvider comparerProvider);

        int CalclateMatchIndex(T instance1, T instance2);

        void AddMatchEvaluator<TProperty>(int weight, Func<T, TProperty> propertyAccessor, ISoftEqualityComparer<TProperty> comparer = null);
    }

    public class MatchAlgorithm<T> : IMatchAlgorithm<T>
    {
        private ISoftEqualityComparerProvider _comparerProvider;

        public MatchAlgorithm()
        {
            _matchEvaluators = new List<IMatchEvaluator<T>>();
            _comparerProvider = new DefaultSoftEqualityComparerProvider();
        }

        public int CalclateMatchIndex(T instance1, T instance2)
        {
            if (object.ReferenceEquals(instance1, instance2))
                return int.MaxValue;

            return _matchEvaluators.Sum(e => e.Evaluate(instance1, instance2));
        }

        private List<IMatchEvaluator<T>> _matchEvaluators;

        public void AddMatchEvaluator<TProperty>(int weight, Func<T, TProperty> propertyAccessor, ISoftEqualityComparer<TProperty> comparer = null)
        {
            var matchEvaluator = new MatchEvaluator<T, TProperty>(propertyAccessor, weight, comparer ?? _comparerProvider.Get<TProperty>());
            _matchEvaluators.Add(matchEvaluator);
        }

        public void SetDefaultComparer(ISoftEqualityComparerProvider comparerProvider)
        {
            _comparerProvider = comparerProvider;
        }
    }
}
