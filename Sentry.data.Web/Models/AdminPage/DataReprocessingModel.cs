using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
namespace Sentry.data.Web
{
    public class DataReprocessingModel
    {
        public List<string> SelectedFiles { get; set; }
        public List<SelectListItem> AllDatasets { get; set; }
        public List<string> Schema { get; set; }
        public List<string> FlowSteps { get; set; }


    }
}