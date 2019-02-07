using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class SearchIndexModel
    {
        public SearchIndexModel()
        {
            this.SortByOptions = Helpers.Utility.BuildDatasetSortByOptions();
        }

        public List<SelectListItem> SortByOptions { get; set; }

    }
}