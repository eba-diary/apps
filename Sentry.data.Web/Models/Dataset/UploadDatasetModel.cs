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
    public class UploadDatasetModel : BaseDatasetModel
    {
        public UploadDatasetModel()
        {
            this.Category = "";
            //this.CategoryList = new List<string>();
            this.ChangedDtm = DateTime.MinValue;
            this.CreationFreqDesc = DatasetFrequency.NonSchedule.ToString();  // Default to NonScheduled
            this.DatasetDesc = "";
            this.DatasetDtm = DateTime.MinValue;
            this.DatasetName = "";
            this.FileExtension = "";
            this.FileSize = 0;
            this.DatasetId = 0;
            this.OriginationCode = "";
            this.RecordCount = 0;
            this.S3Key = "";
            this.SentryOwnerName = "";
            this.UploadDtm = DateTime.MinValue;
            this.UploadUserName = "";
            this.IsSensitive = true;

        }

        //public UploadDatasetModel(Dataset ds, IList<String> catList) : base(ds)
        //{
        //    System.IO.FileStream fs = new System.IO.FileStream(ds.DatasetName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

        //    if (null != catList)
        //    {
        //        List<SelectListItem> sliCatList = new List<SelectListItem>();

        //        foreach (String cat in catList)
        //        {
        //            SelectListItem sliCat = new SelectListItem();
        //            sliCat.Text = cat;
        //            sliCat.Value = cat;
        //            sliCatList.Add(sliCat);
        //        }
        //        this.AllCategories = sliCatList.AsEnumerable();
        //    }

        //    //if (null != ds.Columns)
        //    //{
        //    //    this.md_Columns = new List<_DatasetMetadataModel>();
        //    //    foreach (DatasetMetadata dsm in ds.Columns)
        //    //    {
        //    //        md_Columns.Add(new _DatasetMetadataModel(dsm));
        //    //    }
        //    //}

        //    //if (null != ds.Metadata)
        //    //{
        //    //    this.md_Metadata = new List<_DatasetMetadataModel>();
        //    //    foreach (DatasetMetadata dsm in ds.Metadata)
        //    //    {
        //    //        md_Metadata.Add(new _DatasetMetadataModel(dsm));
        //    //    }
        //    //}
        //}

        [DisplayName("File Upload")]
        public HttpPostedFile f { get; set; }

        //[DisplayName("Column Metadata")]
        //[Required]
        //public IList<_DatasetMetadataModel> md_Columns { get; set; }

        //[DisplayName("User Metadata")]
        //[Required]
        //public IList<_DatasetMetadataModel> md_Metadata { get; set; }

        ///// <summary>
        ///// CategoryIDs holds the IDs of the selected categories.  
        ///// It is needed for model binding and MVC editor helpers
        ///// </summary>
        //[DisplayName("Categories")]
        //[Required]
        //public int[] CategoryIDs { get; set; }

        public long ProgressConnectionId { get; set; }

        /// <summary>
        /// AllCategories holds the sorted list of all possible categories.
        /// </summary>
        public IEnumerable<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllFrequencies { get; set; }

        [DisplayName("Categories")]
        public int CategoryIDs { get; set; }

        [DisplayName("Frequency")]
        public int FreqencyID { get; set; }

        [DisplayName("Dataset File")]
        public String DatasetFileName { get; set; }
    }
}
