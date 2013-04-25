using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Merger
{
    public interface ICompareAlgorithm<T>
    {
        void SetDefaultComparer(ISoftEqualityComparerProvider comparerProvider);

        void IgnoreProperty<TProperty>(Expression<Func<T, TProperty>> propertyAccessor);
        void ForProperty<TProperty>(Expression<Func<T, TProperty>> propertyAccessor, ISoftEqualityComparer<TProperty> comparer = null);

        IEnumerable<Conflict> Compare(T source, T destination);
        void MergeMissing(T source, T destination);
        void MergeAll(T source, T destination);
    }

    public class CompareAlgorithm<T, TKey> : ICompareAlgorithm<T>
    {
        Func<T, TKey> KeyAccessor { get; set; }

        private HashSet<string> _ignoredProperties;
        private Dictionary<string, IPropertyWrapper<T>> _propertyWrappers;
        private ISoftEqualityComparerProvider _comparerProvider;

        public CompareAlgorithm(Expression<Func<T, TKey>> keyAccessor, bool ignoreKey)
        {
            _ignoredProperties = new HashSet<string>();
            _propertyWrappers = new Dictionary<string, IPropertyWrapper<T>>(StringComparer.Ordinal);
            _comparerProvider = new DefaultSoftEqualityComparerProvider();

            KeyAccessor = keyAccessor.Compile();

            if (ignoreKey)
                IgnoreProperty(keyAccessor);
        }

        #region Default comparer types

        public void SetDefaultComparer(ISoftEqualityComparerProvider comparerProvider)
        {
            _comparerProvider = comparerProvider;
        }

        #endregion

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.Equals(typeof(string)) ||
                   type.Equals(typeof(DateTime)) ||
                   type.Equals(typeof(Decimal)) ||
                   type.Equals(typeof(Guid)) ||
                   type.Equals(typeof(DateTimeOffset)) ||
                   type.Equals(typeof(TimeSpan));
        }

        private bool _discoveredProperties = false;
        private void DiscoverProperties()
        {
            if (_discoveredProperties)
                return;

            _discoveredProperties = true;

            var type = typeof(T);

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (!IsSimpleType(property.PropertyType))
                    continue;

                var propertyWrapper = PropertyWrapperHelper.Create<T>(property, _comparerProvider);
                Add(propertyWrapper, false);
            }
        }

        public void ForProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression, ISoftEqualityComparer<TProperty> comparer = null)
        {
            var propertyWrapper = PropertyWrapperHelper.Create(propertyExpression, comparer ?? _comparerProvider.Get<TProperty>());
            Add(propertyWrapper, true);
        }

        private void Add(IPropertyWrapper<T> propertyWrapper, bool overwriteExisting)
        {
            if (overwriteExisting || !_propertyWrappers.ContainsKey(propertyWrapper.Name))
                _propertyWrappers[propertyWrapper.Name] = propertyWrapper;
        }

        public void IgnoreProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (!propertyExpression.IsPropertyExpression())
                throw new ArgumentException("must be a property access expression", "propertyExpression");

            var memberExpression = (MemberExpression)propertyExpression.Body;
            _ignoredProperties.Add(memberExpression.Member.Name);
        }

        private IEnumerable<IPropertyWrapper<T>> GetPropertiesToMerge()
        {
            DiscoverProperties();

            return _propertyWrappers.Values.Where(w => !_ignoredProperties.Contains(w.Name));
        }

        public IEnumerable<Conflict> Compare(T source, T destination)
        {
            var propertiesToMerge = GetPropertiesToMerge();

            var conflicts = new List<Conflict>();

            foreach (var propertyWrapper in propertiesToMerge)
            {
                Conflict conflict;
                if (!propertyWrapper.AreEqual(source, destination, out conflict))
                    conflicts.Add(conflict);
            }

            return conflicts;
        }

        public void MergeMissing(T source, T destination)
        {
            var propertiesToMerge = GetPropertiesToMerge();

            foreach (var propertyWrapper in propertiesToMerge)
            {
                if (propertyWrapper.IsDefaultValueOn(destination))
                    propertyWrapper.Copy(source, destination);
            }
        }

        public void MergeAll(T source, T destination)
        {
            var propertiesToMerge = GetPropertiesToMerge();

            foreach (var propertyWrapper in propertiesToMerge)
            {
                propertyWrapper.Copy(source, destination);
            }
        }
    }
}
