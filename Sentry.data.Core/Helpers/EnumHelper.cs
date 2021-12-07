using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sentry.data.Core.Helpers
{
    public static class EnumHelper
    {
        public static T GetByDescription<T>(string description) where T : Enum
        {
            Type enumType = typeof(T);            
            FieldInfo[] fields = enumType.GetFields();

            foreach (FieldInfo field in fields)
            {
                IEnumerable<DescriptionAttribute> attributes = field.GetCustomAttributes<DescriptionAttribute>();
                if (attributes?.Any() == true && description.Equals(attributes.First().Description, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null);
                }
            }

            return default;
        }
    }
}
