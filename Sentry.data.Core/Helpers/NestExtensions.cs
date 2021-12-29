using Nest;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Sentry.data.Core
{
    public static class NestExtensions
    {
        public static void TryAddWildcard<T>(this List<QueryContainer> container, Expression<Func<T, object>> field, string value) where T : class
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                container.Add(new WildcardQuery()
                {
                    Field = Infer.Field(field),
                    Value = $"*{value}*",
                    CaseInsensitive = false
                });
            }
        }
    }
}
