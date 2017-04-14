using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;

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

        }

        public int DatasetId { get; set; }

        public string Category { get; set; }

        public string DatasetName { get; set; }

        public string DatasetDesc { get; set; }

        public string CreationUserName { get; set; }

        public string SentryOwnerName { get; set; }

        public string UploadUserName { get; set; }

        public string OriginationCode { get; set; }

        public string FileExtension { get; set; }

        public DateTime DatasetDtm { get; set; }

        public DateTime ChangedDtm { get; set; }

        public DateTime UploadDtm { get; set; }

        public string CreationFreqDesc { get; set; }

        public long FileSize { get; set; }

        public long RecordCount { get; set; }

        public string S3Key { get; set; }

        public Boolean IsSensitive { get; set; }

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
        //public IList<String> CategoryList { get; set; }

        ///// <summary>
        ///// AllCategories holds the sorted list of all possible categories.
        ///// </summary>
        //public IEnumerable<SelectListItem> AllCategories { get; set; }

    }
}
