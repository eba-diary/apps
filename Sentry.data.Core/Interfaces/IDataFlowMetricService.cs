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
        long GetAllTotalFilesCount();
        List<DatasetProcessActivityDto> GetAllTotalFiles();
        List<DatasetFileProcessActivityDto> GetAllTotalFilesBySchema(int schemaId);
        List<SchemaProcessActivityDto> GetAllTotalFilesByDataset(int datasetId);
        long GetAllFailedFilesCount();
        List<DatasetProcessActivityDto> GetAllFailedFiles();
        List<DatasetFileProcessActivityDto> GetAllFailedFilesBySchema(int schemaId);
        List<SchemaProcessActivityDto> GetAllFailedFilesByDataset(int datasetId);
        long GetAllInFlightFilesCount();
        List<DatasetProcessActivityDto> GetAllInFlightFiles();
        List<DatasetFileProcessActivityDto> GetAllInFlightFilesBySchema(int schemaId);
        List<SchemaProcessActivityDto> GetAllInFlightFilesByDataset(int datasetId);
    }
}
