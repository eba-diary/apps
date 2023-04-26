using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class BaseGlobalDatasetPageDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public int Layout { get; set; }
        public List<SearchGlobalDatasetDto> GlobalDatasets { get; set; }
    }
}
