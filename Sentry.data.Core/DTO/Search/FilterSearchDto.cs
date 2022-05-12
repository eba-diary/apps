using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class FilterSearchDto : DaleEventableDto
    {
        public string SearchName { get; set; }
        public string SearchText { get; set; }
        public List<FilterCategoryDto> FilterCategories { get; set; } = new List<FilterCategoryDto>();
    }
}
