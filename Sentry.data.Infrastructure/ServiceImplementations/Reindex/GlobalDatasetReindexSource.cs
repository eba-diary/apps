using Org.BouncyCastle.Math.EC.Rfc7748;
using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class GlobalDatasetReindexSource : IReindexSource<GlobalDataset>
    {
        private readonly IDatasetContext _datasetContext;
        private Dictionary<int, List<int>> _globalDatasetIdGroups;
        private int _batchSize = 50;
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

            var batch = GlobalDatasetIdGroups.Skip(_pageNumber * _batchSize).Take(_batchSize).ToList();

            List<int> batchDatasetIds = batch.SelectMany(x => x.Value).ToList();

            List<Dataset> datasets = _datasetContext.Datasets.Where(x => batchDatasetIds.Contains(x.DatasetId)).ToList();

            var schemas = _datasetContext.DatasetFileConfigs.Where(x => batchDatasetIds.Contains(x.ParentDataset.DatasetId) && x.ObjectStatus == ObjectStatusEnum.Active)
                .Select(x => new { x.ParentDataset.DatasetId, x.Schema }).ToList();

            List<DataFlow> dataFlows = _datasetContext.DataFlow.Where(x => batchDatasetIds.Contains(x.DatasetId) && x.ObjectStatus == ObjectStatusEnum.Active).ToList();

            foreach (int globalDatasetId in batch.Select(x => x.Key).ToList())
            {
                List<Dataset> environmentDatasets = datasets.Where(x => x.GlobalDatasetId == globalDatasetId).ToList();

                GlobalDataset globalDataset = new GlobalDataset
                {
                    GlobalDatasetId = globalDatasetId,
                    DatasetName = environmentDatasets.First().DatasetName,
                    DatasetSaidAssetCode = environmentDatasets.First().Asset.SaidKeyCode,
                    EnvironmentDatasets = new List<EnvironmentDataset>()
                };

                foreach (Dataset dataset in environmentDatasets)
                {
                    List<FileSchema> datasetSchemas = schemas.Where(x => x.DatasetId == dataset.DatasetId).Select(x => x.Schema).ToList();
                    List<DataFlow> flows = dataFlows.Where(x => x.DatasetId == dataset.DatasetId).ToList();

                    List<EnvironmentSchema> environmentSchemas = new List<EnvironmentSchema>();

                    foreach (var datasetSchema in datasetSchemas)
                    {
                        environmentSchemas.Add(new EnvironmentSchema
                        {
                            SchemaId = datasetSchema.SchemaId,
                            SchemaName = datasetSchema.Name,
                            SchemaDescription = datasetSchema.Description,
                            SchemaSaidAssetCode = flows.FirstOrDefault(x => x.SchemaId == datasetSchema.SchemaId)?.SaidKeyCode
                        });
                    }

                    EnvironmentDataset environmentDataset = dataset.ToEnvironmentDataset();
                    globalDataset.EnvironmentDatasets.Add(environmentDataset);
                }

                documents.Add(globalDataset);
            }

            return documents.Any();
        }
    }
}
