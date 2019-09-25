using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ManageConfigsModel
    {
        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string CategoryColor { get; set; }
        public List<DatasetFileConfigsModel> DatasetFileConfigs { get; set; }        
    }
}