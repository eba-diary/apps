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
            //Default values
            Delimiter = ",";
            GuessingRows = 1000;
        }

        [DisplayName("File Name")]
        public string DatasetFileName { get; set; }


        [MaxLength(32)]
        [DisplayName("File Name Override")]
        public string FileNameOverride { get; set; }

        [DisplayName("Delimiter")]
        public string Delimiter { get; set; }

        [DisplayName("Guessing Rows")]
        public int GuessingRows { get; set; }
    }
}