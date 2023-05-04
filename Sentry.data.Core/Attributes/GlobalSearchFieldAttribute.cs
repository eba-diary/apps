using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GlobalSearchFieldAttribute : Attribute
    {
        public double? Boost { get; set;  }

        public GlobalSearchFieldAttribute() { }
        public GlobalSearchFieldAttribute(double boost) 
        {
            Boost = boost;
        }
    }
}
