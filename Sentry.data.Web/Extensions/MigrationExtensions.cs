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
            DatasetMigrationRequest requset = new DatasetMigrationRequest()
            {
                SourceDatasetId = model.SourceDatasetId,
                TargetDatasetNamedEnvironment = model.TargetDatasetNamedEnvironment
            };

            return requset;
        }
    }
}