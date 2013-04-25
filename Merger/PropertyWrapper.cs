using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Merger
{
    public interface IPropertyWrapper<T>
    {
        string Name { get; }

        void Copy(T source, T destination);

        bool AreEqual(T source, T destination, out Conflict conflict);

        bool IsDefaultValueOn(T instance);
    }

    public static class PropertyWrapper
    {
        #region PropertyInfo -> Expression -> delegate

        private static readonly Dictionary<Type, Dictionary<Type, Delegate>> _cachedCreateFromPropertyMethods = new Dictionary<Type, Dictionary<Type, Delegate>>();

        private static readonly MethodInfo _createGetterExpressionMethodInfo = typeof(PropertyWrapper).GetMethod("CreateFromPropertyInfo", BindingFlags.Static | BindingFlags.NonPublic);

        private static Func<PropertyInfo, ISoftEqualityComparerProvider, IPropertyWrapper<T>> GetMethodDelegate<T>(Type propertyType)
        {
            var type = typeof(T);

            Dictionary<Type, Delegate> methods;
            if (!_cachedCreateFromPropertyMethods.TryGetValue(type, out methods))
            {
                methods = new Dictionary<Type, Delegate>();
                _cachedCreateFromPropertyMethods.Add(type, methods);
            }

            Delegate methodDelegate;
            if (!methods.TryGetValue(propertyType, out methodDelegate))
            {
                var methodInfo = _createGetterExpressionMethodInfo.MakeGenericMethod(type, propertyType);
                methodDelegate = Delegate.CreateDelegate(typeof(Func<PropertyInfo, ISoftEqualityComparerProvider, IPropertyWrapper<T>>), methodInfo);
                methods.Add(propertyType, methodDelegate);
            }

            return (Func<PropertyInfo, ISoftEqualityComparerProvider, IPropertyWrapper<T>>)methodDelegate;
        }

        private static IPropertyWrapper<T> CreateFromPropertyInfo<T, TProperty>(PropertyInfo propertyInfo, ISoftEqualityComparerProvider comparerProvider)
        {
            var instance = Expression.Parameter(typeof(T), "instance");

            var lambdaExpression =
                Expression.Lambda<Func<T, TProperty>>(
                    Expression.Property(instance, propertyInfo),
                    instance);

            return Create(lambdaExpression, comparerProvider);
        }

        #endregion

        public static IPropertyWrapper<T> Create<T>(PropertyInfo propertyInfo, ISoftEqualityComparerProvider comparerProvider = null)
        {
            var methodDelegate = GetMethodDelegate<T>(propertyInfo.PropertyType);

            return methodDelegate(propertyInfo, comparerProvider ?? new DefaultSoftEqualityComparerProvider());
        }

        public static IPropertyWrapper<T> Create<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression, ISoftEqualityComparerProvider comparerProvider)
        {
            return new PropertyWrapper<T, TProperty>(propertyExpression, comparerProvider.Get<TProperty>());
        }

        public static IPropertyWrapper<T> Create<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression, ISoftEqualityComparer<TProperty> comparer = null)
        {
            return new PropertyWrapper<T, TProperty>(propertyExpression, comparer);
        }
    }

    public class PropertyWrapper<T, TProperty> : IPropertyWrapper<T>
    {
        public string Name { get; private set; }

        private Action<T, TProperty> _setter;
        private Func<T, TProperty> _getter;

        private ISoftEqualityComparer<TProperty> _softComparer;
        private ISoftEqualityComparer<TProperty> _comparerForDefaultValueComparison;

        public PropertyWrapper(Expression<Func<T, TProperty>> propertyExpression, ISoftEqualityComparer<TProperty> comparer = null)
        {
            #region Validate parameters

            if (!propertyExpression.IsMemberExpression())
                throw new ArgumentException("must be a member access expression", "propertyExpression");

            var memberExpression = (MemberExpression)propertyExpression.Body;

            if (!propertyExpression.IsPropertyExpression())
                throw new ArgumentException("must be a property access expression", "propertyExpression");

            #endregion

            _comparerForDefaultValueComparison = DefaultSoftEqualityComparer<TProperty>.Instance;
            _softComparer = comparer ?? DefaultSoftEqualityComparer<TProperty>.Instance;
            Name = memberExpression.Member.Name;

            #region Setup getter and setter

            _getter = propertyExpression.Compile();

            var instance = Expression.Parameter(typeof(T), "instance");
            var value = Expression.Parameter(typeof(TProperty), "value");

            var lambdaExpression =
                Expression.Lambda<Action<T, TProperty>>(
                    Expression.Assign(
                        Expression.Property(instance, (PropertyInfo)memberExpression.Member),
                        value),
                    instance, value);

            _setter = lambdaExpression.Compile();

            #endregion
        }

        public void Copy(T source, T destination)
        {
            var value = _getter(source);
            _setter(destination, value);
        }

        public bool AreEqual(T source, T destination, out Conflict conflict)
        {
            var sourceProperty = _getter(source);
            var destinationProperty = _getter(destination);

            var areEqual = _softComparer.Equals(sourceProperty, destinationProperty);

            if (!areEqual)
                conflict = new Conflict(Name, sourceProperty.ToString(), destinationProperty.ToString());
            else
                conflict = null;

            return areEqual;
        }

        public bool IsDefaultValueOn(T instance)
        {
            var value = _getter(instance);

            return value == null
                || IsStringDefaultValue(value)
                || _comparerForDefaultValueComparison.Equals(value, default(TProperty));
        }

        private bool IsStringDefaultValue(TProperty value)
        {
            var str = value as string;

            return str != null && string.IsNullOrWhiteSpace(str);
        }
    }
}
