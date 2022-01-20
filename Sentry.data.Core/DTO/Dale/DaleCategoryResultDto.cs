using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DaleCategoryResultDto : DaleEventableDto
    {
        public List<DaleCategoryDto> DaleCategories { get; set; }
    }
}
