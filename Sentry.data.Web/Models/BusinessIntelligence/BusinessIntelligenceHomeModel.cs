using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class BusinessIntelligenceHomeModel
    {
        public int DatasetCount { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public Boolean CanEditDataset { get; set; }
        public Boolean CanUpload { get; set; }
        public Boolean CanManageReports { get; set; }
    }
}
