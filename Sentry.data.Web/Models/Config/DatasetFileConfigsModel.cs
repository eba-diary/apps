using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;
using Sentry.Core;

namespace Sentry.data.Web
{
    public class DatasetFileConfigsModel
    {
        public DatasetFileConfigsModel() {
        }

        public DatasetFileConfigsModel(DatasetFileConfig dsfc, Boolean renderingForTable, Boolean renderingForPopup)
        {
            this.ConfigId = dsfc.ConfigId;
            this.FileTypeId = dsfc.FileTypeId;
            this.ConfigFileName = dsfc.Name;
            this.ConfigFileDesc = dsfc.Description;
            this.ParentDatasetName = dsfc.ParentDataset.DatasetName;
            this.DatasetScopeTypeID = dsfc.DatasetScopeType.ScopeTypeId;
            this.ScopeType = dsfc.DatasetScopeType;
            this.FileExtensionID = dsfc.FileExtension.Id;
            this.FileExtension = dsfc.FileExtension;
            this.Schemas = dsfc.Schema;
            this.RawStorageId = dsfc.GetStorageCode();

            try
            {


                if (renderingForTable)
                {
                    this.RetrieverJobs = dsfc.RetrieverJobs.ToList();
                }

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
            catch (Exception ex)
            {
                throw;
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
            this.IncludedInSAS = dto.IsInSAS;
        }

        public DatasetFileConfigsModel(DatasetFileConfig dsfc, Boolean renderingForTable, Boolean renderingForPopup, IDatasetContext datasetContext)
        {
            this.ConfigId = dsfc.ConfigId;
            this.FileTypeId = dsfc.FileTypeId;
            this.ConfigFileName = dsfc.Name;
            this.ConfigFileDesc = dsfc.Description;
            this.ParentDatasetName = dsfc.ParentDataset.DatasetName;
            this.DatasetScopeTypeID = dsfc.DatasetScopeType.ScopeTypeId;
            this.ScopeType = dsfc.DatasetScopeType;
            this.FileExtensionID = dsfc.FileExtension.Id;
            this.FileExtension = dsfc.FileExtension;
            this.Schemas = dsfc.Schema;
            this.RawStorageId = dsfc.GetStorageCode();

            try
            {

                if (renderingForTable)
                {
                    this.RetrieverJobs = dsfc.RetrieverJobs.ToList();
                }

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
            catch (Exception ex)
            {
                throw;
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
                href = $"<a href=\"Config\\Dataset\\{DatasetId}\\Edit\\{ConfigId}\" class=\"table-row-icon\" title=\"Edit Config File\"><i class='glyphicon glyphicon-edit text-primary'></i></a>";
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
        [DisplayName("Add to SAS")]
        public bool IncludedInSAS { get; set; }


        public int DatasetId { get; set; }
        public DatasetScopeType ScopeType { get; set; }
        public FileExtension FileExtension { get; set; }
        public string RawStorageId { get; set; }

        public IList<RetrieverJob> RetrieverJobs { get; set; }

        public IList<DataElement> Schemas { get; set; }     

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }
        public IEnumerable<SelectListItem> ExtensionList { get; set; }

        public UserSecurity Security { get; set; }
        public string SasLibrary { get; set; }
    }
}