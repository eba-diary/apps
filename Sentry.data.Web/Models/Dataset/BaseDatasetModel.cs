using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Sentry.data.Infrastructure;
using Sentry.Associates;
using Sentry.data.Common;

namespace Sentry.data.Web
{
    public class BaseDatasetModel
    {
        public BaseDatasetModel()
        {

        }

        public BaseDatasetModel(Dataset ds, IAssociateInfoProvider associateInfoService, IDatasetContext datasetContext = null)
        {
            this.SentryOwner = associateInfoService.GetAssociateInfo(ds.SentryOwnerName);
            this.SentryOwnerName = this.SentryOwner.FullName;
            this.DatasetId = ds.DatasetId;
            this.Category = ds.Category;
            this.DatasetName = ds.DatasetName;
            this.DatasetDesc = ds.DatasetDesc;
            this.CreationUserName = ds.CreationUserName;

            int n;
            if (!string.IsNullOrEmpty(ds.UploadUserName) && int.TryParse(ds.UploadUserName, out n))
            {
                this.UploadUserName = associateInfoService.GetAssociateInfo(ds.UploadUserName).FullName;
            }
            else
            {
                this.UploadUserName = ds.UploadUserName;
            }

            this.OriginationCode = ds.OriginationCode;
            this.FileExtension = null;
            this.DatasetDtm = ds.DatasetDtm;
            this.ChangedDtm = ds.ChangedDtm;
            this.S3Key = ds.S3Key;
            this.IsSensitive = ds.IsSensitive;
            this.CanDisplay = ds.CanDisplay;
            this.DatasetInformation = ds.DatasetInformation; 

            this.IsPushToTableauCompatible = false;
            this.DatasetCategory = ds.DatasetCategory; 
            this.DatasetFiles = new List<BaseDatasetFileModel>();

            foreach (DatasetFile df in ds.DatasetFiles.OrderByDescending(x => x.CreateDTM))
            {
                this.DatasetFiles.Add(new BaseDatasetFileModel(df));
            }

            this.DatasetScopeType = ds.DatasetScopeType;

            
            this.DatasetFileConfigs = new List<DatasetFileConfigsModel>();
            List<string> locations = new List<string>();
            foreach (DatasetFileConfig dfc in ds.DatasetFileConfigs)
            {
                if(datasetContext != null)
                {
                    this.DatasetFileConfigs.Add(new DatasetFileConfigsModel(dfc, true, false, datasetContext));
                }
                else
                {
                    this.DatasetFileConfigs.Add(new DatasetFileConfigsModel(dfc, true, false));
                }

                foreach (RetrieverJob rj in dfc.RetrieverJobs.Where(x => x.DataSource.Is<DfsBasic>()))
                {
                    locations.Add(rj.GetUri().LocalPath);
                }
            }

            this.DropLocations = locations;


            if (this.DistinctFileExtensions().Where(w => Utilities.IsExtentionPushToSAScompatible(w)).Count() > 0)
            { this.IsPushToSASCompatible = true; }
            else
            { this.IsPushToSASCompatible = false; }

            //if (ds.FileExtension == ".csv")
            //{ this.IsPushToSASCompatible = true; }
            //else
            //{ this.IsPushToSASCompatible = false; }
            if (this.DistinctFileExtensions().Where(w => Utilities.IsExtentionPreviewCompatible(w)).Count() > 0)
            //if (ds.FileExtension == ".csv" || ds.FileExtension == ".txt" || ds.FileExtension == ".json")
            { this.IsPreviewCompatible = true; }
            else
            { this.IsPreviewCompatible = false; }
        }

        public List<string> DistinctFileExtensions()
        {
            List<string> extensions = new List<string>();
            foreach (BaseDatasetFileModel item in this.DatasetFiles)
            {
                extensions.Add(Utilities.GetFileExtension(item.FileName));
            }
            return extensions.Distinct().ToList();
        }

        [DisplayName("Creation Frequency")]
        public List<string> DistinctFrequencies()
        {
            List<string> frequencies = new List<string>();

            foreach(var item in this.DatasetFileConfigs)
            {
                if (item.RetrieverJobs != null)
                {
                    if(item.RetrieverJobs.Count == 1)
                    {
                        frequencies.Add(item.RetrieverJobs.First().ReadableSchedule);
                    }
                    else
                    {
                        foreach (var job in item.RetrieverJobs.Where(x => !x.IsGeneric))
                        {
                            frequencies.Add(job.ReadableSchedule);
                        }
                    }
                }
            }

            return frequencies.Distinct().ToList();
        }


        public int DatasetId { get; set; }

        public IEnumerable<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllFrequencies { get; set; }

        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }

        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }

        public IEnumerable<SelectListItem> AllDataClassifications { get; set; }

        //[Required()]
        [MaxLength(64)]
        [DisplayName("Category")]
        public string Category { get; set; }

        [Required()]
        [MaxLength(1024)]
        [DisplayName("Dataset Name")]
        public string DatasetName { get; set; }

        [Required()]
        [MaxLength(4096)]
        [DisplayName("Description")]
        public string DatasetDesc { get; set; }

        [MaxLength(4096)]
        [DisplayName("Usage Information")]
        public string DatasetInformation { get; set; }


        [Required]
        [MaxLength(128)]
        [DisplayName("Originating Creator")]
        public string CreationUserName { get; set; }

        [Required]
        [DisplayName("Sentry Owner")]
        public string SentryOwnerName { get; set; }

        //[Required]
        [MaxLength(128)]
        [DisplayName("Creator")]
        public string UploadUserName { get; set; }

        //[Required]
        [MaxLength(16)]
        [DisplayName("Origination Code")]
        public string OriginationCode { get; set; }

        [MaxLength(16)]
        [DisplayName("File Extension")]
        public string FileExtension { get; set; }
        public int FileExtensionID { get; set; }

        [Required]
        [DisplayName("Creation Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime DatasetDtm { get; set; }

        [DisplayName("Last Modified")]
        public DateTime ChangedDtm { get; set; }

        //[Required]
        [MaxLength(1024)]
        [DisplayName("S3 Location")]
        public string S3Key { get; set; }

        [DisplayName("Sensitive")]
        public Boolean IsSensitive { get; set; }

        public Boolean CanDisplay { get; set; }


        public Boolean CanDwnldSenstive { get; set; }
        public Boolean CanEditDataset { get; set; }
        public Boolean CanManageConfigs { get; set; }
        public Boolean CanUpload { get; set; }
        public Boolean CanDwnldNonSensitive { get; set; }
        public Boolean IsPushToSASCompatible { get; set; }
        public Boolean IsPushToTableauCompatible { get; set; }
        public Boolean IsPreviewCompatible { get; set; }
        public Boolean CanQueryTool { get; set; }

        public Boolean IsSubscribed { get; set; }
        public Category DatasetCategory { get; set; } 


        public IList<BaseDatasetFileModel> DatasetFiles { get; set; }

        [DisplayName("Dataset Scope")]
        public List<DatasetScopeType> DatasetScopeType { get; set; }

        public Associate SentryOwner { get; set; }

        public IList<DatasetFileConfigsModel> DatasetFileConfigs { get; set; }

        [DisplayName("Drop Location")]
        public List<string> DropLocations { get; set; }

        public int AmountOfSubscriptions { get; set; }

    }
}
