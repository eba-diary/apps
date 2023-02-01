using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class DatasetMigrationRequestModel : MigrationRequestModel
    {
        public int SourceDatasetId { get; set; }
        public List<SchemaMigrationRequestModel> SchemaMigrationRequests { get; set; } = new List<SchemaMigrationRequestModel>();
    }
}