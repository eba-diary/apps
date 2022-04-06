using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sentry.data.Core
{
    public static class CustomAttributeHelper
    {
        public static bool TryGetFilterCategoryName<T>(string categoryName, out string result)
        {
            if (TryGetFilterSearchFieldProperty<T>(categoryName, out PropertyInfo property))
            {
                result = property.GetCustomAttribute<FilterSearchField>().FilterCategoryName;
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryGetFilterSearchFieldProperty<T>(string categoryName, out PropertyInfo property)
        {
            property = GetPropertiesWithAttribute<T, FilterSearchField>().FirstOrDefault(x => x.GetCustomAttribute<FilterSearchField>().FilterCategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            return property != null;
        }
        
        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T, attrT>()
        {
            return typeof(T).GetProperties().Where(p => Attribute.IsDefined(p, typeof(attrT)));
        }
    }
}
