using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DatasetFileGridModel
    {
        public DatasetFileGridModel(DatasetFile f)
        {
            this.Id = f.DatasetFileId;
            this.Name = f.FileName;
            this.UploadUserName = f.UploadUserName;
            this.ModifiedDTM = f.ModifiedDTM;
            this.CreateDTM = f.CreateDTM;
            this.s3Key = f.FileLocation;
            this.ConfigFileName = f.DatasetFileConfig.Name;
            this.ConfigFileDesc = f.DatasetFileConfig.Description;
            this.VersionId = f.VersionId;
            this.IsSensitive = f.IsSensitive;

        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameHref
        {
            get
            {
                string href = null;
                href = $"<a href = \"#\" onclick=\"data.DatasetDetail.GetDatasetFileVersions({ Id })\" title=\"View File Versions\">{Name}</a>";
                return href;
            }
        }
        public string UploadUserName { get; set; }        
        public DateTime ModifiedDTM { get; set; }
        public DateTime CreateDTM { get; set; }
        public string ActionLinks
        {
            get
            {
                string href = null;
                //if (IsPreviewCompatible)
                //{
                href = "<a href = \"#\" onclick=\"data.DatasetDetail.PreviewDatafileModal(" + Id + ")\" class=\"table-row-icon row-filepreview-icon\" title=\"Preview file\"><i class='glyphicon glyphicon-search text-primary'></i></a>";
                //}

                 if ((IsSensitive && CanDwnldSenstive) || (!IsSensitive && CanDwnldNonSensitive))
                {
                    href += "<a href = \"#\" onclick=\"data.DatasetDetail.DownloadDatasetFile(" + Id + ")\" class=\"table-row-icon row-filedownload-icon\" title=\"Download File\"><i class='glyphicon glyphicon-cloud-download text-primary'></i></a>";
                } 

                return href;
            }
        }
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }

        //PreviewDatafileModal
        public string s3Key { get; set; }
        public string VersionId { get; set; }
        public Boolean CanDwnldSenstive { get; set; }
        public Boolean CanDwnldNonSensitive { get; set; }
        public Boolean IsSensitive { get; set; }
    }
}