using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class GlobalDatasetExtensions
    {
        public static GlobalDataset ToGlobalDataset(this Dataset dataset)
        {
            return new GlobalDataset
            {
                GlobalDatasetId = dataset.GlobalDatasetId.Value,
                DatasetName = dataset.DatasetName,
                DatasetSaidAssetCode = dataset.Asset.SaidKeyCode,
                EnvironmentDatasets = new List<EnvironmentDataset> { dataset.ToEnvironmentDataset() }
            };
        }

        public static EnvironmentDataset ToEnvironmentDataset(this Dataset dataset)
        {
            return new EnvironmentDataset
            {
                DatasetId = dataset.DatasetId,
                DatasetDescription = dataset.DatasetDesc,
                CategoryCode = dataset.DatasetCategories.First().Name,
                NamedEnvironment = dataset.NamedEnvironment,
                NamedEnvironmentType = dataset.NamedEnvironmentType.ToString(),
                OriginationCode = dataset.OriginationCode,
                IsSecured = dataset.IsSecured,
                FavoriteUserIds = dataset.Favorities?.Select(x => x.UserId).ToList(),
                EnvironmentSchemas = new List<EnvironmentSchema>()
            };
        }

        public static EnvironmentSchema ToEnvironmentSchema(this SchemaResultDto schemaResultDto)
        {
            return new EnvironmentSchema
            {
                SchemaId = schemaResultDto.SchemaId,
                SchemaName = schemaResultDto.SchemaName,
                SchemaDescription = schemaResultDto.SchemaDescription,
                SchemaSaidAssetCode = schemaResultDto.SaidAssetCode
            };
        }

        public static EnvironmentSchema ToEnvironmentSchema(this FileSchemaDto fileSchemaDto)
        {
            return new EnvironmentSchema
            {
                SchemaId = fileSchemaDto.SchemaId,
                SchemaName = fileSchemaDto.Name,
                SchemaDescription = fileSchemaDto.Description
            };
        }
    }
}
