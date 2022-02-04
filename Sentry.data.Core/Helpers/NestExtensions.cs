using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sentry.data.Core
{
    public static class NestExtensions
    {
        public static void AddWildcard<T>(this List<QueryContainer> container, Expression<Func<T, object>> field, string value) where T : class
        {
            if (TryBuild(field, value, out WildcardQuery query))
            {
                query.Value = $"*{value}*";
                query.CaseInsensitive = true;

                container.Add(query);
            }
        }

        public static void AddMatch<T>(this List<QueryContainer> container, Expression<Func<T, object>> field, string value) where T : class
        {
            container.AddMatch(field, value, false);
        }

        public static void AddFuzzyMatch<T>(this List<QueryContainer> container, Expression<Func<T, object>> field, string value) where T : class
        {
            container.AddMatch(field, value, true);
        }

        private static void AddMatch<T>(this List<QueryContainer> container, Expression<Func<T, object>> field, string value, bool fuzzy) where T : class
        {
            if (TryBuild(field, value, out MatchQuery query))
            {
                query.Query = value;
                if (fuzzy)
                {
                    query.Fuzziness = Fuzziness.Auto;
                }
                container.Add(query);
            }
        }

        private static bool TryBuild<T, T2>(Expression<Func<T2, object>> field, string value, out T result) where T : FieldNameQueryBase where T2 : class
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = Activator.CreateInstance<T>();
                result.Field = Infer.Field(field);
                return true;
            }

            result = default;
            return false;
        }
    }
}
