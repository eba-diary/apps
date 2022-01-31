using static Sentry.data.Core.GlobalConstants;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterSearchModel
    {
        public string SearchText { get; set; }
        public List<FilterCategoryModel> FilterCategories { get; set; } = new List<FilterCategoryModel>();

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