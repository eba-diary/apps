using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class SearchGlobalDatasetsRequestModel : IRequestModel
    {
        public string SearchText { get; set; }
        public List<FilterCategoryRequestModel> FilterCategories { get; set; } = new List<FilterCategoryRequestModel>();
    }
}