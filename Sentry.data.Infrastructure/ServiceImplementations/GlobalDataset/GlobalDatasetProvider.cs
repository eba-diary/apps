using Sentry.Common.Logging;
using Sentry.data.Core;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetProvider : IGlobalDatasetProvider
    {
        private readonly IElasticContext _elasticContext;

        public GlobalDatasetProvider(IElasticContext elasticContext)
        {
            _elasticContext = elasticContext;
        }

        #region Global Dataset
        public async Task AddUpdateGlobalDatasetAsync(GlobalDataset globalDataset)
        {
            await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
        }
        #endregion

        #region Environment Dataset
        public async Task AddUpdateEnvironmentDatasetAsync(int globalDatasetId, EnvironmentDataset environmentDataset)
        {
            GlobalDataset globalDataset = await _elasticContext.GetByIdAsync<GlobalDataset>(globalDatasetId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset existingDataset = globalDataset.EnvironmentDatasets.FirstOrDefault(x => x.DatasetId == environmentDataset.DatasetId);
                if (existingDataset != null)
                {
                    environmentDataset.EnvironmentSchemas = existingDataset.EnvironmentSchemas;
                    globalDataset.EnvironmentDatasets.Remove(existingDataset);
                }

                globalDataset.EnvironmentDatasets.Add(environmentDataset);

                await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
            }
            else
            {
                Logger.Warn($"Global dataset {globalDatasetId} was not found");
            }
        }

        public async Task DeleteEnvironmentDatasetAsync(int environmentDatasetId)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset existingDataset = globalDataset.EnvironmentDatasets.First(x => x.DatasetId == environmentDatasetId);
                globalDataset.EnvironmentDatasets.Remove(existingDataset);

                if (!globalDataset.EnvironmentDatasets.Any())
                {
                    //delete whole global dataset if no environmnet datasets left
                    await _elasticContext.DeleteByIdAsync<GlobalDataset>(globalDataset.GlobalDatasetId).ConfigureAwait(false);
                }
                else
                {
                    await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
                }
            }
        }

        public async Task AddEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First(x => x.DatasetId == environmentDatasetId);
                if (!environmentDataset.FavoriteUserIds.Contains(favoriteUserId))
                {
                    environmentDataset.FavoriteUserIds.Add(favoriteUserId);

                    await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
                }
            }
        }

        public async Task RemoveEnvironmentDatasetFavoriteUserIdAsync(int environmentDatasetId, string favoriteUserId)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First(x => x.DatasetId == environmentDatasetId);
                if (environmentDataset.FavoriteUserIds.Contains(favoriteUserId))
                {
                    environmentDataset.FavoriteUserIds.Remove(favoriteUserId);

                    await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
                }
            }
        }
        #endregion

        #region Environment Schema
        public async Task AddUpdateEnvironmentSchemaAsync(int environmentDatasetId, EnvironmentSchema environmentSchema)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentDatasetIdAsync(environmentDatasetId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First(x => x.DatasetId == environmentDatasetId);

                EnvironmentSchema existingSchema = environmentDataset.EnvironmentSchemas.FirstOrDefault(x => x.SchemaId == environmentSchema.SchemaId);
                if (existingSchema != null)
                {
                    environmentDataset.EnvironmentSchemas.Remove(existingSchema);
                }

                environmentDataset.EnvironmentSchemas.Add(environmentSchema);

                await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
            }
            else
            {
                Logger.Warn($"Global dataset was not found for environment dataset {environmentDatasetId}");
            }
        }

        public async Task DeleteEnvironmentSchemaAsync(int environmentSchemaId)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentSchemaIdAsync(environmentSchemaId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First(x => x.EnvironmentSchemas.Any(s => s.SchemaId == environmentSchemaId));
                EnvironmentSchema existingSchema = environmentDataset.EnvironmentSchemas.First(x => x.SchemaId == environmentSchemaId);

                environmentDataset.EnvironmentSchemas.Remove(existingSchema);

                await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
            }
        }

        public async Task AddUpdateEnvironmentSchemaSaidAssetCodeAsync(int environmentSchemaId, string saidAssetCode)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentSchemaIdAsync(environmentSchemaId).ConfigureAwait(false);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.EnvironmentDatasets.First(x => x.EnvironmentSchemas.Any(s => s.SchemaId == environmentSchemaId));
                EnvironmentSchema existingSchema = environmentDataset.EnvironmentSchemas.First(x => x.SchemaId == environmentSchemaId);

                existingSchema.SchemaSaidAssetCode = saidAssetCode;

                await _elasticContext.IndexAsync(globalDataset).ConfigureAwait(false);
            }
            else
            {
                Logger.Warn($"Global dataset was not found for environment schema {environmentSchemaId}");
            }
        }
        #endregion

        #region Private
        private async Task<GlobalDataset> GetGlobalDatasetByEnvironmentDatasetIdAsync(int environmentDatasetId)
        {
            ElasticResult<GlobalDataset> elasticResult = await _elasticContext.SearchAsync<GlobalDataset>(x => x
                .Query(q => q
                    .Nested(n => n
                        .Path(p => p.EnvironmentDatasets)
                        .Query(nq => nq
                            .Term(t => t.EnvironmentDatasets.First().DatasetId, environmentDatasetId)
                         )
                    )
                )
                .Size(1)
            ).ConfigureAwait(false);

            return elasticResult.Documents.FirstOrDefault();
        }

        private async Task<GlobalDataset> GetGlobalDatasetByEnvironmentSchemaIdAsync(int environmentSchemaId)
        {
            ElasticResult<GlobalDataset> elasticResult = await _elasticContext.SearchAsync<GlobalDataset>(x => x
                .Query(q => q
                    .Nested(n => n
                        .Path(p => p.EnvironmentDatasets)
                        .Query(nq => nq
                            .Nested(dn => dn
                                .Path(p => p.EnvironmentDatasets.First().EnvironmentSchemas)
                                .Query(dnq => dnq
                                    .Term(t => t.EnvironmentDatasets.First().EnvironmentSchemas.First().SchemaId, environmentSchemaId)
                                 )
                            )
                         )
                    )
                )
                .Size(1)
            ).ConfigureAwait(false);

            return elasticResult.Documents.FirstOrDefault();
        }
        #endregion
    }
}
