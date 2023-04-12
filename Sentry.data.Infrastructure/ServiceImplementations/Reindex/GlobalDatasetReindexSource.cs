using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetReindexSource : IReindexSource<GlobalDataset>
    {
        private readonly IDatasetContext _datasetContext;
        private Dictionary<int, List<int>> _globalDatasetIdGroups;
        private readonly int _batchSize = 50;
        private int _pageNumber;

        private Dictionary<int, List<int>> GlobalDatasetIdGroups
        {
            get
            {
                if (_globalDatasetIdGroups == null)
                {
                    _globalDatasetIdGroups = _datasetContext.Datasets.Where(x => x.DatasetType == DataEntityCodes.DATASET && x.ObjectStatus == ObjectStatusEnum.Active)
                        .Select(x => new { x.DatasetId, x.GlobalDatasetId })
                        .GroupBy(x => x.GlobalDatasetId)
                        .ToDictionary(x => x.Key.Value, y => y.Select(s => s.DatasetId).ToList());
                }

                return _globalDatasetIdGroups;
            }
        }

        public GlobalDatasetReindexSource(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public bool TryGetNextDocuments(out List<GlobalDataset> documents)
        {
            documents = new List<GlobalDataset>();

            //get next batch to build documents with
            var batch = GlobalDatasetIdGroups.Skip(_pageNumber * _batchSize).Take(_batchSize).ToList();

            if (batch.Any())
            {
                //use dataset ids to retrieve datasets from database more efficiently
                List<int> batchDatasetIds = batch.SelectMany(x => x.Value).ToList();

                //Pre-retrieve all needed entities from database for batch
                List<Dataset> batchDatasets = _datasetContext.Datasets.Where(x => batchDatasetIds.Contains(x.DatasetId)).ToList();

                //get schemas by dataset ids in batch
                List<KeyValuePair<int, FileSchema>> schemas = _datasetContext.DatasetFileConfigs
                    .Where(x => batchDatasetIds.Contains(x.ParentDataset.DatasetId) && x.ObjectStatus == ObjectStatusEnum.Active)
                    .Select(x => new KeyValuePair<int, FileSchema>(x.ParentDataset.DatasetId, x.Schema))
                    .ToList();

                //get dataflows by dataset ids in batch
                List<DataFlow> dataFlows = _datasetContext.DataFlow.Where(x => batchDatasetIds.Contains(x.DatasetId) && x.ObjectStatus == ObjectStatusEnum.Active).ToList();

                foreach (var globalDatasetIdGroup in batch)
                {
                    //get datasets for global dataset
                    List<Dataset> datasets = batchDatasets.Where(x => x.GlobalDatasetId == globalDatasetIdGroup.Key).ToList();

                    //get schemas for the datasets for global dataset
                    List<KeyValuePair<int, FileSchema>> datasetIdSchemas = schemas.Where(x => globalDatasetIdGroup.Value.Contains(x.Key)).ToList();

                    //get dataflows for the datasets for global dataset
                    List<DataFlow> datasetDataFlows = dataFlows.Where(x => globalDatasetIdGroup.Value.Contains(x.DatasetId)).ToList();

                    GlobalDataset globalDataset = datasets.ToGlobalDataset(datasetIdSchemas, datasetDataFlows);

                    documents.Add(globalDataset);
                }

                _pageNumber++;
            }

            return documents.Any();
        }
    }
}
