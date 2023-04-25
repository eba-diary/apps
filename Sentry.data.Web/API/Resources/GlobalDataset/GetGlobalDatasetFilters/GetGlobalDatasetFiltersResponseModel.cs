using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class GetGlobalDatasetFiltersResponseModel : IResponseModel
    {
        public List<FilterCategoryResponseModel> FilterCategories { get; set; }
    }
}