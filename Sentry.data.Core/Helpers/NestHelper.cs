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
        public static Nest.Fields SearchFields<T>()
        {
            return Infer.Fields(GetPropertiesWithAttribute<T, GlobalSearchField>().ToArray());
        }

        public static Dictionary<string, Field> FilterCategoryFields<T>() where T : class
        {
            Dictionary<string, Field> categoryFields = new Dictionary<string, Field>();

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
                
                categoryFields.Add(property.GetCustomAttribute<FilterSearchField>().FilterCategoryName, field);
            }

            return categoryFields;
        }

        public static Field FilterCategoryField<T>(string categoryName)
        {
            PropertyInfo property = GetPropertiesWithAttribute<T, FilterSearchField>().FirstOrDefault(x => x.GetCustomAttribute<FilterSearchField>().FilterCategoryName == categoryName);

            if (property == null)
            {
                throw new InvalidOperationException($"Filter category name: {categoryName} does not match any category names registered to type of: {typeof(T).Name}");
            }

            return Infer.Field(property);
        }

        public static Field BuildExpression<T, TValue>(PropertyInfo property) where T : class
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            Expression<Func<T, TValue>> expression = Expression.Lambda<Func<T, TValue>>(Expression.Property(parameter, property.Name), parameter);

            return Infer.Field(expression.AppendSuffix("keyword"));
        }

        private static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T, attrT>()
        {
            return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(attrT)));
        }
    }
}
