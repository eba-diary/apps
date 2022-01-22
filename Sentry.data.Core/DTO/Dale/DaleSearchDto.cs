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
        public List<FilterCategoryDto> Filters { get; set; }
        
        public bool HasFilterFor(string category, string value)
        {
            return Filters?.Any(f => f.CategoryName == category && f.CategoryOptions?.Any(x => x.OptionValue == value) == true) == true;
        }
    }
}
