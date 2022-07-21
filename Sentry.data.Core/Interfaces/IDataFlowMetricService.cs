using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface IDataFlowMetricService
    {
        List<DataFlowMetric> GetDataFlowMetricEntities(DataFlowMetricSearchDto dto);
        List<DataFlowMetricDto> GetMetricList(List<DataFlowMetric> entityList);
        List<DataFileFlowMetricsDto> GetFileMetricGroups(List<DataFlowMetricDto> dtoList);
    }
}
