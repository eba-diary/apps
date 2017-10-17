using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class EditDatasetFileConfigModel : DatasetFileConfigsModel
    {
        public EditDatasetFileConfigModel() { }
        public EditDatasetFileConfigModel(DatasetFileConfig dfc) : base(dfc)
        { }
        public string DeleteHref
        {
            get
            {
                string href = null;
                href = $"<a class=\"delete\" href=\"\">Delete</a>";
                return href;
            }
        }
        public string ModifyType { get; set; }
    }
}