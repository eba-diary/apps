using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class CreateDatasetModel : BaseDatasetModel
    {
        public CreateDatasetModel()
        {
            this.Category = "";
            //this.CategoryList = new List<string>();
            this.ChangedDtm = DateTime.MinValue;
            //this.CreationFreqDesc = DatasetFrequency.NonSchedule.ToString();  // Default to NonScheduled
            this.DatasetDesc = "";
            //this.DatasetDtm = DateTime.MinValue;
            this.DatasetName = "";
            this.FileExtension = "";
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

        /// <summary>
        /// AllCategories holds the sorted list of all possible categories.
        /// </summary>
        public IEnumerable<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllFrequencies { get; set; }

        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }

        public IEnumerable<SelectListItem> AllDatasetScopeTypes { get; set; }

        [Required]
        [DisplayName("Categories")]
        public int CategoryIDs { get; set; }

        [Required]
        [DisplayName("Upload Frequency")]
        public int FreqencyID { get; set; }

        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [Required]
        [DisplayName("Dataset Scope")]
        public int DatasetScopeTypeID { get; set; }

        //[DisplayName("Dataset File")]
        //public String DatasetFileName { get; set; }
    }
}
