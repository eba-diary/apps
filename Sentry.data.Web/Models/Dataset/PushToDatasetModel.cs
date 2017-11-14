using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class PushToDatasetModel : BaseDatasetFileModel
    {
        public PushToDatasetModel()
        {

        }

        [DisplayName("File Name")]
        public string DatasetFileName { get; set; }


        [MaxLength(32)]
        [DisplayName("File Name Override (exclude extension)")]
        public string FileNameOverride { get; set; }
    }
}