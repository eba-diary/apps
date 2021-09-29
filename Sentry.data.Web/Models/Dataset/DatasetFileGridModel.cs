using Sentry.data.Common;
using Sentry.data.Core;
using System;
using System.IO;

namespace Sentry.data.Web
{
    public class DatasetFileGridModel
    {

        public DatasetFileGridModel()
        {
        }

        public DatasetFileGridModel(DatasetFile f, IAssociateInfoProvider associateInfoService, IDataFeatures dataFeatures)
        {
            this.Id = f.DatasetFileId;
            this.Name = f.FileName;
            this.FlowExecutionGuid = f.FlowExecutionGuid;
            this.RunInstanceGuid = f.RunInstanceGuid;


            //Used to differentiate between service and user accounts, user accounts will parse into a numeric value
            this.UploadUserName = int.TryParse(f.UploadUserName, out _) ? associateInfoService.GetAssociateInfo(f.UploadUserName).FullName : f.UploadUserName;

            //this.ModifiedDTM = (dataFeatures.CLA3048_StandardizeOnUTCTime.GetValue()) ? f.ModifiedDTM.ToLocalTime() : f.ModifiedDTM;
            this.ModifiedDtm = new DateTime(f.ModifiedDTM.Ticks, DateTimeKind.Utc);
            this.CreateDtm = (dataFeatures.CLA3048_StandardizeOnUTCTime.GetValue()) ? f.CreateDTM.ToLocalTime() : f.CreateDTM;
            this.S3Key = f.FileLocation;
            this.ConfigFileName = f.DatasetFileConfig.Name;
            this.ConfigFileDesc = f.DatasetFileConfig.Description;
            this.VersionId = f.VersionId;
            this.ParentDataSetID = f.Dataset.DatasetId;
            this.IsBundled = f.IsBundled;
            this.Information = f.Information;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string FlowExecutionGuid { get; set; }
        public string RunInstanceGuid { get; set; }
        public string UploadUserName { get; set; }
        public DateTime ModifiedDtm { get; set; }
        public DateTime CreateDtm { get; set; }
        public string ActionLinks
        {
            get
            {
                string href = "";
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
        public string S3Key { get; set; }
        public string VersionId { get; set; }
        public int ParentDataSetID { get; set; }
        public Boolean IsBundled { get; set; }
        public string Information { get; set; }


        public bool CanPreviewDataset { get; set; }
        public bool CanViewFullDataset { get; set; }
        public bool CanEditDataset { get; set; }
    }
}