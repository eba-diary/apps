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
        public Boolean HasSchema { get; set; }
        public Boolean HasQueryableSchema { get; set; }
        public Boolean IsGeneric { get; set; }
        public List<QueryableSchema> Schemas { get; set; }
    }
    
    public class QueryableSchema
    {
        //This is assuming only a single hive table per schema revision.
        //  The controller will filter to a single hive table based on
        //  IsPrimary property on HiveTable.  Future expansion would need
        //  add another class and expose the multiple hive tables.
        public string SchemaName { get; set; }
        public string SchemaDSC { get; set; }
        public int SchemaID { get; set; }
        public int RevisionID { get; set; }
        public string HiveDatabase { get; set; }
        public string HiveTable { get; set; }
        public string HiveTableStatus { get; set; }
        public Boolean HasTable { get; set; }
    }
}
