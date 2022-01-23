using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web
{
    public class FilterSearchModel
    {
        public string SearchText { get; set; }
        public List<FilterCategoryModel> FilterCategories { get; set; } = new List<FilterCategoryModel>();
        public string IconPath { get; set; }
        public string PageTitle { get; set; }
        public string ResultView { get; set; }
        
        public bool IsValid(bool canViewSensitive)
        {
            if (!canViewSensitive)
            {
                FilterCategories.RemoveAll(x => x.CategoryName == FilterCategoryNames.SENSITIVE);

                FilterCategoryModel category = new FilterCategoryModel() { CategoryName = FilterCategoryNames.SENSITIVE };
                category.CategoryOptions.Add(new FilterCategoryOptionModel() { OptionValue = "D", Selected = true, ParentCategoryName = category.CategoryName });
            }

            //must have search text or at least 1 selected filter to search on
            return !string.IsNullOrWhiteSpace(SearchText) || FilterCategories?.Any(x => x.CategoryOptions?.Any(o => o.Selected) == true) == true;
        }
    }
}