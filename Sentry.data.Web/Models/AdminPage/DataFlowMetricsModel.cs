using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class DataFlowMetricsModel
    {
        public List<SelectListItem> DatasetsList { get; set; }
    }
}