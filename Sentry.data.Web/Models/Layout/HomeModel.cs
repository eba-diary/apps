using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class HomeModel
    {
        public HomeModel()
        {

        }

        public int DatasetCount { get; set; }
        public List<Category> Categories { get; set; }
        public Boolean CanEditDataset { get; set; }
    }
}
