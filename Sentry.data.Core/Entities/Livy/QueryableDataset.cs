using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Livy
{
    public class QueryableDataset
    {
        public List<QueryableConfig> Configs { get; set; }
        public string datasetCategory { get; set; }
        public string datasetColor { get; set; }
    }

    public class QueryableConfig
    {
        public string configName { get; set; }
        public string bucket { get; set; }
        public string s3Key { get; set; }
        public string description { get; set; }

        public string primaryFileId { get; set; }

        public List<string> extensions { get; set; }
        public int fileCount { get; set; }
        public Boolean IsGeneric { get; set; }
        public Boolean IsPowerUser { get; set; }
    }
}
