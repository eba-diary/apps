using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class BusinessIntelligenceHomeModel
    {
        public BusinessIntelligenceHomeModel()
        {
            Categories = new List<Category>();
        }

        public int DatasetCount { get; set; }
        public List<Category> Categories { get; set; }
        public Boolean CanEditDataset { get; set; }
        public Boolean CanUpload { get; set; }
    }
}
