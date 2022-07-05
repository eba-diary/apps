using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface IDataFlowMetricService
    {
        List<DataFlowMetricEntity> GetDataFlowMetricEntities(DataFlowMetricSearchDto dto);
        List<DataFlowMetricDto> GetMetricList(List<DataFlowMetricEntity> entityList);
        List<DataFileFlowMetricsDto> GetFileMetricGroups(List<DataFlowMetricDto> dtoList);
    }
}
