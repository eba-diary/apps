using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sentry.data.Web
{

    public class AnonymousTypeUtils
    {
        public static T GetValueFromAnonymousType<T>(dynamic item, string key)
        {
            Type type = item.GetType();
            PropertyInfo prop = type.GetProperty(key);
            if (prop == null)
            {
                return default(T);
            }

            dynamic value = (T)prop.GetValue(item, null);
            return value;
        }

        public static Dictionary<string, T> GetDictionaryFromAnonymousType<T>(object item)
        {
            Type t = item.GetType();
            return t.GetProperties().ToDictionary(((key) => key.Name), ((value) => (T)value.GetValue(item)));
        }
    }
}
