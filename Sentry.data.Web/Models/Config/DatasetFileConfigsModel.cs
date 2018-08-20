﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;
using Sentry.data.Core.Entities.Metadata;

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
            this.DataElement_ID = dsfc.DataElement_ID;

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
            catch(Exception ex)
            {
                throw;
            }
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
            this.DataElement_ID = dsfc.DataElement_ID;

            try
            {
                if (datasetContext.Schemas.Any(x => x.DatasetFileConfig.ConfigId == this.ConfigId))
                {
                    this.Schemas = datasetContext.Schemas.Where(x => x.DatasetFileConfig.ConfigId == this.ConfigId).ToList();
                }

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
                return ((FileType) FileTypeId).ToString();
            }
            set
            {
                FileTypeId = (int) Enum.Parse(typeof(FileType), value); ;
            }
        }

        [Required]
        [DisplayName("Configuration Name")]
        public string ConfigFileName { get; set; }

        [DisplayName("Description")]
        public string ConfigFileDesc { get; set; }

        public int DatasetId { get; set; }
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

        public DatasetScopeType ScopeType { get; set; }
        [DisplayName("Data Scope Type")]
        public int DatasetScopeTypeID { get; set; }

        public FileExtension FileExtension { get; set; }
        public int FileExtensionID { get; set; }

        public IList<RetrieverJob> RetrieverJobs { get; set; }

        public IList<Schema> Schemas { get; set; }     

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }
        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }
        public IEnumerable<SelectListItem> ExtensionList { get; set; }
    }
}