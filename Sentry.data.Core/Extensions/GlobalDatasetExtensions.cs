using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class GlobalDatasetExtensions
    {
        public static GlobalDataset ToGlobalDataset(this List<Dataset> datasets, List<KeyValuePair<int, FileSchema>> datasetIdSchemas, List<DataFlow> datasetDataFlows)
        {
            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = datasets.First().GlobalDatasetId.Value,
                DatasetName = datasets.First().DatasetName,
                DatasetSaidAssetCode = datasets.First().Asset.SaidKeyCode,
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            foreach (Dataset dataset in datasets)
            {
                EnvironmentDataset environmentDataset = dataset.ToEnvironmentDataset();

                foreach (FileSchema datasetSchema in datasetIdSchemas.Where(x => x.Key == dataset.DatasetId).Select(x => x.Value).ToList())
                {
                    EnvironmentSchema environmentSchema = datasetSchema.ToEnvironmentSchema();
                    environmentSchema.SchemaSaidAssetCode = datasetDataFlows.FirstOrDefault(x => x.SchemaId == datasetSchema.SchemaId)?.SaidKeyCode;

                    environmentDataset.EnvironmentSchemas.Add(environmentSchema);
                }

                globalDataset.EnvironmentDatasets.Add(environmentDataset);
            }

            return globalDataset;
        }

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
                FavoriteUserIds = dataset.Favorities?.Select(x => x.UserId).ToList() ?? new List<string>(),
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

        public static EnvironmentSchema ToEnvironmentSchema(this FileSchema fileSchema)
        {
            return new EnvironmentSchema
            {
                SchemaId = fileSchema.SchemaId,
                SchemaName = fileSchema.Name,
                SchemaDescription = fileSchema.Description
            };
        }

        public static List<SearchGlobalDatasetDto> ToSearchResults(this List<GlobalDataset> globalDatasets, string userId = null)
        {
            return globalDatasets.Select(x => x.ToSearchResult(userId)).ToList();
        }

        #region Private
        private static SearchGlobalDatasetDto ToSearchResult(this GlobalDataset globalDataset, string userId = null)
        {
            //use first prod environment dataset for display fields
            EnvironmentDataset targetEnvironmentDataset = globalDataset.EnvironmentDatasets.FirstOrDefault(x => x.NamedEnvironmentType == NamedEnvironmentType.Prod.ToString());

            //if no prod environment, use the most recent added
            globalDataset.EnvironmentDatasets.Reverse();
            if (targetEnvironmentDataset == null)
            {
                targetEnvironmentDataset = globalDataset.EnvironmentDatasets.First();
            }

            return new SearchGlobalDatasetDto
            {
                GlobalDatasetId = globalDataset.GlobalDatasetId,
                DatasetName = globalDataset.DatasetName,
                DatasetSaidAssetCode = globalDataset.DatasetSaidAssetCode,
                DatasetDescription = targetEnvironmentDataset.DatasetDescription,
                CategoryCode = targetEnvironmentDataset.CategoryCode,
                NamedEnvironments = globalDataset.EnvironmentDatasets.OrderByDescending(x => x.NamedEnvironmentType).Select(x => x.NamedEnvironment).ToList(),
                IsSecured = targetEnvironmentDataset.IsSecured,
                IsFavorite = globalDataset.EnvironmentDatasets.Any(x => x.FavoriteUserIds.Contains(userId)),
                TargetDatasetId = targetEnvironmentDataset.DatasetId,
                SearchHighlights = globalDataset.SearchHighlights.ToDtos()
            };
        }
        #endregion
    }
}
