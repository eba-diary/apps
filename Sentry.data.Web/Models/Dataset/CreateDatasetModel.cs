using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class CreateDatasetModel : BaseDatasetModel
    {
        public CreateDatasetModel()
        {
            this.Category = "";
            this.ChangedDtm = DateTime.MinValue;
            this.DatasetDesc = "";
            this.DatasetName = "";
            this.FileExtension = null;
            this.ConfigFileName = "Default";
            this.ConfigFileDesc = "Default Config for Dataset.  Uploaded files that do not match any configs will default to this config";
            this.DatasetId = 0;
            this.OriginationCode = "";
            this.S3Key = "";
            this.SentryOwnerName = "";
            this.UploadUserName = "";
            this.IsSensitive = false;
            this.CanDisplay = true;
        }

        //[DisplayName("File Upload")]
        //public HttpPostedFile f { get; set; }

        //public long ProgressConnectionId { get; set; }

        [Required]
        [DisplayName("Categories")]
        public int CategoryIDs { get; set; }

        [Required]
        [DisplayName("Sentry Owner")]
        public string OwnerID { get; set; }

        [Required]
        [DisplayName("Data Classification")]
        public int DataClassification { get; set; }

        [Required]
        [DisplayName("Upload Frequency")]
        public int FreqencyID { get; set; }

        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [Required]
        [DisplayName("Dataset Scope")]
        public int DatasetScopeTypeID { get; set; }

        [DisplayName("Search Criteria")]
        public string SearchCriteria { get; set; }

        [DisplayName("Target File Name")]
        public string TargetFileName { get; set; }

        [DisplayName("Custom Drop Path")]
        public Boolean CustomDropPath { get; set; }

        [DisplayName("Use Regex Search")]
        public Boolean IsRegexSearch { get; set; }

        [DisplayName("Overwrite Dataset Files")]
        public Boolean OverwriteDatasetFile { get; set; }

        public int FileTypeId { get; set; }

        [DisplayName("File Type")]
        public string FileType
        {
            get
            {
                return ((FileType)FileTypeId).ToString();
            }
            set
            {
                FileTypeId = (int)Enum.Parse(typeof(FileType), value); ;
            }
        }

        public List<DatasetFileConfigsModel> Configs { get; set; }




        [DisplayName("Configuration Name")]
        public string ConfigFileName { get; set; }

        [DisplayName("Description")]
        public string ConfigFileDesc { get; set; }

        [DisplayName("Data Source")]
        public DataSource DataSource { get; set; }
        
        public string TargetFileType { get; set; }

        public IEnumerable<SelectListItem> SourceTypes { get; set; }

        public IEnumerable<SelectListItem> ExtensionList { get; set; }

        public string TagString { get; set; }
    }
}
