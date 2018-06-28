using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Common;
using System.IO;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class DatasetFileGridModel
    {
        public DatasetFileGridModel()
        {
        }

        public DatasetFileGridModel(DatasetFile f, IAssociateInfoProvider associateInfoService)
        {
            this.Id = f.DatasetFileId;
            this.Name = f.FileName;

            //Used to differentiate between service and user accounts, user accounts will parse into a numeric value
            int n;
            this.UploadUserName = int.TryParse(f.UploadUserName, out n) ? associateInfoService.GetAssociateInfo(f.UploadUserName).FullName : f.UploadUserName;

            this.ModifiedDTM = f.ModifiedDTM;
            this.CreateDTM = f.CreateDTM;
            this.s3Key = f.FileLocation;
            this.ConfigFileName = f.DatasetFileConfig.Name;
            this.ConfigFileDesc = f.DatasetFileConfig.Description;
            this.VersionId = f.VersionId;
            this.IsSensitive = f.Dataset.IsSensitive;
            this.ParentDataSetID = f.Dataset.DatasetId;
            this.IsBundled = f.IsBundled;
            this.Information = f.Information;
            this.IsUsable = f.IsUsable;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string UploadUserName { get; set; }        
        public DateTime ModifiedDTM { get; set; }
        public DateTime CreateDTM { get; set; }
        public string ActionLinks
        {
            get
            {
                string href = "";
                Boolean correctFileTypeForPreview = (Path.GetExtension(Name).Contains("csv") || Path.GetExtension(Name).Contains("txt") || Path.GetExtension(Name).Contains("json"));
                Boolean correctSensitivityForDownload = ((IsSensitive && CanDwnldSenstive) || (!IsSensitive && CanDwnldNonSensitive));

                if (IsUsable)
                {
                    if (CanPreview && correctSensitivityForDownload && correctFileTypeForPreview)
                    {
                        href += "<a href = \"#\" onclick=\"data.DatasetDetail.PreviewDatafileModal(" + Id + ")\" class=\"table-row-icon row-filepreview-icon\" title=\"Preview file\"><i class='glyphicon glyphicon-search text-primary'></i></a>";
                    }
                    else if(!correctFileTypeForPreview)
                    {
                        href += "<a disabled class=\"table-row-icon row-filepreview-icon disabled\" title=\"Not available for this file type.\"><i class='glyphicon glyphicon-search text-primary disabled' style='color:gray;'></i></a>";
                    }
                    else
                    {
                        href += "<a disabled class=\"table-row-icon row-filepreview-icon disabled\" title=\"This operation is not available.\"><i class='glyphicon glyphicon-search text-primary disabled' style='color:gray;'></i></a>";
                    }

                    if (correctSensitivityForDownload)
                    {
                        href += "<a href = \"#\" onclick=\"data.DatasetDetail.DownloadDatasetFile(" + Id + ")\" class=\"table-row-icon row-filedownload-icon\" title=\"Download File\"><i class='glyphicon glyphicon-cloud-download text-primary'></i></a>";
                    }
                }
                if (CanEdit)
                {
                    href += "<a href = \"#\" onclick=\"data.DatasetDetail.EditDataFileInformation(" + Id + ")\" class=\"table-row-icon\" title=\"Edit File\"><i class='glyphicon glyphicon-edit text-primary'></i></a>";
                }

                if (IsUsable)
                {
                    if (correctSensitivityForDownload && Utilities.IsExtentionPushToSAScompatible(Path.GetExtension(Name)))
                    {
                        href += "<a href = \"#\" onclick=\"data.Dataset.FileNameModal(" + Id + ")\" title=\"Push to SAS\">" +
                            "<img src=\"../../Images/sas_logo_min.png\" style=\" height: 15px; margin-bottom: 4px; margin-left: 5px;\"/>" +
                            "</a>";

                    }
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
        public Boolean CanEdit { get; set; }
        public Boolean IsSensitive { get; set; }
        public Boolean CanPreview { get; set; }
        public int ParentDataSetID { get; set; }
        public Boolean IsBundled { get; set; }
        public Boolean IsUsable { get; set; }
        public string Information { get; set; }
    }
}