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
    public class CreateBusinessIntelligenceModel : BaseDatasetModel
    {
        public CreateBusinessIntelligenceModel()
        {
            this.Category = "";
            this.DatasetDesc = "";
            this.DatasetName = "";
            this.FileExtension = null;
            this.DatasetId = 0;
            this.S3Key = "";
            this.SentryOwnerName = "";
            this.UploadUserName = "";
            this.CanDisplay = true;
        }

        [Required]
        [DisplayName("Categories")]
        public int CategoryIDs { get; set; }

        [Required]
        [DisplayName("Sentry Owner")]
        public string OwnerID { get; set; }

        [Required]
        [DisplayName("Report Location")]
        public string Location { get; set; }

        public string LocationType { get; set; }

        [DisplayName("Update Frequency")]
        public int FreqencyID { get; set; }

        [DisplayName("Exhibit Type")]
        public int FileTypeId { get; set; }
    }
}
