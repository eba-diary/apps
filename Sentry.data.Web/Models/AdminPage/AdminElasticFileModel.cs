using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class AdminElasticFileModel
    {
        public long CompletedFiles { get; set; }

        public long InFlightFiles { get; set; }

        public long FailedFiles { get; set; }

        public bool CLA4553_FeatureFlag { get; set; }
        public bool CLA5112_FeatureFlag { get; set; }
    }
}