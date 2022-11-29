using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class EnumDisplayName : Attribute
    {
        public string DisplayName { get; set; }

        public EnumDisplayName(string name) 
        { 
            DisplayName = name;
        }
    }
}
