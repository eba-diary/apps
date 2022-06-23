using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DatasetFileConfigsModel
    {
        public DatasetFileConfigsModel() {
        }

        public DatasetFileConfigsModel(DatasetFileConfig dsfc, Boolean renderingForTable, Boolean renderingForPopup)
        {
            this.ToModel(dsfc);

            if (renderingForPopup)
            {
                SearchCriteria = new List<string>();
                IsRegexSearch = new List<bool>();
                foreach (var job in dsfc.RetrieverJobs)
                {
                    SearchCriteria.Add(job.JobOptions.SearchCriteria);
                    IsRegexSearch.Add(job.JobOptions.IsRegexSearch);
                }
            }
        }
        public DatasetFileConfigsModel(DatasetFileConfigDto dto)
        {
            this.ConfigId = dto.ConfigId;
            this.FileTypeId = dto.FileTypeId;
            this.ConfigFileName = dto.Name;
            this.ConfigFileDesc = dto.Description;
            this.DatasetId = dto.ParentDatasetId;
            this.DatasetScopeTypeID = dto.DatasetScopeTypeId;
            this.FileExtensionID = dto.FileExtensionId;
            this.RawStorageId = dto.StorageCode;
            this.Security = dto.Security;
            this.CreateCurrentView = dto.CreateCurrentView;
            this.Delimiter = dto.Delimiter;
            this.HasHeader = dto.HasHeader;
            this.SchemaId = dto.Schema.SchemaId;
            this.OldSchemaId = (dto.Schemas != null) ? dto.Schemas.FirstOrDefault().DataElementID : 0;
            this.CLA1396_NewEtlColumns = (dto.Schema != null) ? dto.Schema.CLA1396_NewEtlColumns : false;
            this.CLA1580_StructureHive = (dto.Schema != null) ? dto.Schema.CLA1580_StructureHive : false;
            this.CLA2472_EMRSend = (dto.Schema != null) ? dto.Schema.CLA2472_EMRSend : false;
            this.CLA1286_KafkaFlag = (dto.Schema != null) ? dto.Schema.CLA1286_KafkaFlag : false;
            this.CLA3014_LoadDataToSnowflake = (dto.Schema != null) ? dto.Schema.CLA3014_LoadDataToSnowflake : false;
            this.ObjectStatus = dto.ObjectStatus;
            this.SchemaRootPath = dto.SchemaRootPath;
            this.ParquetStorageBucket = dto.ParquetStorageBucket;
            this.ParquetStoragePrefix = dto.ParquetStoragePrefix;
            //this.CLA24 = (dto.Schema != null) ? dto.Schema.CLA1580_StructureHive : false;
        }

        public DatasetFileConfigsModel(DatasetFileConfig dsfc, Boolean renderingForTable, Boolean renderingForPopup, IDatasetContext datasetContext)
        {
            this.ToModel(dsfc);

            if (renderingForPopup)
            {
                SearchCriteria = new List<string>();
                IsRegexSearch = new List<bool>();
                foreach (var job in dsfc.RetrieverJobs)
                {
                    SearchCriteria.Add(job.JobOptions.SearchCriteria);
                    IsRegexSearch.Add(job.JobOptions.IsRegexSearch);
                }
            }
        }

        //This is a list in order of retriever jobs info for the UI.
        public List<String> SearchCriteria { get; set; }
        public List<Boolean> IsRegexSearch { get; set; }

        public int ConfigId { get; set; }

        public int FileTypeId { get; set; }

        public int DataElement_ID { get; set; }

        [DisplayName("File Type")]
        public string FileType {
            get
            {
                return ((FileType)FileTypeId).ToString();
            }
            set
            {
                FileTypeId = (int)Enum.Parse(typeof(FileType), value); ;
            }
        }

        [Required]
        [DisplayName("Configuration Name")]
        public string ConfigFileName { get; set; }

        [DisplayName("Description")]
        public string ConfigFileDesc { get; set; }

        [DisplayName("Delimiter")]
        public string Delimiter { get; set; }

        [DisplayName("Includes Header Row")]
        public bool HasHeader { get; set; }

        public string EditHref
        {
            get
            {
                string href = null;
                href = $"<a href=\"Config\\Dataset\\{DatasetId}\\Edit\\{ConfigId}\" class=\"table-row-icon\" title=\"Edit Config File\"><em class='far fa-edit text-primary'></em></a>";
                return href;
            }
        }

        [DisplayName("Parent Dataset")]
        public string ParentDatasetName { get; set; }
        [DisplayName("Data Scope Type")]
        public int DatasetScopeTypeID { get; set; }
        [DisplayName("File Extension")]
        public int FileExtensionID { get; set; }
        [DisplayName("Create Current View")]
        public bool CreateCurrentView { get; set; }
        public int DatasetId { get; set; }
        public DatasetScopeType ScopeType { get; set; }
        public FileExtension FileExtension { get; set; }
        [DisplayName("Schema Root Path")]
        public string SchemaRootPath { get; set; }
        public string RawStorageId { get; set; }
        public int SchemaId { get; set; }
        public int OldSchemaId { get; set; }
        public bool CLA1396_NewEtlColumns { get; set; }
        public bool CLA1580_StructureHive { get; set; }
        public bool CLA2472_EMRSend { get; set; }
        public bool CLA1286_KafkaFlag { get; set; }
        public bool CLA3014_LoadDataToSnowflake { get; set; }
        public AssociatedDataFlowModel DataFlowJobs { get; set; }
        public IList<AssociatedDataFlowModel> ExternalDataFlowJobs { get; set; }

        public FileSchema Schema { get; set; }

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }
        public IEnumerable<SelectListItem> ExtensionList { get; set; }

        public UserSecurity Security { get; set; }
        public string SasLibrary { get; set; }
        public bool DeleteInd { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }
        public string ParquetStorageBucket { get; set; }
        public string ParquetStoragePrefix { get; set; }
        public string DatasetScopeReadonly { get; set; }
    }
}