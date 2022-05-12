using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterSearchModel
    {
        public string SearchName { get; set; }
        public string SearchText { get; set; }
        public List<FilterCategoryModel> FilterCategories { get; set; } = new List<FilterCategoryModel>();
    }
}