using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class DataFlowDetailDto : DataFlowDto
    {
        public List<DataFlowStepDto> steps { get; set; }
    }
}
