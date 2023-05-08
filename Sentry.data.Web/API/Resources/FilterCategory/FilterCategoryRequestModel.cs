using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class FilterCategoryRequestModel : BaseFilterCategoryModel
    {
        public List<FilterCategoryOptionRequestModel> CategoryOptions { get; set; }
    }
}