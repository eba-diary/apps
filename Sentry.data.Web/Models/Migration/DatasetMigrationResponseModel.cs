using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.Migration
{
    public class DatasetMigrationResponseModel
    {
        public MigrationResponseModel DatasetResponse { get; set; }
        public IEnumerable<SchemaMigrationResponseModel> SchemaResponses { get; set; }
    }
}