using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web.Models.AdminPage
{
    public class DataReprocessingModel
    {
        public DataReprocessingModel()
        {
            //ID set to fill value section of html selection element
            datasetIds = null;
            //remember to clean schema and flowSteps every time a new dataSet is selected, or these will keep growing based on current plan.
            datasets = null;
            //available schema depend on dataset selection, will be added to existing list when a schema is selected. Complete population method when test data available
            schemas = populateSchemas("PlaceHolder");
            //available flowSteps depend on schema selection, will be added to existing list when a schema is selected. Complete population method when test data available
            flowSteps = populateFlowSteps("PlaceHolder");
        }
        public List<String> datasets { get; set; }
        public List<int> datasetIds { get; set; }
        public List<String> schemas { get; set; }
        public List<String> flowSteps { get; set; }

        public void populateDatasets(List<DatasetDto> dtoList) {
            datasets = new List<String>();
            foreach(DatasetDto d in dtoList)
            {
                datasets.Add(d.DatasetName);
            }
        }
        public void populateDatasetIds(List<DatasetDto> dtoList)
        {
            datasetIds = new List<int>();
            foreach(DatasetDto d in dtoList)
            {
                datasetIds.Add(d.DatasetId);
            }
        }
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