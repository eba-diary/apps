using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sentry.data.Core
{
    public static class NestHelper
    {
        public static Nest.Fields GetSearchFields<T>()
        {
            return Infer.Fields(GetPropertiesWithAttribute<T, GlobalSearchField>().ToArray());
        }

        public static AggregationDictionary GetFilterAggregations<T>() where T : class
        {
            AggregationDictionary aggregations = new AggregationDictionary();

            foreach (PropertyInfo property in GetPropertiesWithAttribute<T, FilterSearchField>())
            {
                Field field;
                if (property.PropertyType == typeof(string))
                {
                    ParameterExpression parameter = Expression.Parameter(typeof(T));
                    Expression<Func<T, object>> expression = Expression.Lambda<Func<T, object>>(Expression.Property(parameter, property.Name), parameter);

                    field = Infer.Field(expression.AppendSuffix("keyword"));
                }
                else
                {
                    field = Infer.Field(property);
                }

                FilterSearchField filterAttribute = property.GetCustomAttribute<FilterSearchField>();
                aggregations.Add(filterAttribute.FilterCategoryName, new TermsAggregation(filterAttribute.FilterCategoryName)
                {
                    Field = field,
                    Size = filterAttribute.IsPinnedFilter ? 10000 : 15
                });
            }

            return aggregations;
        }

        public static Field GetFilterCategoryField<T>(string categoryName)
        {
            PropertyInfo property = GetPropertiesWithAttribute<T, FilterSearchField>().FirstOrDefault(x => x.GetCustomAttribute<FilterSearchField>().FilterCategoryName == categoryName);

            if (property == null)
            {
                throw new InvalidOperationException($"Filter category name: {categoryName} does not match any category names registered to type of: {typeof(T).Name}");
            }

            return Infer.Field(property);
        }

        private static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T, attrT>()
        {
            return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(attrT)));
        }
    }
}
