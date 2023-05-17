using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DocumentsFiltersDto<T>
    {
        public List<T> Documents { get; set; }
        public List<FilterCategoryDto> FilterCategories { get; set; }
    }
}
