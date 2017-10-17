using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class BaseDatasetFileModel
    {
        public BaseDatasetFileModel()
        {
        }

        public BaseDatasetFileModel(DatasetFile dsf)
        {
            this.DatasetFileId = dsf.DatasetFileId;
            this.FileName = dsf.FileName;
            this.Dataset = dsf.Dataset;
            this.UploadUserName = dsf.UploadUserName;
            this.CreateDTM = dsf.CreateDTM;
            this.ModifiedDTM = dsf.ModifiedDTM;
            this.FileLocation = dsf.FileLocation;
        }

        public int DatasetFileId { get; set; }
        public string FileName { get; set; }
        
        public Dataset Dataset { get; set; }

        public string UploadUserName { get; set; }

        public DateTime CreateDTM { get; set; }

        public DateTime ModifiedDTM { get; set; }

        public string FileLocation { get; set; }

        public int VersionCount { get; set; }

        //public string DownloadHref
        //{
        //    get
        //    {
        //        string href = null;

        //        href = $"< a href = \"@Url.Action(\"Dataset\")/{0}\" target=\"_blank\" class=\"table-row-icon\" title=\"Edit User\"><i class='glyphicon glyphicon-cloud-download text-primary'></i></a>";

        //        return href;
        //    }
        //}

    }
}