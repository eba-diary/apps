using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface IDataFlowMetricService
    {
        List<DataFileFlowMetricsDto> GetFileMetricGroups(DataFlowMetricSearchDto searchDto);
        List<DatasetProcessActivityDto> GetAllTotalFiles();
        List<DatasetFileProcessActivityDto> GetAllTotalFilesBySchema(int schemaId);
        List<SchemaProcessActivityDto> GetAllTotalFilesByDataset(int datasetId);
    }
}
