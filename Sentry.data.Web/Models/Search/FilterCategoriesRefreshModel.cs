using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterCategoriesRefreshModel
    {
        public List<FilterCategoryModel> CurrentFilterCategories { get; set; }
        public List<FilterCategoryModel> ResultFilterCategories { get; set; }
    }
}