using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterSearchField : Attribute
    {
        public string FilterCategoryName { get; set; }
        public bool IsPinnedFilter { get; set; }

        public FilterSearchField(string filterCategoryName) : this(filterCategoryName, false)
        {

        }

        public FilterSearchField(string filterCategoryName, bool isPinnedFilter)
        {
            FilterCategoryName = filterCategoryName;
            IsPinnedFilter = isPinnedFilter;
        }
    }
}
