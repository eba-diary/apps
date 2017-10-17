using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.DatasetLoader.Entities
{
    class DatasetMetadata
    {
        public int datasetId { get; set; }
        public string datasetName { get; set; }
        public string category { get; set; }
        public string datasetNamePrefix { get; set; }
        public string datasetNameSufix { get; set; }
        public string description { get; set; }
        public string createUser { get; set; }
        public string owner { get; set; }
        public string frequency { get; set; }
        public string notificationOn { get; set; }
        public string notificationEmail { get; set; }
        public bool overwrite { get; set; }
    }
}
