using System;

namespace Sentry.data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GlobalSearchField : Attribute
    {
        public string FilterCategoryName { get; set; }

        public GlobalSearchField()
        {

        }

        public GlobalSearchField(string filterCategoryName)
        {
            FilterCategoryName = filterCategoryName;
        }
    }
}
