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
            this.ParentDataSetID = f.Dataset.DatasetId;
            this.IsBundled = f.IsBundled;
            this.Information = f.Information;
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

                if (CanPreviewDataset && correctFileTypeForPreview)
                {
                    href += "<a href = \"#\" onclick=\"data.Dataset.PreviewDatafileModal(" + Id + ")\" class=\"table-row-icon row-filepreview-icon\" title=\"Preview file\"><i class='glyphicon glyphicon-search text-primary'></i></a>";
                }
                else if (!correctFileTypeForPreview)
                {
                    href += "<a disabled class=\"table-row-icon row-filepreview-icon disabled\" title=\"Not available for this file type.\"><i class='glyphicon glyphicon-search text-primary disabled' style='color:gray;'></i></a>";
                }
                else
                {
                    href += "<a disabled class=\"table-row-icon row-filepreview-icon disabled\" title=\"This operation is not available.\"><i class='glyphicon glyphicon-search text-primary disabled' style='color:gray;'></i></a>";
                }

                if (CanViewFullDataset)
                {
                    href += "<a href = \"#\" onclick=\"data.Dataset.DownloadDatasetFile(" + Id + ")\" class=\"table-row-icon row-filedownload-icon\" title=\"Download File\"><i class='glyphicon glyphicon-cloud-download text-primary'></i></a>";
                }
                if (CanEditDataset)
                {
                    href += "<a href = \"#\" onclick=\"data.Dataset.EditDataFileInformation(" + Id + ")\" class=\"table-row-icon\" title=\"Edit File\"><i class='glyphicon glyphicon-edit text-primary'></i></a>";
                }

                if (CanViewFullDataset && Utilities.IsExtentionPushToSAScompatible(Path.GetExtension(Name)))
                {
                    href += "<a href = \"#\" onclick=\"data.Dataset.FileNameModal(" + Id + ")\" title=\"Push to SAS\">" +
                        "<img src=\"../../Images/sas_logo_min.png\" style=\" height: 15px; margin-bottom: 4px; margin-left: 5px;\"/>" +
                        "</a>";

                }
                return href;
            }
        }
        public string ConfigFileName { get; set; }
        public string ConfigFileDesc { get; set; }

        //PreviewDatafileModal
        public string s3Key { get; set; }
        public string VersionId { get; set; }
        public int ParentDataSetID { get; set; }
        public Boolean IsBundled { get; set; }
        public string Information { get; set; }


        public bool CanPreviewDataset { get; set; }
        public bool CanViewFullDataset { get; set; }
        public bool CanEditDataset { get; set; }
    }
}