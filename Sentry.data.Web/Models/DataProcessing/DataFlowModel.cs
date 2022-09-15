using Sentry.Core;
using Sentry.data.Core;
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
            IsBackFilled = false;
            IsPreProcessingRequired = false;
            RetrieverJob = new JobModel();
            ObjectStatus = ObjectStatusEnum.Active;
            SelectedDataset = 0;
            SelectedSchema = 0;
        }

        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; }

        /// <summary>
        /// Is the incoming data compressed?
        /// </summary>
        /// 
        [DisplayName("Is incoming data compressed?")]
        public bool IsCompressed { get; set; }
        [DisplayName("Do you need to backfill data from DFS Drop Location?")]
        public bool IsBackFilled { get; set; }                                    //IsCompressed
        public bool IsPreProcessingRequired { get; set; }
        [DisplayName("Pre Processing Options")]
        public int PreProcessingSelection { get; set; }
        /// <summary>
        /// Target
        /// </summary>
        public int SchemaId { get; set; }

        [DisplayName("SAID Asset")]
        [System.ComponentModel.DataAnnotations.Required]
        public string SAIDAssetKeyCode { get; set; }

        [DisplayName("Where should this data be loaded?")]
        public List<SchemaMapModel> SchemaMaps { get; set; }
        public JobModel RetrieverJob { get; set; }
        public List<CompressionModel> CompressionJob { get; set; }
        public string CreatedBy { get; set; }
        public string PrimaryContactId { get; set; }
        public bool IsSecured { get; set; }
        public DateTime CreatedDTM { get; set; }
        public int DataFlowId { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
        public string StorageCode { get; set; }

        /// <summary>
        /// How is data getting into DSC (Push or Pull)
        /// </summary>
        /// 
        [DisplayName("How will data be ingested into DSC?")]
        public int IngestionTypeSelection { get; set; }

        [DisplayName("What is the Topic Name?")]
        public string TopicName { get; set; }
        public string S3ConnectorName { get; set; }

        /// <summary>
        /// Named Environment naming conventions from https://confluence.sentry.com/x/eQNvAQ
        /// </summary>
        [DisplayName("Named Environment")]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression("^[A-Z0-9]{1,10}$", ErrorMessage = "Named environment must be alphanumeric, all caps, and less than 10 characters")]
        public string NamedEnvironment { get; set; }


        [DisplayName("Named Environment Type")]
        [System.ComponentModel.DataAnnotations.Required]
        public NamedEnvironmentType NamedEnvironmentType { get; set; }
        //[Range(1, int.MaxValue, ErrorMessage = "Please Select a Dataset")]
        public int SelectedDataset { get; set; }
        //[Range(1, int.MaxValue, ErrorMessage = "Please Select a Schema")]
        public int SelectedSchema { get; set; }

        public IEnumerable<SelectListItem> CompressionDropdown { get; set; }
        public IEnumerable<SelectListItem> BackfillDropdown { get; set; }       //CompressionDropdown
        public IEnumerable<SelectListItem> PreProcessingRequiredDropdown { get; set; }
        public IEnumerable<SelectListItem> PreProcessingOptionsDropdown { get; set; }
        public IEnumerable<SelectListItem> SAIDAssetDropDown { get; set; }
        public IEnumerable<SelectListItem> IngestionTypeDropDown { get; set; }
        public IEnumerable<SelectListItem> NamedEnvironmentDropDown { get; set; }
        public IEnumerable<SelectListItem> NamedEnvironmentTypeDropDown { get; set; }
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
            if (IngestionTypeSelection == (int)IngestionType.DSC_Pull && RetrieverJob == null)
            {
                results.Add(string.Empty, "Pull type data flows required retriever job configuration");
            }
            if(IngestionTypeSelection == (int)IngestionType.DSC_Pull && RetrieverJob != null)
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
            else
            {
                foreach(SchemaMapModel model in SchemaMaps)
                {
                    foreach(ValidationResult result in model.Validate().ValidationResults.GetAll())
                    {
                        results.Add($"SchemaMaps[{model.Index}].{result.Id}", result.Description, result.Severity);
                    }
                }
            }

            if (IsPreProcessingRequired && PreProcessingSelection == 0)
            {
                results.Add("PreprocessingOptions", "Pre Processing selection is required");
            }

            if (String.IsNullOrWhiteSpace(SAIDAssetKeyCode))
            {
                results.Add(GlobalConstants.ValidationErrors.SAID_ASSET_REQUIRED, "Must associate data flow with SAID asset");
            }

            //IF IngestionType==TOPIC THEN ENSURE NOT EMPTY
            if (IngestionTypeSelection == (int) IngestionType.Topic && string.IsNullOrWhiteSpace(TopicName))
            {
                results.Add(DataFlow.ValidationErrors.topicNameIsBlank, "Must specify a topic name");
            }

            ValidationException ex = new ValidationException(results);            
            return ex;
        }
    }
}