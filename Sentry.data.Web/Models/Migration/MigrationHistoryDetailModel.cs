using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Sentry.data.Core;

namespace Sentry.data.Web
{
    public class MigrationHistoryDetailModel
    {
        public int MigrationHistoryDetailId { get; set; }
        public int MigrationHistoryId { get; set; }

        public int SourceDatasetId { get; set; }
        public bool IsDatasetMigrated { get; set; }
        public int? DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetMigrationMessage { get; set; }


        public int? SourceSchemaId { get; set; }
        public bool IsSchemaMigrated { get; set; }
        public int? SchemaId { get; set; }
        public string SchemaName { get; set; }
        public string SchemaMigrationMessage { get; set; }


        public bool IsDataFlowMigrated { get; set; }
        public int? DataFlowId { get; set; }
        public string DataFlowName { get; set; }
        public string DataFlowMigrationMessage { get; set; }


        public bool IsSchemaRevisionMigrated { get; set; }
        public int? SchemaRevisionId { get; set; }
        public string SchemaRevisionName { get; set; }
        public string SchemaRevisionMigrationMessage { get; set; }

        public string Type { get { return (DatasetId != null) ? GlobalConstants.MigrationHistory.TYPE_DATASET : GlobalConstants.MigrationHistory.TYPE_SCHEMA;  }   }
        public string Name { get {  return (DatasetId != null) ? DatasetName : SchemaName; } }
        public bool Success { get { return true; } }
    }
}