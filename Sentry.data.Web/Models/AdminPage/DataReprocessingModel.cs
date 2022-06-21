using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web.Models.AdminPage
{
    public class DataReprocessingModel
    {
        public DataReprocessingModel()
        {
            //All of these are required based on design documents, but the only one used right now is allDatasets
            selectedFiles = null;
            allDatasets = null;
            schema = null;
            flowSteps = null;
        }
        public List<String> selectedFiles { get; set; }
        public List<SelectListItem> allDatasets { get; set; }
        public List<String> schema { get; set; }
        public List<String> flowSteps { get; set; }




    }
}