using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class SchemaMigrationResponseModel
    {
        public int SourceSchemaId { get; set; }
        public bool IsSchemaMigrated { get; set; }
        public string SchemaMigrationMessage { get; set; }
        public int SchemaId { get; set; }
        public string SchemaName { get; set; }

        public bool IsSchemaRevisionMigrated { get; set; }
        public string SchemaRevisionMigrationMessage { get; set; }
        public int SchemaRevisionId { get; set; }
        public string SchemaRevisionName { get; set; }

        public bool IsDataFlowMigrated { get; set; }
        public string DataFlowMigrationMessage { get; set; }
        public int DataFlowId { get; set; }
        public string DataFlowName { get; set; }
    }
}