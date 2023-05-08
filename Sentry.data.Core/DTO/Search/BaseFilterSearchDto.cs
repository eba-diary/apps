using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public abstract class BaseFilterSearchDto
    {
        public string SearchText { get; set; }
        public List<FilterCategoryDto> FilterCategories { get; set; } = new List<FilterCategoryDto>();

        public override string ToString()
        {
            List<string> searchCriteria = new List<string>();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                searchCriteria.Add(SearchText);
            }

            if (FilterCategories?.Any() == true)
            {
                searchCriteria.AddRange(FilterCategories?.Select(x => x.CategoryName + ":" + string.Join(" OR ", x.CategoryOptions?.Where(w => w.Selected).Select(s => s.OptionValue))));
            }

            return string.Join(" AND ", searchCriteria);
        }
    }
}
