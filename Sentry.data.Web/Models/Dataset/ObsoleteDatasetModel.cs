using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Sentry.Associates;

namespace Sentry.data.Web
{
    [Obsolete("This is the Old datasetModel that is still being used in some areas but should be moved into seperate models based on the area of use.")]
    public class ObsoleteDatasetModel
    {
        public ObsoleteDatasetModel()
        {

        }

        public ObsoleteDatasetModel(Dataset ds, IAssociateInfoProvider associateInfoService, IDatasetContext datasetContext = null)
        {
            this.DatasetId = ds.DatasetId;
            this.Category = ds.DatasetCategories.First().Name;
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
            this.IsSecured = ds.IsSecured;
            this.CanDisplay = ds.CanDisplay;
            this.DatasetInformation = ds.DatasetInformation;

            this.DatasetCategory = ds.DatasetCategories.First();
            

            this.DatasetScopeType = ds.DatasetScopeType();


            this.DatasetFileConfigs = new List<DatasetFileConfigsModel>();
            List<string> locations = new List<string>();
            foreach (DatasetFileConfig dfc in ds.DatasetFileConfigs.Where(w => w.DeleteInd == false))
            {
                if (datasetContext != null)
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


            
            if (!String.IsNullOrWhiteSpace(ds.DatasetType) && ds.DatasetType == GlobalConstants.DataEntityCodes.REPORT)
            {
                List<MetadataTag> tagList = new List<MetadataTag>();
                foreach (MetadataTag tag in ds.Tags)
                {
                    tagList.Add(tag);
                }
                Tags = tagList;

                UploadFrequency = Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) ?? "Not Specified";
            }
            else
            {
                Tags = new List<MetadataTag>();
                UploadFrequency = null;
            }

            if (ds.DataClassification > 0)
            {
                this.DataClassification = ds.DataClassification.GetDescription();
            }

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
        public Boolean IsSecured { get; set; }

        [DisplayName("Data Classification")]
        public string DataClassification { get; set; }

        public Boolean CanDisplay { get; set; }


        public Boolean CanEditDataset { get; set; }
        public Boolean CanUpload { get; set; }
        public Boolean IsPreviewCompatible { get; set; }
        public Boolean CanQueryTool { get; set; }
        public Boolean CanManageReport { get; set; }

        public Boolean IsSubscribed { get; set; }
        public Category DatasetCategory { get; set; }

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
        public List<MetadataTag> Tags { get; set; }
        public string UploadFrequency { get; set; }
        public Boolean IsFavorite { get; set; }
        public UserSecurity Security { get; set; }
    }
}
