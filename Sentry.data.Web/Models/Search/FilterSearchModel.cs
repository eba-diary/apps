using System.Collections.Generic;
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
        
        public void Validate(bool canViewSensitive)
        {
            if (!canViewSensitive)
            {
                FilterCategories.RemoveAll(x => x.CategoryName == FilterCategoryNames.SENSITIVE);

                FilterCategoryModel category = new FilterCategoryModel() { CategoryName = FilterCategoryNames.SENSITIVE };
                category.CategoryOptions.Add(new FilterCategoryOptionModel() { OptionValue = "D", Selected = true, ParentCategoryName = category.CategoryName });
            }
        }
    }
}