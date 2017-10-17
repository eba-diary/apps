using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class CreateDataFileModel : BaseDatasetModel
    {
        public CreateDataFileModel()
        {
            //this.Category = "";
            //this.ChangedDtm = DateTime.MinValue;
            //this.CreationFreqDesc = DatasetFrequency.NonSchedule.ToString();  // Default to NonScheduled
            //this.DatasetDesc = "";
            ////this.DatasetDtm = DateTime.MinValue;
            //this.DatasetName = "";
            //this.FileExtension = "";
            //this.FileSize = 0;
            //this.DatasetId = 0;
            //this.OriginationCode = "";
            //this.RecordCount = 0;
            //this.S3Key = "";
            //this.SentryOwnerName = "";
            //this.UploadDtm = DateTime.MinValue;
            //this.UploadUserName = "";
            //this.IsSensitive = false;
            //this.CanDisplay = true;
        }

        public CreateDataFileModel(Dataset ds, IAssociateInfoProvider associateService) : base(ds, associateService)
        {
            this.CategoryIDs = ds.DatasetCategory.Id;
            this.FreqencyID = (int)Enum.Parse(typeof(DatasetFrequency), ds.CreationFreqDesc);
            this.OriginationID = (int)Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode);
            this.dsID = ds.DatasetId;
        }

        [DisplayName("File Upload")]
        public HttpPostedFile f { get; set; }

        public long ProgressConnectionId { get; set; }

        /// <summary>
        /// AllCategories holds the sorted list of all possible categories.
        /// </summary>
        public IEnumerable<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllFrequencies { get; set; }

        public IEnumerable<SelectListItem> AllOriginationCodes { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryIDs { get; set; }

        [Required]
        [DisplayName("Frequency")]
        public int FreqencyID { get; set; }

        [Required]
        [DisplayName("Origination Code")]
        public int OriginationID { get; set; }

        [DisplayName("Dataset")]
        public int dsID { get; set; }
    }
}