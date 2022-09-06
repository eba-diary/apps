using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterSearchField : Attribute
    {
        public string FilterCategoryName { get; set; }
        public bool IsPinnedFilter { get; set; }
        public bool DefaultOpen { get; set; }
        public bool HideResultCounts { get; set; }

        public FilterSearchField(string filterCategoryName, bool isPinnedFilter = false, bool defaultOpen = false, bool hideResultCounts = false)
        {
            FilterCategoryName = filterCategoryName;
            IsPinnedFilter = isPinnedFilter;
            DefaultOpen = defaultOpen;
            HideResultCounts = hideResultCounts;
        }
    }
}
