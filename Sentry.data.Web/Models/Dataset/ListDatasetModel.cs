using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ListDatasetModel
    {
        public ListDatasetModel()
        {
            this.CategoryList = new List<string>();
            this.DatasetList = new List<BaseDatasetModel>();
        }

        public IList<String> CategoryList { get; set; }

        public IList<BaseDatasetModel> DatasetList { get; set; }

    }
}
