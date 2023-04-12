using Hangfire;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
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
        private readonly IReindexService _reindexService;
        private readonly IDataFeatures _dataFeatures;

        public GlobalDatasetAdminService(IUserService userService, 
            IBackgroundJobClient backgroundJobClient, 
            IDatasetContext datasetContext, 
            IGlobalDatasetProvider globalDatasetProvider, 
            IReindexService reindexService, 
            IDataFeatures dataFeatures)
        {
            _userService = userService;
            _backgroundJobClient = backgroundJobClient;
            _datasetContext = datasetContext;
            _globalDatasetProvider = globalDatasetProvider;
            _reindexService = reindexService;
            _dataFeatures = dataFeatures;
        }

        public async Task<IndexGlobalDatasetsResultDto> IndexGlobalDatasetsAsync(IndexGlobalDatasetsDto indexGlobalDatasetsDto)
        {
            if (!_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                throw new ResourceFeatureDisabledException(nameof(_dataFeatures.CLA4789_ImprovedSearchCapability), "IndexGlobalDatasets");
            }

            //admins only
            if (!_userService.GetCurrentUser().IsAdmin)
            {
                throw new ResourceForbiddenException(_userService.GetCurrentUser().AssociateId, "Admin", "IndexGlobalDatasets");
            }

            if (indexGlobalDatasetsDto.IndexAll)
            {
                //queue full index to run in background
                string jobId = _backgroundJobClient.Enqueue(() => _reindexService.ReindexAsync());
                return new IndexGlobalDatasetsResultDto { BackgroundJobId = jobId };
            }
            else
            {
                //process list of global dataset ids
                return await IndexGlobalDatasetsByIdsAsync(indexGlobalDatasetsDto.GlobalDatasetIds);
            }
        }

        #region Private
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
                    GlobalDataset globalDataset = BuildGlobalDataset(globalDatasetGroup.Where(x => x.ObjectStatus == ObjectStatusEnum.Active).ToList());
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
            List<int> datasetIds = datasets.Select(x => x.DatasetId).ToList();

            List<KeyValuePair<int, FileSchema>> datasetIdSchemas = _datasetContext.DatasetFileConfigs
                .Where(x => datasetIds.Contains(x.ParentDataset.DatasetId) && x.ObjectStatus == ObjectStatusEnum.Active)
                .Select(x => new KeyValuePair<int, FileSchema>(x.ParentDataset.DatasetId, x.Schema))
                .ToList();

            List<DataFlow> dataFlows = _datasetContext.DataFlow.Where(x => datasetIds.Contains(x.DatasetId) && x.ObjectStatus == ObjectStatusEnum.Active).ToList();

            GlobalDataset globalDataset = datasets.ToGlobalDataset(datasetIdSchemas, dataFlows);

            return globalDataset;
        }
        #endregion
    }
}
