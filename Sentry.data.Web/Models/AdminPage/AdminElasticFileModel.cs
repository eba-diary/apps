using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class AdminElasticFileModel
    {
        public int CompletedFiles { get; set; }

        public int InFlightFiles { get; set; }

        public int FailedFiles { get; set; }

        public bool FeatureFlag { get; set; }
    }
}