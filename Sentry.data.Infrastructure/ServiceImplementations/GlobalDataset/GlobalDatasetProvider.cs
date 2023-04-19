using Nest;
using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetProvider : IGlobalDatasetProvider
    {
        private readonly IElasticDocumentClient _elasticDocumentClient;
        private readonly IDatasetContext _datasetContext;

        public GlobalDatasetProvider(IElasticDocumentClient elasticDocumentClient, IDatasetContext datasetContext)
        {
            _elasticDocumentClient = elasticDocumentClient;
            _datasetContext = datasetContext;
        }

        #region Search
        public async Task<List<GlobalDataset>> SearchGlobalDatasetsAsync(BaseFilterSearchDto filterSearchDto)
        {
            SearchRequest<GlobalDataset> searchRequest = GetSearchRequest(filterSearchDto);
            searchRequest.Size = 10000;

            ElasticResult<GlobalDataset> elasticResult = await _elasticDocumentClient.SearchAsync(searchRequest);

            return elasticResult.Documents.ToList();
        }

        public async Task<List<FilterCategoryDto>> GetGlobalDatasetFiltersAsync(BaseFilterSearchDto filterSearchDto)
        {
            SearchRequest<GlobalDataset> searchRequest = GetSearchRequest(filterSearchDto);
            searchRequest.Size = 0;
            searchRequest.Aggregations = null;

            ElasticResult<GlobalDataset> elasticResult = await _elasticDocumentClient.SearchAsync(searchRequest);

            List<FilterCategoryDto> filterCategories = elasticResult.Aggregations.ToFilterCategories(filterSearchDto.FilterCategories);

            return filterCategories;
        }

        private SearchRequest<GlobalDataset> GetSearchRequest(BaseFilterSearchDto filterSearchDto)
        {
            BoolQuery searchQuery = filterSearchDto.ToSearchQuery<GlobalDataset>();

            return new SearchRequest<GlobalDataset>
            {
                Query = searchQuery
            };
        }
        #endregion

        #region Global Dataset
        public async Task AddUpdateGlobalDatasetAsync(GlobalDataset globalDataset)
        {
            await _elasticDocumentClient.IndexAsync(globalDataset).ConfigureAwait(false);
        }

        public async Task AddUpdateGlobalDatasetsAsync(List<GlobalDataset> globalDatasets)
        {
            await _elasticDocumentClient.IndexManyAsync(globalDatasets);
        }

        public async Task DeleteGlobalDatasetsAsync(List<int> globalDatasetIds)
        {
            List<GlobalDataset> globalDatasets = globalDatasetIds.Select(x => new GlobalDataset { GlobalDatasetId = x }).ToList();
            await _elasticDocumentClient.DeleteManyAsync(globalDatasets);
        }
        #endregion

        #region Environment Dataset
        public async Task AddUpdateEnvironmentDatasetAsync(int globalDatasetId, EnvironmentDataset environmentDataset)
        {
            GetByEnvironmentDatasetIdResult getByResult = await GetGlobalDatasetByEnvironmentDatasetIdAsync(globalDatasetId, environmentDataset.DatasetId).ConfigureAwait(false);

            if (getByResult.GlobalDataset != null)
            {
                if (getByResult.WasFound())
                {
                    environmentDataset.EnvironmentSchemas = getByResult.EnvironmentDataset.EnvironmentSchemas;
                    getByResult.GlobalDataset.EnvironmentDatasets.Remove(getByResult.EnvironmentDataset);
                }

                getByResult.GlobalDataset.EnvironmentDatasets.Add(environmentDataset);

                await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
            }
            else
            {
                Logger.Warn($"Global dataset {globalDatasetId} could not be found for add/update environment dataset {environmentDataset.DatasetId}");
            }
        }

        public async Task DeleteEnvironmentDatasetAsync(int environmentDatasetId)
        {
            GetByEnvironmentDatasetIdResult getByResult = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (getByResult.WasFound())
            {
                getByResult.GlobalDataset.EnvironmentDatasets.Remove(getByResult.EnvironmentDataset);

                if (!getByResult.GlobalDataset.EnvironmentDatasets.Any())
                {
                    //delete whole global dataset if no environmnet datasets left
                    await _elasticDocumentClient.DeleteByIdAsync<GlobalDataset>(getByResult.GlobalDataset.GlobalDatasetId).ConfigureAwait(false);
                }
                else
                {
                    await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
                }
            }
        }

        public async Task AddEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId)
        {
            GetByEnvironmentDatasetIdResult getByResult = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (getByResult.WasFound() && !getByResult.EnvironmentDataset.FavoriteUserIds.Contains(favoriteUserId))
            {
                getByResult.EnvironmentDataset.FavoriteUserIds.Add(favoriteUserId);

                await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
            }
        }

        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId)
        {
            GetByEnvironmentDatasetIdResult getByResult = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (getByResult.WasFound() && getByResult.EnvironmentDataset.FavoriteUserIds.Contains(favoriteUserId))
            {
                getByResult.EnvironmentDataset.FavoriteUserIds.Remove(favoriteUserId);

                await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
            }
        }
        #endregion

        #region Environment Schema
        public async Task AddUpdateEnvironmentSchemaAsync(int environmentDatasetId, EnvironmentSchema environmentSchema)
        {
            GetByEnvironmentDatasetIdResult getByResult = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (getByResult.WasFound())
            {
                EnvironmentSchema existingSchema = getByResult.EnvironmentDataset.EnvironmentSchemas.FirstOrDefault(x => x.SchemaId == environmentSchema.SchemaId);
                if (existingSchema != null)
                {
                    getByResult.EnvironmentDataset.EnvironmentSchemas.Remove(existingSchema);
                }

                getByResult.EnvironmentDataset.EnvironmentSchemas.Add(environmentSchema);

                await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
            }
            else
            {
                Logger.Warn($"Environment dataset {environmentDatasetId} could not be found for add/update environment schema {environmentSchema.SchemaId}");
            }
        }

        public async Task DeleteEnvironmentSchemaAsync(int environmentSchemaId)
        {
            GetByEnvironmentSchemaIdResult getByResult = await GetGlobalDatasetByEnvironmentSchemaIdAsync(environmentSchemaId).ConfigureAwait(false);

            if (getByResult.WasFound())
            {
                getByResult.EnvironmentDataset.EnvironmentSchemas.Remove(getByResult.EnvironmentSchema);

                await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
            }
        }

        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync(int environmentSchemaId, string saidAssetCode)
        {
            GetByEnvironmentSchemaIdResult getByResult = await GetGlobalDatasetByEnvironmentSchemaIdAsync(environmentSchemaId).ConfigureAwait(false);

            if (getByResult.WasFound())
            {
                getByResult.EnvironmentSchema.SchemaSaidAssetCode = saidAssetCode;

                await _elasticDocumentClient.IndexAsync(getByResult.GlobalDataset).ConfigureAwait(false);
            }
            else if (!string.IsNullOrEmpty(saidAssetCode))
            {
                Logger.Warn($"Environment schema {environmentSchemaId} could not be found for add/update SAID asset");
            }
        }
        #endregion

        #region Private
        private async Task<GetByEnvironmentDatasetIdResult> GetGlobalDatasetByEnvironmentDatasetIdAsync(int environmentDatasetId)
        {
            int? globalDatasetId = _datasetContext.Datasets.Where(x => x.DatasetId == environmentDatasetId).Select(x => x.GlobalDatasetId).FirstOrDefault();

            return await GetGlobalDatasetByEnvironmentDatasetIdAsync(globalDatasetId.Value, environmentDatasetId).ConfigureAwait(false);
        }

        private async Task<GetByEnvironmentDatasetIdResult> GetGlobalDatasetByEnvironmentDatasetIdAsync(int globalDatasetId, int environmentDatasetId)
        {
            GetByEnvironmentDatasetIdResult getByResult = new GetByEnvironmentDatasetIdResult
            {
                GlobalDataset = await _elasticDocumentClient.GetByIdAsync<GlobalDataset>(globalDatasetId).ConfigureAwait(false)
            };

            if (getByResult.GlobalDataset != null)
            {
                getByResult.EnvironmentDataset = getByResult.GlobalDataset.EnvironmentDatasets.FirstOrDefault(x => x.DatasetId == environmentDatasetId);
            }

            return getByResult;
        }

        private async Task<GetByEnvironmentSchemaIdResult> GetGlobalDatasetByEnvironmentSchemaIdAsync(int environmentSchemaId)
        {
            int? globalDatasetId = _datasetContext.DatasetFileConfigs.Where(x => x.Schema.SchemaId == environmentSchemaId).Select(x => x.ParentDataset.GlobalDatasetId).FirstOrDefault(); 
            
            GetByEnvironmentSchemaIdResult getByResult = new GetByEnvironmentSchemaIdResult
            {
                GlobalDataset = await _elasticDocumentClient.GetByIdAsync<GlobalDataset>(globalDatasetId).ConfigureAwait(false)
            };

            if (getByResult.GlobalDataset != null)
            {
                getByResult.EnvironmentDataset = getByResult.GlobalDataset.EnvironmentDatasets.FirstOrDefault(x => x.EnvironmentSchemas.Any(s => s.SchemaId == environmentSchemaId));

                if (getByResult.EnvironmentDataset != null)
                {
                    getByResult.EnvironmentSchema = getByResult.EnvironmentDataset.EnvironmentSchemas.FirstOrDefault(x => x.SchemaId == environmentSchemaId);
                }
            }

            return getByResult;
        }
        #endregion
    }
}
