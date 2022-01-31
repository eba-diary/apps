using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DaleSearchDto
    {
        public string Criteria { get; set; }
        public DaleDestiny Destiny { get; set; }
        public DaleSensitive Sensitive { get; set; }
        public DaleAdvancedCriteriaDto AdvancedCriteria { get; set; }
        public List<FilterCategoryDto> FilterCategories { get; set; } = new List<FilterCategoryDto>();
        
        public bool HasFilterFor(string category, string value)
        {
            return FilterCategories?.Any(f => f.CategoryName == category && f.CategoryOptions?.Any(x => x.OptionValue == value) == true) == true;
        }

        public string CriteriaToString()
        {
            if (Destiny == DaleDestiny.Advanced && AdvancedCriteria != null)
            {
                return AdvancedCriteria.ToEventString();
            }

            List<string> searchCriteria = new List<string>();

            if (!string.IsNullOrWhiteSpace(Criteria))
            {
                searchCriteria.Add(Criteria);
            }

            if (FilterCategories?.Any() == true)
            {
                searchCriteria.AddRange(FilterCategories?.Select(x => x.CategoryName + ":" + string.Join(" OR ", x.CategoryOptions?.Where(w => w.Selected).Select(s => s.OptionValue))));
            }
            return string.Join(" AND ", searchCriteria);
        }
    }
}
