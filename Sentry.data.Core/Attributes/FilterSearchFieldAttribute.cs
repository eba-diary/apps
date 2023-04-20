using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterSearchFieldAttribute : Attribute
    {
        public string FilterCategoryName { get; set; }
        public bool IsPinnedFilter { get; set; }
        public bool DefaultOpen { get; set; }
        public bool HideResultCounts { get; set; }

        public FilterSearchFieldAttribute(string filterCategoryName, bool isPinnedFilter = false, bool defaultOpen = false, bool hideResultCounts = false)
        {
            FilterCategoryName = filterCategoryName;
            IsPinnedFilter = isPinnedFilter;
            DefaultOpen = defaultOpen;
            HideResultCounts = hideResultCounts;
        }

        public FilterSearchFieldAttribute()
        {
            //for use by nested object parent
        }
    }
}
