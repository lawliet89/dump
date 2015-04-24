using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BinarySearchTree
{
    public static class ExpressionExtensions
    {
        public static TResult Value<TObject, TResult>(this Expression<Func<TObject, TResult>> expression, TObject obj)
        {
            return expression.Compile()(obj);
        }

        public static void SetValue<TObject, TResult>(this Expression<Func<TObject, TResult>> expression, TObject obj,
            TResult value)
        {
            var selector = expression.Body as MemberExpression;
            if (selector != null)
            {
                var property = selector.Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(obj, value, null);
                    return;
                }
                throw new ArgumentException("Expression provided is not a property expression");
            }
            throw new ArgumentException("Expression provided is not a Member Expression");
        }
    }
}
