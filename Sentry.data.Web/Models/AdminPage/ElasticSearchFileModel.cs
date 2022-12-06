using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.AdminPage
{
    public class ElasticSearchFileModel
    {
        public int CompletedFiles { get; set; }

        public int InFlightFiles { get; set; }

        public int FailedFiles { get; set; }
    }
}