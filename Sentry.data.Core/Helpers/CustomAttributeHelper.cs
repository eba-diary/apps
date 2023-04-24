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
                result = property.GetCustomAttribute<FilterSearchFieldAttribute>().FilterCategoryName;
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryGetFilterSearchFieldProperty<T>(string categoryName, out PropertyInfo property)
        {
            property = GetPropertiesWithAttribute<T, FilterSearchFieldAttribute>().FirstOrDefault(x => x.GetCustomAttribute<FilterSearchFieldAttribute>().FilterCategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            return property != null;
        }
        
        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T, attrT>()
        {
            return GetPropertiesWithAttribute<attrT>(typeof(T));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<attrT>(Type type)
        {
            return type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(attrT)));
        }
    }
}
