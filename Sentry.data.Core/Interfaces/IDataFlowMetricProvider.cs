using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface IDataFlowMetricProvider
    {
        List<DataFlowMetric> GetDataFlowMetrics(DataFlowMetricSearchDto dto);

        ElasticResult<DataFlowMetric> GetAllTotalFilesByDataset(int datasetId);

        ElasticResult<DataFlowMetric> GetAllTotalFilesBySchema(int schemaId);

        ElasticResult<DataFlowMetric> GetAllTotalFiles();

        ElasticResult<DataFlowMetric> GetAllFailedFilesByDataset(int datasetId);

        ElasticResult<DataFlowMetric> GetAllFailedFilesBySchema(int schemaId);

        ElasticResult<DataFlowMetric> GetAllFailedFiles();

        ElasticResult<DataFlowMetric> GetAllInFlightFilesByDataset(int datasetId);

        ElasticResult<DataFlowMetric> GetAllInFlightFilesBySchema(int schemaId);

        ElasticResult<DataFlowMetric> GetAllInFlightFiles();
    }
}
