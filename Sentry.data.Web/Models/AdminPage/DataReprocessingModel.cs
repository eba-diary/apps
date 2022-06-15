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
            allDatasets = null;
           
            //available schema depend on dataset selection, will be added to existing list when a schema is selected. Complete population method when test data available
            schemas = populateSchemas("PlaceHolder");
            //available flowSteps depend on schema selection, will be added to existing list when a schema is selected. Complete population method when test data available
            flowSteps = populateFlowSteps("PlaceHolder");
        }
        public List<SelectListItem> allDatasets { get; set; }
        public List<String> schemas { get; set; }
        public List<String> flowSteps { get; set; }
        public List<String> populateSchemas(String dataSet)
        {
            schemas = new List<String>();
            //for each loop to add all schemaID associated with selected dataset ID
            return schemas;
        }
        public List<String> populateFlowSteps(String schema)
        { 
            flowSteps = new List<String>();
            //for each loop to add all flow steps associated with selected schema ID
            return flowSteps;
        }



    }
}