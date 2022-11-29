using Sentry.data.Core.Entities.Migration;
using Sentry.data.Web.Models.ApiModels.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public static class MigrationExtensions
    {
        public static DatasetMigrationRequest ToDto(this DatasetMigrationRequestModel model)
        {
            DatasetMigrationRequest request = new DatasetMigrationRequest()
            {
                SourceDatasetId = model.SourceDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment
            };

            foreach (SchemaMigrationRequestModel schemaMigrationRequestModel in model.SchemaMigrationRequests)
            {
                request.SchemaMigrationRequests.Add(schemaMigrationRequestModel.ToDto());
            }

            return request;
        }

        public static SchemaMigrationRequest ToDto(this SchemaMigrationRequestModel model)
        {
            return new SchemaMigrationRequest()
            {
                SourceSchemaId = model.SourceSchemaId,
                TargetDataFlowNamedEnvironment = model.TargetDataFlowNamedEnviornment,
                TargetDatasetId = model.TargetDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment
            };
        }
    }
}