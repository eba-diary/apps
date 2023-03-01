using System.Collections.Generic;

namespace Sentry.data.Core.Extensions
{
    public static class MigrationExtensions
    {
        public static List<SchemaMigrationRequest> MapToSchemaMigrationRequest(this DatasetMigrationRequest request)
        {
            List<SchemaMigrationRequest> requestList = new List<SchemaMigrationRequest>();
            foreach (var item in request.SchemaMigrationRequests)
            {
                requestList.Add(new SchemaMigrationRequest()
                {
                    SourceSchemaId = item.SourceSchemaId,
                    TargetDatasetId = request.TargetDatasetId,
                    TargetDataFlowNamedEnvironment = item.TargetDataFlowNamedEnvironment,
                    TargetDatasetNamedEnvironment = request.TargetDatasetNamedEnvironment,
                    TargetDatasetNamedEnvironmentType = request.TargetDatasetNamedEnvironmentType
                });
            }
            return requestList;
        }
    }
}
