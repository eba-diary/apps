using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class DatasetMigrationResponseModel
    {
        public bool IsDatasetMigrated { get; set; }
        public string DatasetMigrationReason { get; set; }
        public int DatasetId { get; set; }
        public List<SchemaMigrationResponseModel> SchemaMigrationResponse { get; set; }
    }
}