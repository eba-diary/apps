using Sentry.data.Web.Models.ApiModels.Migration;
using System.Collections.Generic;

namespace Sentry.data.Web.Extensions
{
    public static class MigrationExtensions
    {
        public static Core.SchemaMigrationRequest ToDto(this SchemaMigrationRequestModel model)
        {
            return new Core.SchemaMigrationRequest()
            {
                SourceSchemaId = model.SourceSchemaId,
                TargetDataFlowNamedEnvironment = model.TargetDataFlowNamedEnviornment,
                TargetDatasetId = model.TargetDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment
            };
        }

        public static Core.DatasetMigrationRequest ToDto(this DatasetMigrationRequestModel model)
        {
            Core.DatasetMigrationRequest request = new Core.DatasetMigrationRequest()
            {
                SourceDatasetId = model.SourceDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment,
                TargetDatasetId = model.TargetDatasetId,
                SchemaMigrationRequests = new List<Core.SchemaMigrationRequest>()
            };

            foreach (SchemaMigrationRequestModel schemaMigrationRequestModel in model.SchemaMigrationRequests)
            {
                request.SchemaMigrationRequests.Add(schemaMigrationRequestModel.ToDto());
            }

            return request;
        }
    }
}