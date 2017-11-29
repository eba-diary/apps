using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class ListDatasetModel
    {
        public IList<String> CategoryList { get; set; }

        public IList<BaseDatasetModel> DatasetList { get; set; }

        public Boolean CanDwnldSenstive { get; set; }

        public IList<Category> AllCategories { get; set; }

        public IList<string> SentryOwnerList { get; set; }

        public IList<FilterModel> SearchFilters { get; set; }

        //input properties
        public string SearchSentryOwner { get; set; }

        public string SearchCategory { get; set; }

        public string SearchText { get; set; }

        public int SearchFrequencyId { get; set; }
    }
}
