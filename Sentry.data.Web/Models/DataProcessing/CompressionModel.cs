using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class CompressionModel
    {
        public CompressionModel()
        {
            FileNameExclusionList = new List<string>(); 
        }
        [DisplayName("Compression Type")]
        public string CompressionType { get; set; }
        public IEnumerable<SelectListItem> CompressionTypesDropdown { get; set; }

        public string NewFileNameExclusionList { get; set; }

        //This is for post backs that fail.
        public List<string> FileNameExclusionList { get; set; }
    }
}