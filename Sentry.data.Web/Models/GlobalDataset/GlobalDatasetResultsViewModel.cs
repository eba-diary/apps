using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class GlobalDatasetResultsViewModel
    {
        public List<SelectListItem> PageSizeOptions { get; set; }
        public List<SelectListItem> SortByOptions { get; set; }
        public List<SelectListItem> LayoutOptions { get; set; }
        public List<PageItemModel> PageItems { get; set; }
        public bool ShouldSearchColumns { get; set; }
        public List<GlobalDatasetViewModel> GlobalDatasets { get; set; }
    }
}