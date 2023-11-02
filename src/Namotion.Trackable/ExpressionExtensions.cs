using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Namotion.Trackable
{
    internal static class ExpressionExtensions
    {
        internal static string GetExpressionPath<TItem, TField>(this Expression<Func<TItem, TField>> fieldSelector)
        {
            var parts = new List<string>();
            var body = fieldSelector.Body;
            while (body is MemberExpression member)
            {
                parts.Add(member.Member.Name);
                body = member.Expression;
            }

            parts.Reverse();
            return string.Join(".", parts);
        }
    }
}
