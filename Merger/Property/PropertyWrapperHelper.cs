using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Merger
{
    internal static class PropertyWrapperHelper
    {
        #region PropertyInfo -> Expression -> delegate

        private static readonly Dictionary<Type, Dictionary<Type, Delegate>> _cachedCreateFromPropertyMethods = new Dictionary<Type, Dictionary<Type, Delegate>>();

        private static readonly MethodInfo _createGetterExpressionMethodInfo = typeof(PropertyWrapperHelper).GetMethod("CreateFromPropertyInfo", BindingFlags.Static | BindingFlags.NonPublic);

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
}
