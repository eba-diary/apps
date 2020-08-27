using Sentry.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            RetrieverJob = new JobModel();
        }

        [System.ComponentModel.DataAnnotations.Required]
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
        public JobModel RetrieverJob { get; set; }
        public List<CompressionModel> CompressionJob { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDTM { get; set; }
        public int DataFlowId { get; set; }



        public IEnumerable<SelectListItem> CompressionDropdown { get; set; }
        public IEnumerable<SelectListItem> PreProcessingRequiredDropdown { get; set; }
        public IEnumerable<SelectListItem> PreProcessingOptionsDropdown { get; set; }
        [DisplayName("Pre Processing Options")]
        public List<int> PreprocessingOptions { get; set; }

        public ValidationException Validate()
        {
            ValidationResults results = new ValidationResults();
            if (string.IsNullOrWhiteSpace(Name))
            {
                results.Add(DataFlow.ValidationErrors.nameIsBlank, "Must specify data flow name");
            }

            if (Name != null && Name.StartsWith("FileSchemaFlow"))
            {
                results.Add(DataFlow.ValidationErrors.nameContainsReservedWords, "FileSchemaFlow is a reserved name, please choose new name");
            }

            #region RetrieverJob validations
            if (IngestionType == IngestionType.DSC_Pull && (RetrieverJob == null))
            {
                results.Add(string.Empty, "Pull type data flows required retriever job configuration");
            }
            if(IngestionType == IngestionType.DSC_Pull && RetrieverJob != null)
            {
                foreach (ValidationResult result in RetrieverJob.Validate().ValidationResults.GetAll())
                {
                    results.Add(result.Id, result.Description, result.Severity);
                }
            }
            
            #endregion


            if (SchemaMaps == null || !SchemaMaps.Any(w => !w.IsDeleted))
            {
                results.Add(DataFlow.ValidationErrors.stepsContainsAtLeastOneSchemaMap, "Must contain at least one schema mapping");
            }
            else if (SchemaMaps.Any(w => !w.IsDeleted))
            {
                bool dsSelectionErr = false;
                bool scmSelectionErr = false;
                foreach (SchemaMapModel model in SchemaMaps.Where(w => !w.IsDeleted))
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
                    results.Add(SchemaMap.ValidationErrors.schemamapMustContainDataset, "Must select dataset for schema mapping");
                }
                if (scmSelectionErr)
                {
                    results.Add(SchemaMap.ValidationErrors.schemamapMustContainSchema, "Must select schema for schema mapping");
                }
            }

            if (IsPreProcessingRequired && PreprocessingOptions.Count == 1 && PreprocessingOptions.First() == 0)
            {
                results.Add("PreprocessingOptions", "Pre Processing selection is required");
            }
            ValidationException ex = new ValidationException(results);            
            return ex;
        }
    }
}