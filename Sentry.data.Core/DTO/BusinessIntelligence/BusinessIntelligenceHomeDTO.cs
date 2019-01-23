using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceHomeDto
    {
        public int DatasetCount { get; set; }
        public List<Category> Categories { get; set; }
        public Boolean CanEditDataset { get; set; }
        public Boolean CanUpload { get; set; }
        public Boolean CanManageReports { get; set; }
    }
}
