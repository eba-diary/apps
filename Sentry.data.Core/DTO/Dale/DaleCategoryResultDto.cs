using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleCategoryResultDto
    {
        public List<DaleCategoryDto> DaleCategories { get; set; }
        public DaleEventDto DaleEvent { get; set; }
    }
}
