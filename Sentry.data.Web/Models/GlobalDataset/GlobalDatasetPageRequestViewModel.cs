using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class GlobalDatasetPageRequestViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortBy { get; set; }
        public int Layout { get; set; }
        public List<GlobalDatasetViewModel> GlobalDatasets { get; set; }
    }
}