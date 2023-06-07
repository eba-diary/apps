using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class AdminElasticFileModel
    {
        public long TotalCompletedFiles { get; set; }

        public long TotalInFlightFiles { get; set; }

        public long TotalFailedFiles { get; set; }

        public bool CLA4553_FeatureFlag { get; set; }
        public bool CLA5112_FeatureFlag { get; set; }
        public bool CLA5260_FeatureFlag { get; set; }
    }
}