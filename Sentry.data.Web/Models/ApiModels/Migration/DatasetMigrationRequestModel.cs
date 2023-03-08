using System.Collections.Generic;

namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class DatasetMigrationRequestModel : BaseMigrationRequestModel
    {
        public int SourceDatasetId { get; set; }
        public List<DatasetSchemaMigrationRequestModel> SchemaMigrationRequests { get; set; } = new List<DatasetSchemaMigrationRequestModel>();
    }
}