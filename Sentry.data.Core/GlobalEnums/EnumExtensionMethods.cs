using System;
using System.Reflection;
using System.Linq;
using System.ComponentModel;

namespace Sentry.data.Core
{
    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum myEnum)
        {
            Type myEnumType = myEnum.GetType();
            MemberInfo[] memberInfo = myEnumType.GetMember(myEnum.ToString());

            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var attribs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((attribs != null && attribs.Any()))
                {
                    return ((DescriptionAttribute)attribs.ElementAt(0)).Description;
                }
            }
            return myEnum.ToString();
        }

    }
}