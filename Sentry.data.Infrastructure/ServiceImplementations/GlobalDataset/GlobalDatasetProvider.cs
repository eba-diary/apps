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
            await _elasticContext.IndexAsync(globalDataset);
        }
        #endregion

        #region Environment Dataset
        public async Task AddUpdateEnvironmentDatasetAsync(int globalDatasetId, EnvironmentDataset environmentDataset)
        {
            GlobalDataset globalDataset = await _elasticContext.GetDocumentAsync<GlobalDataset>(globalDatasetId);

            if (globalDataset != null)
            {
                EnvironmentDataset existingDataset = globalDataset.Datasets.FirstOrDefault(x => x.DatasetId == environmentDataset.DatasetId);
                if (existingDataset != null)
                {
                    globalDataset.Datasets.Remove(existingDataset);
                }

                globalDataset.Datasets.Add(environmentDataset);

                await _elasticContext.IndexAsync(globalDataset);
            }
            else
            {
                Logger.Warn($"Global dataset {globalDatasetId} was not found");
            }
        }

        public async Task DeleteEnvironmentDatasetAsync(int environmentDatasetId)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentDatasetId(environmentDatasetId);

            if (globalDataset != null)
            {
                EnvironmentDataset existingDataset = globalDataset.Datasets.First(x => x.DatasetId == environmentDatasetId);
                globalDataset.Datasets.Remove(existingDataset);

                await _elasticContext.IndexAsync(globalDataset);
            }
        }
        #endregion

        #region Environment Schema
        public async Task AddUpdateEnvironmentSchemaAsync(int environmentDatasetId, EnvironmentSchema environmentSchema)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentDatasetId(environmentDatasetId);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.Datasets.First(x => x.DatasetId == environmentDatasetId);

                EnvironmentSchema existingSchema = environmentDataset.Schemas.FirstOrDefault(x => x.SchemaId == environmentSchema.SchemaId);
                if (existingSchema != null)
                {
                    environmentDataset.Schemas.Remove(existingSchema);
                }

                environmentDataset.Schemas.Add(environmentSchema);

                await _elasticContext.IndexAsync(globalDataset);
            }
            else
            {
                Logger.Warn($"Global dataset was not found for environment dataset {environmentDatasetId}");
            }
        }

        public async Task DeleteEnvironmentSchemaAsync(int environmentSchemaId)
        {
            GlobalDataset globalDataset = await GetGlobalDatasetByEnvironmentSchemaId(environmentSchemaId);

            if (globalDataset != null)
            {
                EnvironmentDataset environmentDataset = globalDataset.Datasets.First(x => x.Schemas.Any(s => s.SchemaId == environmentSchemaId));
                EnvironmentSchema existingSchema = environmentDataset.Schemas.First(x => x.SchemaId == environmentSchemaId);
                environmentDataset.Schemas.Remove(existingSchema);

                await _elasticContext.IndexAsync(globalDataset);
            }
        }
        #endregion

        #region Private
        private async Task<GlobalDataset> GetGlobalDatasetByEnvironmentDatasetId(int environmentDatasetId)
        {
            ElasticResult<GlobalDataset> elasticResult = await _elasticContext.SearchAsync<GlobalDataset>(x => x
                .Query(q => q
                    .Nested(n => n
                        .Path(p => p.Datasets)
                        .Query(nq => nq
                            .Term(t => t.Datasets.First().DatasetId, environmentDatasetId)
                         )
                    )
                )
                .Size(1)
            );

            return elasticResult.Documents.FirstOrDefault();
        }

        private async Task<GlobalDataset> GetGlobalDatasetByEnvironmentSchemaId(int environmentSchemaId)
        {
            ElasticResult<GlobalDataset> elasticResult = await _elasticContext.SearchAsync<GlobalDataset>(x => x
                .Query(q => q
                    .Nested(n => n
                        .Path(p => p.Datasets)
                        .Query(nq => nq
                            .Nested(dn => dn
                                .Path(p => p.Datasets.First().Schemas)
                                .Query(dnq => dnq
                                    .Term(t => t.Datasets.First().Schemas.First().SchemaId, environmentSchemaId)
                                 )
                            )
                         )
                    )
                )
                .Size(1)
            );

            return elasticResult.Documents.FirstOrDefault();
        }
        #endregion
    }
}
