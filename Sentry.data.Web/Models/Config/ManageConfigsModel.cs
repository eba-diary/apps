using Sentry.data.Core;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ManageConfigsModel
    {
        public int DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetNamedEnvironment { get; set; }
        public string CategoryColor { get; set; }
        public List<DatasetFileConfigsModel> DatasetFileConfigs { get; set; }
        public UserSecurity Security { get; set; }
        public bool DisplayDataflowMetadata { get; set; }
        public bool DisplayDataflowEdit { get; set; }
    }
}