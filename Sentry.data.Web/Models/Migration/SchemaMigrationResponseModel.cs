using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.Migration
{
    public class SchemaMigrationResponseModel
    {
        public MigrationResponseModel SchemaResponse { get; set; }
        public MigrationResponseModel SchemaRevisionResponse { get; set; }
        public MigrationResponseModel DataFlowResponse { get; set; }
    }
}