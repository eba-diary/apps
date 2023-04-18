using System.Collections.Generic;

namespace Sentry.data.Web.API
{
    public class FilterCategoryRequestModel
    {
        public string CategoryName { get; set; }
        public List<FilterCategoryOptionRequestModel> CategoryOptions { get; set; } = new List<FilterCategoryOptionRequestModel>();
    }
}