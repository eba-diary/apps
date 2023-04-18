using Nest;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sentry.data.Core
{
    public static class NestHelper
    {
        public static Nest.Fields GetSearchFields<T>()
        {
            return Infer.Fields(CustomAttributeHelper.GetPropertiesWithAttribute<T, GlobalSearchFieldAttribute>().ToArray());
        }

        public static AggregationDictionary GetFilterAggregations<T>() where T : class
        {
            AggregationDictionary aggregations = new AggregationDictionary();

            foreach (PropertyInfo property in CustomAttributeHelper.GetPropertiesWithAttribute<T, FilterSearchFieldAttribute>())
            {
                FilterSearchFieldAttribute filterAttribute = property.GetCustomAttribute<FilterSearchFieldAttribute>();
                aggregations.Add(filterAttribute.FilterCategoryName, new TermsAggregation(filterAttribute.FilterCategoryName)
                {
                    Field = GetFilterCategoryField<T>(property),
                    Size = filterAttribute.IsPinnedFilter ? 10000 : 15
                });
            }

            return aggregations;
        }

        public static Field GetFilterCategoryField<T>(string categoryName) where T : class
        {
            if (CustomAttributeHelper.TryGetFilterSearchFieldProperty<T>(categoryName, out PropertyInfo property))
            {
                return GetFilterCategoryField<T>(property);
            }

            throw new InvalidOperationException($"Filter category name: {categoryName} does not match any category names registered to type of: {typeof(T).Name}");         
        }

        #region Private
        private static Field GetFilterCategoryField<T>(PropertyInfo property) where T : class
        {
            if (property.PropertyType == typeof(string))
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T));
                Expression<Func<T, object>> expression = Expression.Lambda<Func<T, object>>(Expression.Property(parameter, property.Name), parameter);

                return Infer.Field(expression.AppendSuffix("keyword"));
            }

            return Infer.Field(property);
        }
        #endregion
    }
}
