using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GlobalSearchFieldAttribute : Attribute
    {
        public double? Boost { get; set;  }
        public string DisplayName { get; set; }

        public GlobalSearchFieldAttribute() { }

        public GlobalSearchFieldAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public GlobalSearchFieldAttribute(string displayName, double boost) : this(displayName)
        {
            Boost = boost;
        }
    }
}
