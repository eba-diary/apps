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
    }
}
