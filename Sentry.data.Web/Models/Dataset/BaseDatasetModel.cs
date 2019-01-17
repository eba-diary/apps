using System;
using System.Collections.Generic;
using System.Linq;
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

        public BaseDatasetModel(BaseEntityDto dto, IAssociateInfoProvider associateInfoService)
        {
            this.SentryOwner = associateInfoService.GetAssociateInfo(dto.SentryOwnerName);
            this.SentryOwnerName = this.SentryOwner.FullName;
            this.DatasetId = dto.DatasetId;
            //this.Category = ds.Category;
            this.DatasetName = dto.DatasetName;
            this.DatasetDesc = dto.DatasetDesc;
            this.CreationUserName = dto.CreationUserName;

            int n;
            if (!string.IsNullOrEmpty(dto.UploadUserName) && int.TryParse(dto.UploadUserName, out n))
            {
                this.UploadUserName = associateInfoService.GetAssociateInfo(dto.UploadUserName).FullName;
            }
            else
            {
                this.UploadUserName = dto.UploadUserName;
            }

            this.OriginationCode = dto.OriginationCode;
            this.FileExtension = null;
            this.DatasetDtm = dto.DatasetDtm;
            this.ChangedDtm = dto.ChangedDtm;
            this.IsSensitive = dto.IsSensitive;
            this.CanDisplay = dto.CanDisplay;
            this.DatasetInformation = dto.DatasetInformation; 

            this.IsPushToTableauCompatible = false;
            this.DatasetCategoryIds = dto.DatasetCategories.Select(x=> x.Id).ToList(); 
            this.DatasetFiles = new List<BaseDatasetFileModel>();

            foreach (DatasetFile df in dto.DatasetFiles.OrderByDescending(x => x.CreateDTM))
            {
                this.DatasetFiles.Add(new BaseDatasetFileModel(df));
            }

            this.DatasetScopeType = dto.DatasetScopeType;

            
            this.DatasetFileConfigs = new List<DatasetFileConfigsModel>();
            List<string> locations = new List<string>();
            foreach (DatasetFileConfig dfc in dto.DatasetFileConfigs)
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
            
            if (this.DistinctFileExtensions().Where(w => Utilities.IsExtentionPreviewCompatible(w)).Count() > 0)
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


        public int DatasetId { get; set; }

        public IEnumerable<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllFrequencies { get; set; }

        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }

        public IEnumerable<SelectListItem> AllDataFileTypes { get; set; }

        public IEnumerable<SelectListItem> AllDataClassifications { get; set; }

        //[Required()]
        //[MaxLength(64)]
        //[DisplayName("Category")]
        //public string Category { get; set; }

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
        public Boolean CanManageReport { get; set; }

        public Boolean IsSubscribed { get; set; }

        [Required]
        [DisplayName("Category")]
        public List<int> DatasetCategoryIds { get; set; } 


        public IList<BaseDatasetFileModel> DatasetFiles { get; set; }

        [DisplayName("Dataset Scope")]
        public List<DatasetScopeType> DatasetScopeType { get; set; }

        public Associate SentryOwner { get; set; }
        public string AssociateCommonName
        {
            get
            {
                return Sentry.data.Core.Helpers.DisplayFormatter.FormatAssociateName(this.SentryOwner);
            }
        }

        public IList<DatasetFileConfigsModel> DatasetFileConfigs { get; set; }

        [DisplayName("Drop Location")]
        public List<string> DropLocations { get; set; }

        public int AmountOfSubscriptions { get; set; }

        public int Views { get; set; }
        public int Downloads { get; set; }
        public string ObjectType { get; set; }
        public Boolean IsFavorite { get; set; }
    }
}
