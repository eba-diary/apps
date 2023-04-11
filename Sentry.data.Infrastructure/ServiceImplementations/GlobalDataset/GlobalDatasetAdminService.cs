using Hangfire;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetAdminService : IGlobalDatasetAdminService
    {
        private readonly IUserService _userService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IDatasetContext _datasetContext;
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly IDataFeatures _dataFeatures;

        public GlobalDatasetAdminService(IUserService userService, IBackgroundJobClient backgroundJobClient, IDatasetContext datasetContext, IGlobalDatasetProvider globalDatasetProvider, IDataFeatures dataFeatures)
        {
            _userService = userService;
            _backgroundJobClient = backgroundJobClient;
            _datasetContext = datasetContext;
            _globalDatasetProvider = globalDatasetProvider;
            _dataFeatures = dataFeatures;
        }

        public async Task<IndexGlobalDatasetsResultDto> IndexGlobalDatasetsAsync(IndexGlobalDatasetsDto indexGlobalDatasetsDto)
        {
            if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                //admins only
                if (_userService.GetCurrentUser().IsAdmin)
                {
                    if (indexGlobalDatasetsDto.IndexAll)
                    {
                        //queue full index to run in background
                        string jobId = _backgroundJobClient.Enqueue<GlobalDatasetAdminService>(x => x.IndexAllGlobalDatasets());
                        return new IndexGlobalDatasetsResultDto { BackgroundJobId = jobId };
                    }
                    else
                    {
                        //process list of global dataset ids
                        return await IndexGlobalDatasetsByIdsAsync(indexGlobalDatasetsDto.GlobalDatasetIds);
                    }
                }
                else
                {
                    throw new ResourceForbiddenException(_userService.GetCurrentUser().AssociateId, "Admin", "IndexGlobalDatasets");
                }
            }
            else
            {
                throw new ResourceFeatureNotEnabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "IndexGlobalDatasets");
            }
        }

        #region Private
        [DisplayName("Index Global Datasets")]
        [AutomaticRetry(Attempts = 0)]
        private string IndexAllGlobalDatasets()
        {
            throw new NotImplementedException();
        }

        private async Task<IndexGlobalDatasetsResultDto> IndexGlobalDatasetsByIdsAsync(List<int> globalDatasetIds)
        {
            List<int> deleteGlobalDatasetIds = new List<int>();
            List<GlobalDataset> indexGlobalDatasets = new List<GlobalDataset>();

            var globalDatasetGroups = _datasetContext.Datasets.Where(x => x.GlobalDatasetId.HasValue && globalDatasetIds.Contains(x.GlobalDatasetId.Value)).GroupBy(x => x.GlobalDatasetId).ToList();

            foreach (var globalDatasetGroup in globalDatasetGroups)
            {
                if (globalDatasetGroup.All(x => x.ObjectStatus != ObjectStatusEnum.Active))
                {
                    deleteGlobalDatasetIds.Add(globalDatasetGroup.Key.Value);
                }
                else
                {
                    GlobalDataset globalDataset = BuildGlobalDataset(globalDatasetGroup.ToList());
                    indexGlobalDatasets.Add(globalDataset);
                }
            }

            Task[] asyncRequests = new Task[]
            {
                _globalDatasetProvider.DeleteGlobalDatasetsAsync(deleteGlobalDatasetIds),
                _globalDatasetProvider.AddUpdateGlobalDatasetsAsync(indexGlobalDatasets)
            };

            await Task.WhenAll(asyncRequests);

            return new IndexGlobalDatasetsResultDto
            {
                IndexCount = indexGlobalDatasets.Count,
                DeleteCount = deleteGlobalDatasetIds.Count
            };
        }

        private GlobalDataset BuildGlobalDataset(List<Dataset> datasets)
        {
            GlobalDataset globalDataset = new GlobalDataset
            {
                GlobalDatasetId = datasets.First().GlobalDatasetId.Value,
                DatasetName = datasets.First().DatasetName,
                DatasetSaidAssetCode = datasets.First().Asset.SaidKeyCode,
                EnvironmentDatasets = new List<EnvironmentDataset>()
            };

            foreach (Dataset dataset in datasets.Where(x => x.ObjectStatus == ObjectStatusEnum.Active).ToList())
            {
                List<FileSchema> schemas = _datasetContext.DatasetFileConfigs.Where(x => x.ParentDataset.DatasetId == dataset.DatasetId && x.ObjectStatus == ObjectStatusEnum.Active).Select(x => x.Schema).ToList();
                List<DataFlow> flows = _datasetContext.DataFlow.Where(x => x.DatasetId == dataset.DatasetId && x.ObjectStatus == ObjectStatusEnum.Active).ToList();

                List<EnvironmentSchema> environmentSchemas = new List<EnvironmentSchema>();

                foreach (var schema in schemas)
                {
                    environmentSchemas.Add(new EnvironmentSchema
                    {
                        SchemaId = schema.SchemaId,
                        SchemaName = schema.Name,
                        SchemaDescription = schema.Description,
                        SchemaSaidAssetCode = flows.FirstOrDefault(x => x.SchemaId == schema.SchemaId)?.SaidKeyCode
                    });
                }

                EnvironmentDataset environmentDataset = dataset.ToEnvironmentDataset();
                globalDataset.EnvironmentDatasets.Add(environmentDataset);
            }

            return globalDataset;
        }
        #endregion
    }
}
