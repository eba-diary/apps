using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class FilterCategoryDto
    {
        public string CategoryName { get; set; }
        public List<FilterCategoryOptionDto> CategoryOptions { get; set; } = new List<FilterCategoryOptionDto>();
        public bool DefaultCategoryOpen { get; set; }
        public bool HideResultCounts { get; set; }

        public List<string> GetSelectedValues()
        {
            return CategoryOptions.Where(x => x.Selected).Select(x => x.OptionValue).ToList();
        }
    }
}
