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
                Expression<Func<T, object>> expression = Expression.Lambda<Func<T, object>>(Expression.PropertyOrField(Expression.Parameter(typeof(T)), property.Name));
                categoryFields.Add(property.GetCustomAttribute<FilterSearchField>().FilterCategoryName, Infer.Field(expression.AppendSuffix("keyword")));
            }

            return categoryFields;
        }

        private static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T, attrT>()
        {
            return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(attrT)));
        }
    }
}
