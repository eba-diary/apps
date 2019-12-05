using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class DataFlowDetailDto : DataFlowDto
    {
        public List<DataFlowStepDto> steps { get; set; }
    }
}
