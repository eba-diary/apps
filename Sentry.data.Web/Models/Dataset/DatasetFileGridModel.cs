using Sentry.data.Common;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.IO;

namespace Sentry.data.Web
{
    public class DatasetFileGridModel
    {

        public DatasetFileGridModel()
        {
        }

        public DatasetFileGridModel(DatasetFile f, IAssociateInfoProvider associateInfoService, bool CLA3048_StandardizeOnUTCTime)
        {
            this.Id = f.DatasetFileId;
            this.FileName = f.FileName;
            this.FlowExecutionGuid = f.FlowExecutionGuid;
            this.RunInstanceGuid = f.RunInstanceGuid;


            //Used to differentiate between service and user accounts, user accounts will parse into a numeric value
            this.UploadUserName = int.TryParse(f.UploadUserName, out _) ? associateInfoService.GetAssociateInfo(f.UploadUserName).FullName : f.UploadUserName;

            this.ModifiedDtm = CLA3048_StandardizeOnUTCTime ? DateTime.SpecifyKind(f.ModifiedDTM, DateTimeKind.Utc) : f.ModifiedDTM;
            this.CreateDtm = DateTime.SpecifyKind(f.CreatedDTM, DateTimeKind.Utc);
            this.S3Key = f.FileLocation;
            this.ConfigFileName = f.DatasetFileConfig.Name;
            this.ConfigFileDesc = f.DatasetFileConfig.Description;
            this.VersionId = f.VersionId;
            this.ParentDataSetID = f.Dataset.DatasetId;
            this.IsBundled = f.IsBundled;
            this.Information = f.Information;
            this.ObjectStatus = f.ObjectStatus;
        }
        public int Id { get; set; }
        public string FileName { get; set; }
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
                if (HasDataAccess)
                {
                    href += "<a href = \"#\" onclick=\"data.Dataset.DownloadDatasetFile(" + Id + ")\" class=\"table-row-icon row-filedownload-icon\" title=\"Download File\"><em class='fas fa-cloud-download-alt text-primary'></em></a>";
                }

                if (HasFullViewDataset && Utilities.IsExtentionPushToSAScompatible(Path.GetExtension(FileName)))
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
        public UserSecurity Security { get; set; }
        public ObjectStatusEnum ObjectStatus { get; set; }


        #region Security
        public bool HasDataAccess { get; set; }
        public bool HasDataFileEdit { get; set; }
        public bool HasFullViewDataset { get; set; }
        #endregion

    }
}