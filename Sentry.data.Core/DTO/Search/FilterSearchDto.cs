using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class FilterSearchDto : DaleEventableDto
    {
        public List<FilterCategoryDto> FilterCategories { get; set; } = new List<FilterCategoryDto>();
    }
}
