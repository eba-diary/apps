using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterSearchField : Attribute
    {
        public string FilterCategoryName { get; set; }

        public FilterSearchField(string filterCategoryName)
        {
            FilterCategoryName = filterCategoryName;
        }
    }
}
