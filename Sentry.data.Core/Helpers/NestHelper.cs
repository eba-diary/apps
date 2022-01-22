using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sentry.data.Core
{
    public static class NestHelper
    {
        public static Nest.Fields SearchFields<T>()
        {
            return Infer.Fields(GetGlobalSearchFieldProperties<T>().ToArray());
        }

        public static Dictionary<string, Field> FilterCategoryFields<T>()
        {
            Dictionary<string, Field> categoryFields = new Dictionary<string, Field>();

            foreach (PropertyInfo property in GetGlobalSearchFieldProperties<T>())
            {
                categoryFields.Add(property.GetCustomAttribute<GlobalSearchField>().FilterCategoryName, Infer.Field(property));
            }

            return categoryFields;
        }

        private static IEnumerable<PropertyInfo> GetGlobalSearchFieldProperties<T>()
        {
            return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(GlobalSearchField)));
        }
    }
}
