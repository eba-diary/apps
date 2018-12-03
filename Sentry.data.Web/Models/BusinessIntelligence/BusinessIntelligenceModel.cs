using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System.Web.Script.Serialization;

namespace Sentry.data.Web
{
    public class BusinessIntelligenceModel : BaseDatasetModel
    {
        public BusinessIntelligenceModel()
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

        public BusinessIntelligenceModel(Dataset ds, IAssociateInfoProvider associateService) : base(ds, associateService)
        {
            OwnerID = ds.SentryOwnerName;
            Location = ds.Metadata.ReportMetadata.Location;
            LocationType = ds.Metadata.ReportMetadata.LocationType;
            FreqencyID = ds.Metadata.ReportMetadata.Frequency;
            FileTypeId = ds.DatasetFileConfigs.First().FileTypeId;
            TagString = new JavaScriptSerializer().Serialize(ds.Tags.Select(x => x.GetSearchableTag()));
        }

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

        public string TagString { get; set; }
    }
}