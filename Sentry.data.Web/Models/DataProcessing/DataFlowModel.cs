using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DataFlowModel
    {
        public DataFlowModel()
        {
            SchemaMaps = new List<SchemaMapModel>();
            IsCompressed = false;
            IsPreProcessingRequired = false;
        }

        public string Name { get; set; }
        /// <summary>
        /// How is data getting into DSC (Push or Pull)
        /// </summary>
        /// 
        [DisplayName("How will data be ingested into DSC?")]
        public IngestionType IngestionType { get; set; }

        public string SelectedIngestionType { get; set; }

        /// <summary>
        /// Is the incoming data compressed?
        /// </summary>
        /// 
        [DisplayName("Is incoming data compressed?")]
        public bool IsCompressed { get; set; }

        public bool IsPreProcessingRequired { get; set; }
        /// <summary>
        /// Target
        /// </summary>
        public int SchemaId { get; set; }

        [DisplayName("Where should this data be loaded?")]
        public List<SchemaMapModel> SchemaMaps { get; set; }
        public List<JobModel> RetrieverJob { get; set; }
        public List<CompressionModel> CompressionJob { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDTM { get; set; }
        public int DataFlowId { get; set; }



        public IEnumerable<SelectListItem> CompressionDropdown { get; set; }
        public IEnumerable<SelectListItem> PreProcessingRequiredDropdown { get; set; }
        public IEnumerable<SelectListItem> PreProcessingOptionsDropdown { get; set; }
        [DisplayName("Pre Processing Options")]
        public List<int> PreprocessingOptions { get; set; }

        public List<string> Validate()
        {
            List<string> errors = new List<string>();

            if (SchemaMaps == null || SchemaMaps.Count == 0)
            {
                errors.Add("Must contain atleast one schema mapping");
            }
            else if (SchemaMaps.Count > 0)
            {
                bool dsSelectionErr = false;
                bool scmSelectionErr = false;
                foreach (SchemaMapModel model in SchemaMaps)
                {
                    if (model.SelectedDataset == 0)
                    {
                        dsSelectionErr = true;
                    }
                    if (model.SelectedSchema == 0)
                    {
                        scmSelectionErr = true;
                    }
                }

                if (dsSelectionErr)
                {
                    errors.Add("Must select dataset for schema mapping");
                }
                if (scmSelectionErr)
                {
                    errors.Add("Must select schema for schema mapping");
                }
            }

            if (Name.StartsWith("FileSchemaFlow"))
            {
                errors.Add("FileSchemaFlow is a reserved name, please choose new name");
            }

            if (IsPreProcessingRequired && PreprocessingOptions.Count == 1 && PreprocessingOptions.First() == 0)
            {
                errors.Add("Pre Processing selection is required");
            }

            return errors;
        }
    }
}