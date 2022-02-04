using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class FilterCategoryDto
    {
        public string CategoryName { get; set; }
        public List<FilterCategoryOptionDto> CategoryOptions { get; set; } = new List<FilterCategoryOptionDto>();
    }
}
