using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class MigrationHistoryDetail
    {
        public virtual int MigrationHistoryDetailId { get; set; }
        public virtual int MigrationHistoryId { get; set; }

        public virtual int SourceDatasetId { get; set; }
        public virtual bool IsDatasetMigrated { get; set; }
        public virtual int? DatasetId { get; set; }
        public virtual string DatasetName { get; set; }
        public virtual string DatasetMigrationMessage { get; set; }


        public virtual int? SourceSchemaId { get; set; }
        public virtual bool IsSchemaMigrated { get; set; }
        public virtual int? SchemaId { get; set; }
        public virtual string SchemaName { get; set; }
        public virtual string SchemaMigrationMessage { get; set; }


        public virtual bool IsDataFlowMigrated { get; set; }
        public virtual int? DataFlowId { get; set; }
        public virtual string DataFlowName { get; set; }
        public virtual string DataFlowMigrationMessage { get; set; }


        public virtual bool IsSchemaRevisionMigrated { get; set; }
        public virtual int? SchemaRevisionId { get; set; }
        public virtual string SchemaRevisionName { get; set; }
        public virtual string SchemaRevisionMigrationMessage { get; set; }

       
    }
}
