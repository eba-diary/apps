using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class FilterCategoryResponseModel : BaseFilterCategoryModel
    {
        public List<FilterCategoryOptionResponseModel> CategoryOptions { get; set; }
        public bool DefaultCategoryOpen { get; set; }
        public bool HideResultCounts { get; set; }
    }
}