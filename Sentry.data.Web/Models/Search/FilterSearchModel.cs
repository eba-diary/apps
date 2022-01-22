using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class FilterSearchModel
    {
        public string SearchText { get; set; }
        public List<FilterCategoryModel> FilterCategories { get; set; }
        public string IconPath { get; set; }
        public string PageTitle { get; set; }
        public string ResultView { get; set; }

        public DaleSearchDto ToDto()
        {
            DaleSearchDto dto = new DaleSearchDto()
            {
                Criteria = SearchText,
                Filters = FilterCategories?.Select(x => x.ToDto()).ToList()
            };

            return dto;
        }        
    }
}