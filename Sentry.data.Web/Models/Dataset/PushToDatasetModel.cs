using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class PushToDatasetModel : BaseDatasetModel
    {
        public PushToDatasetModel()
        {

        }

        [DisplayName("File Name")]
        public string DatasetFileName { get; set; }

        [DisplayName("File Name Override (exclude extension)")]
        public string FileNameOverride { get; set; }
    }
}