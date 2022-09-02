using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class FilterCategoryModel
    {
        public string CategoryName { get; set; }
        public List<FilterCategoryOptionModel> CategoryOptions { get; set; } = new List<FilterCategoryOptionModel>();
        public bool DefaultCategoryOpen { get; set; }
        public bool HideResultCounts { get; set; }
    }
}