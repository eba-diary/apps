using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Sentry.data.Web
{
    public class BaseDatasetModel
    {
        public BaseDatasetModel()
        {

        }

        public BaseDatasetModel(Dataset ds)
        {
            this.DatasetId = ds.DatasetId;
            this.Category = ds.Category;
            this.DatasetName = ds.DatasetName;
            this.DatasetDesc = ds.DatasetDesc;
            this.CreationUserName = ds.CreationUserName;
            this.SentryOwnerName = ds.SentryOwnerName;
            this.UploadUserName = ds.UploadUserName;
            this.OriginationCode = ds.OriginationCode;
            this.FileExtension = ds.FileExtension;
            this.DatasetDtm = ds.DatasetDtm;
            this.ChangedDtm = ds.ChangedDtm;
            this.UploadDtm = ds.UploadDtm;
            this.CreationFreqDesc = ds.CreationFreqDesc;
            this.FileSize = ds.FileSize;
            this.RecordCount = ds.RecordCount;
            this.S3Key = ds.S3Key;
            this.IsSensitive = ds.IsSensitive;
            this.CanDisplay = ds.CanDisplay;
            this.RawMetadata = new List<_DatasetMetadataModel>();
            foreach (DatasetMetadata dsm in ds.RawMetadata)
            {
                this.RawMetadata.Add(new _DatasetMetadataModel(dsm));
            }
            //this.Columns = new List<_DatasetMetadataModel>();
            //foreach (DatasetMetadata dsm in ds.Columns)
            //{
            //    this.Columns.Add(new _DatasetMetadataModel(dsm));
            //}
            //this.Metadata = new List<_DatasetMetadataModel>();
            //foreach (DatasetMetadata dsm in ds.Metadata)
            //{
            //    this.Metadata.Add(new _DatasetMetadataModel(dsm));
            //}
            this.SearchHitList = new List<string>();

            if (ds.FileExtension == ".csv")
                { this.IsPushToSASCompatible = true; }
            else
                { this.IsPushToSASCompatible = false; }

            if (ds.FileExtension == ".csv" || ds.FileExtension == ".txt")
            { this.IsPreviewCompatible = true; }
            else
            { this.IsPreviewCompatible = false; }

            this.IsPushToTableauCompatible = false;
            DatasetCategory = ds.DatasetCategory; /*Caden a change here for the Category reference*/
        }

        public string FileExtensionDisplay()
        {
            string fe = FileExtension.TrimStart('.');
            return fe;
        }

        public int DatasetId { get; set; }

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

        [Required]
        [MaxLength(128)]
        [DisplayName("Originating Creator")]
        public string CreationUserName { get; set; }

        [Required]
        [RegularExpression("(^[0-9]{6,6}$)", ErrorMessage ="Please enter a Sentry ID")]
        [DisplayName("Sentry Owner")]
        public string SentryOwnerName { get; set; }

        //[Required]
        [MaxLength(128)]
        [DisplayName("Upload User")]
        public string UploadUserName { get; set; }

        //[Required]
        [MaxLength(16)]
        [DisplayName("Origination Code")]
        public string OriginationCode { get; set; }

        [MaxLength(16)]
        [DisplayName("File Extension")]
        public string FileExtension { get; set; }

        [Required]
        [DisplayName("Original Creation Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime DatasetDtm { get; set; }

        [DisplayName("File Change Date")]
        public DateTime ChangedDtm { get; set; }

        [Required]
        [DisplayName("Upload Date")]
        public DateTime UploadDtm { get; set; }

        [DisplayName("Creation Frequency")]
        public string CreationFreqDesc { get; set; }

        [Required]
        [DisplayName("File Size")]
        public long FileSize { get; set; }

        [DisplayName("Record Count")]
        public long RecordCount { get; set; }

        //[Required]
        [MaxLength(1024)]
        [DisplayName("S3 Key")]
        public string S3Key { get; set; }

        [DisplayName("Sensitive")]
        public Boolean IsSensitive { get; set; }

        public Boolean CanDisplay { get; set; }

        public IList<_DatasetMetadataModel> RawMetadata { get; set; }

        public IList<_DatasetMetadataModel> Columns
        {
            get
            {
                if (null != this.RawMetadata)
                    return RawMetadata.Where(x => x.IsColumn == true).ToList();
                else
                    return null;
            }
        }

        public IList<_DatasetMetadataModel> Metadata
        {
            get
            {
                if (null != this.RawMetadata)
                    return RawMetadata.Where(x => x.IsColumn == false).ToList();
                else
                    return null;
            }
        }

        public List<String> SearchHitList;

        public Boolean CanDwnldSenstive { get; set; }
        public Boolean CanEditDataset { get; set; }

        public Boolean IsPushToSASCompatible { get; set; }

        public Boolean IsPushToTableauCompatible { get; set; }

        public Boolean IsPreviewCompatible { get; set; }

        public Category DatasetCategory { get; set; } /* Caden made a change here for the Category reference */ 
        //public IList<String> CategoryList { get; set; }

        ///// <summary>
        ///// AllCategories holds the sorted list of all possible categories.
        ///// </summary>
        //public IEnumerable<SelectListItem> AllCategories { get; set; }

    }
}
