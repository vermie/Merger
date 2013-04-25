using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Merger
{
    public static class ExpressionExtensions
    {
        public static bool IsMemberExpression<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            return expression.Body.NodeType == ExpressionType.MemberAccess;
        }

        public static bool IsPropertyExpression<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            return expression.IsMemberExpression() && ((MemberExpression)expression.Body).Member is PropertyInfo;
        }
    }
}
