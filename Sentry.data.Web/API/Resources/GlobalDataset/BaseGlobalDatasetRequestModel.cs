using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class BaseGlobalDatasetRequestModel : IRequestModel
    {
        public string SearchText { get; set; }
        public List<FilterCategoryRequestModel> FilterCategories { get; set; }
        public bool ShouldSearchColumns { get; set; }
    }
}